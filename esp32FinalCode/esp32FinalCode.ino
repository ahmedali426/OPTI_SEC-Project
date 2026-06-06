#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <Keypad.h>
#include <Adafruit_Fingerprint.h>
#include <Firebase_ESP_Client.h>

// --- 1. Network & Firebase Configurations ---
#define WIFI_SSID         "A13"
#define WIFI_PASSWORD     "123456789*"
#define API_KEY           "AlzaSyAJxudhGGSXfbpLjygTE4OZX_tCbZw2QWQ"
#define DATABASE_URL      "https://opti-sec-default-rtdb.europe-west1.firebasedatabase.app/"
#define USER_EMAIL        "ahmedali123@gmail.com"
#define USER_PASSWORD     "ahmedali123"

// --- 2. Backend API Configurations ---
const char* backend_password_url = "https://opti-sec.runasp.net/api/Device/validatepassword";
const char* backend_verify_fingerprint_url = "https://opti-sec.runasp.net/api/Device/verify-fingerprint";
const int currentGateId          = 43;
const char* currentDeviceId      = "device13";

// --- 3. Hardware Pin Mapping ---
#define LASER_RELAY   18  // ريلاي الليزر (Active Low)
#define BUZZER        19  // 🚨 ريلاي البازر مخرج رقم 19 وشغال Active Low زي الليزر
#define LDR_PIN       34  

// --- 4. Peripherals Configurations ---
LiquidCrystal_I2C lcd(0x27, 16, 2);
HardwareSerial mySerial(2); 
Adafruit_Fingerprint finger = Adafruit_Fingerprint(&mySerial);

// --- 5. 4x4 Keypad Matrix Configuration (Fixed single quotes) ---
const byte ROWS = 4;
const byte COLS = 4;
char keys[ROWS][COLS] = {
  {'1','2','3','A'},
  {'4','5','6','B'},
  {'7','8','9','C'},
  {'*','0','#','D'}
};
byte rowPins[ROWS] = {13, 12, 14, 27};
byte colPins[COLS]  = {26, 25, 33, 32};
Keypad keypad = Keypad(makeKeymap(keys), rowPins, colPins, ROWS, COLS);

// --- 6. Global Architecture Variables ---
String currentPIN = "";
int remainingAttempts = 4;
uint8_t enrollID = 1;

String currentSessionToken    = "";
String currentNextStep        = "";
bool currentBuzzerStatus      = false;
bool currentGateStatus        = false;

String currentPasswordStatus  = "";
bool isGateLockedOut          = false;
bool presentationBypassActive = false;
bool silentAlarmActive        = false;

// تايمرات الأمان والإنذارات
bool isIntrusionAlarmActive = false;
unsigned long intrusionAlarmStartTime = 0;
const unsigned long intrusionAlarmDuration = 30UL * 1000UL;
unsigned long lockOutStartTime = 0;
const unsigned long lockOutDuration = 20UL * 1000UL;

unsigned long lastBuzzerToggle = 0;
bool buzzerState = false;
unsigned long lastWifiCheck = 0;
int ldrThreshold = 3400;

unsigned long lastFirebaseCheck = 0;
const unsigned long firebaseInterval = 1000; 

// كائنات فايربيز
FirebaseData f1;
FirebaseAuth auth;
FirebaseConfig config;

enum GateState { GATE_IDLE, MFA_SEQUENCE };
GateState currentState = GATE_IDLE;

// --- Function Prototypes ---
void checkPerimeterSecurity();
void processKeypadInput();
void validatePassword(String enteredPassword);
void runMFASequence();
void releaseLock();
void triggerEmergencyAlarm();
void handleLockOut();
void handleIntrusionAlarm();
void syncAndCheckFirebase();
void updateLCD(String top, String bottom);
void triggerBuzzer(int duration);
void maintainWifi();
bool enrollFinger(uint8_t id);
void sendDataToFirebase(String token, String nextStep);
void setMemberFingerprint(int memberId, String fingerprintTemplate);

// ============================================================
// INITIALIZATION (SETUP)
// ============================================================
void setup() {
  Serial.begin(115200);

  pinMode(LASER_RELAY, OUTPUT);
  pinMode(BUZZER,      OUTPUT);

  // 🔒 ضبط الحالة الابتدائية الآمنة (الريلاي طافي ومفصول):
  // بما إن المنطق مقلوب (Active Low)، كتابة HIGH تعني إطفاء الريلاي تماماً عند بدء التشغيل
  digitalWrite(LASER_RELAY, HIGH); 
  digitalWrite(BUZZER,      HIGH); // البازر طافي في البداية

  Wire.begin(22, 23);
  lcd.init();
  lcd.backlight();
  updateLCD("  SYSTEM ARMED  ", "Code:");

  mySerial.begin(57600, SERIAL_8N1, 16, 17);
  finger.begin(57600);
  if (finger.verifyPassword()) {
    Serial.println("[Hardware] Fingerprint sensor found!");
  }

  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  Serial.print("[Network] Connecting to Wi-Fi");
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print("."); delay(300);
  }
  Serial.println("\n[Network] Connected!");

  config.api_key = API_KEY;
  auth.user.email = USER_EMAIL;
  auth.user.password = USER_PASSWORD;
  config.database_url = DATABASE_URL;
  
  Firebase.reconnectNetwork(true); 
  f1.setBSSLBufferSize(4096, 1024);
  f1.setResponseSize(2048);
  Firebase.begin(&config, &auth);

  Serial.println("\n>>> System Ready! <<<");
  lcd.clear();
}

// ============================================================
// MAIN SYSTEM LOOP
// ============================================================
void loop() {
  maintainWifi();

  if (millis() - lastFirebaseCheck >= firebaseInterval) {
    lastFirebaseCheck = millis();
    syncAndCheckFirebase();
  }

  if (isGateLockedOut) {
    handleLockOut();
    return; 
  }

  if (isIntrusionAlarmActive) {
    handleIntrusionAlarm();
  } else {
    checkPerimeterSecurity();
  }

  if (currentState == GATE_IDLE) {
    processKeypadInput();
  }
}

// ============================================================
// 📡 المراقبة والمزامنة الذكية مع الفايربيز
// ============================================================
void syncAndCheckFirebase() {
  if (!Firebase.ready()) return;

  if (Firebase.RTDB.getJSON(&f1, "/")) {
    String jsonStr = f1.jsonString();
    DynamicJsonDocument doc(1024);
    deserializeJson(doc, jsonStr);

    currentBuzzerStatus = doc["BuzzerStatus"].as<bool>();
    currentGateStatus   = doc["GateStatus"].as<bool>();
    currentNextStep     = doc["nextStep"].as<String>();
    currentSessionToken = doc["sessionToken"].as<String>();

    // 🛑 1. إيقاف البازر فوراً (كتابة HIGH لإيقاف ريلاي الـ Active Low)
    if (currentBuzzerStatus == true) {
      Serial.println("[Firebase Command] Stop Buzzer requested.");
      isIntrusionAlarmActive = false; 
      isGateLockedOut = false;        
      digitalWrite(BUZZER, HIGH);       // إطفاء ريلاي البازر العكسي
      lcd.clear();
      currentState = GATE_IDLE;
      Firebase.RTDB.setBool(&f1, "/BuzzerStatus", false);
    }

    // 🔓 2. فتح البوابة وفصل الليزر والبازر 5 ثواني
    if (currentGateStatus == true) {
      Serial.println("[Firebase Command] Remote Gate Open requested.");
      updateLCD("Remote Unlock", "Gate Opening...");
      releaseLock(); 
      Firebase.RTDB.setBool(&f1, "/GateStatus", false);
    }

    if (currentNextStep == "CaptureFingerprint" && currentState == GATE_IDLE && !isGateLockedOut) {
      Serial.println("\n[Firebase Trigger] nextStep is CaptureFingerprint. Starting Biometric verification...");
      currentState = MFA_SEQUENCE;
      runMFASequence(); 
    }
  }
}

// ============================================================
// KEYPAD LOGIC
// ============================================================
void processKeypadInput() {
  char key = keypad.getKey();
  if (!key) {
    if (!isIntrusionAlarmActive && currentPIN.length() == 0) {
      lcd.setCursor(0, 0); lcd.print("  ENTER PIN:   ");
      lcd.setCursor(0, 1); lcd.print("                "); 
    }
    return;
  }
  // صوت تكة خفيفة عند الضغط (تشغيل ثانية ثم إطفاء)
  digitalWrite(BUZZER, LOW); delay(30); digitalWrite(BUZZER, HIGH);

  if (key == 'A') {
    if (currentPIN.length() > 0) enrollID = currentPIN.toInt();
    else enrollID = 1;
    currentPIN = "";
    if (enrollID == 0) enrollID = 1;
    updateLCD("ENROLL MODE", "ID #" + String(enrollID));
    delay(1000);
    enrollFinger(enrollID);

  } else if (key == '#') {
    validatePassword(currentPIN);
    currentPIN = "";
  } else if (key == '*') {
    currentPIN = ""; lcd.clear();
  } else {
    if (currentPIN.length() == 0) {
      lcd.clear();
      lcd.setCursor(0, 0); lcd.print("  ENTER PIN:   ");
    }
    if (currentPIN.length() < 4) {
      currentPIN += key;
      lcd.setCursor(5 + currentPIN.length(), 1); lcd.print("*");
    }
  }
}

// ============================================================
// STAGE 1: Real API Password Validation
// ============================================================
void validatePassword(String enteredPassword) {
  if (WiFi.status() != WL_CONNECTED) return;

  WiFiClientSecure client; client.setInsecure(); HTTPClient http;
  http.begin(client, backend_password_url);
  http.addHeader("Content-Type", "application/json");

  StaticJsonDocument<200> requestDoc;
  requestDoc["gateId"]    = currentGateId;
  requestDoc["password"]  = enteredPassword;
  requestDoc["deviceId"]  = currentDeviceId;
  requestDoc["timestamp"] = "2026-05-31T09:26:00Z";

  String requestBody; serializeJson(requestDoc, requestBody);
  int httpResponseCode = http.POST(requestBody);

  if (httpResponseCode == 200) {
    String responseBody = http.getString();
    DynamicJsonDocument responseDoc(1024);
    deserializeJson(responseDoc, responseBody);
    
    bool success          = responseDoc["success"];
    currentPasswordStatus = responseDoc["passwordStatus"].as<String>();
    remainingAttempts     = responseDoc["remainingAttempts"].as<int>();

    if (success) {
      currentSessionToken = responseDoc["sessionToken"].as<String>();
      currentNextStep     = responseDoc["nextStep"].as<String>();
      
      updateLCD("PIN VALIDATED", "Wait for Face AI"); 
      delay(1000);
      sendDataToFirebase(currentSessionToken, currentNextStep);
    } 
    else {
      if (remainingAttempts > 0) {
        updateLCD("ACCESS DENIED", "Attempts: " + String(remainingAttempts));
        triggerBuzzer(1000); delay(1000);
      } else {
        triggerEmergencyAlarm();
      }
    }
  }
  http.end();
}

// ============================================================
// STAGE 3: Fingerprint Verification & API POST
// ============================================================
void runMFASequence() {
  updateLCD("Scan Fingerprint", "Place finger...");
  unsigned long timeout = millis();
  int fingerID = -1;

  while (true) {
    if (millis() - timeout > 15000) {
      updateLCD("MFA Timeout", "Restarting...");
      triggerBuzzer(800); delay(1500); 
      currentState = GATE_IDLE; lcd.clear();
      sendDataToFirebase("", "Pending"); 
      return;
    }

    int result = finger.getImage();
    if (result == FINGERPRINT_NOFINGER) { delay(50); continue; }
    if (result != FINGERPRINT_OK) continue;

    result = finger.image2Tz();
    if (result != FINGERPRINT_OK) continue;

    result = finger.fingerSearch();
    if (result == FINGERPRINT_OK) {
      fingerID = finger.fingerID; 
      break;
    } else {
      fingerID = -2; 
      break;
    }
  }

  updateLCD("Validating...", "Fingerprint API");

  WiFiClientSecure client; client.setInsecure(); HTTPClient http;
  http.begin(client, backend_verify_fingerprint_url);
  http.addHeader("Content-Type", "application/json");

  StaticJsonDocument<256> doc;
  doc["sessionToken"]        = currentSessionToken;
  doc["memberId"]            = 1;
  doc["fingerprintTemplate"] = "fin13";
  doc["deviceId"]            = currentDeviceId;

  String payload; serializeJson(doc, payload);
  int httpCode = http.POST(payload);

  if (httpCode == 200) {
    StaticJsonDocument<256> response;
    deserializeJson(response, http.getString());
    bool accessGranted = response["accessGranted"];

    if (accessGranted) {
      updateLCD("Welcome Boss!", "Access Granted");
      sendDataToFirebase(currentSessionToken, "Completed"); 
      releaseLock(); 
    } else {
      updateLCD("MFA MISMATCH", "Access Denied");
      triggerBuzzer(1000); delay(1500);
      sendDataToFirebase("", "Failed");
      currentState = GATE_IDLE; lcd.clear();
    }
  } else {
    updateLCD("MFA Server Error", "Access Denied");
    delay(1500); currentState = GATE_IDLE; lcd.clear();
  }
  http.end();
}

// ============================================================
// STAGE 4: BIOMETRIC ENROLLMENT & API POST SYNC
// ============================================================
bool enrollFinger(uint8_t id) {
  int p = -1;
  updateLCD("Place finger...", "ID #" + String(id));
  while (p != FINGERPRINT_OK) { p = finger.getImage(); if (p == FINGERPRINT_OK) triggerBuzzer(100); }
  p = finger.image2Tz(1);
  if (p != FINGERPRINT_OK) { updateLCD("Convert Error", "Try Again"); delay(1500); return false; }

  updateLCD("Remove finger...", "Please lift");
  delay(2000); p = 0;
  while (p != FINGERPRINT_NOFINGER) { p = finger.getImage(); }

  updateLCD("Place SAME finger", "again..."); p = -1;
  while (p != FINGERPRINT_OK) { p = finger.getImage(); if (p == FINGERPRINT_OK) triggerBuzzer(100); }
  p = finger.image2Tz(2);
  if (p != FINGERPRINT_OK) { updateLCD("Convert Error", "Try Again"); delay(1500); return false; }

  updateLCD("Matching Prints", "Please wait...");
  p = finger.createModel();
  if (p != FINGERPRINT_OK) { updateLCD("Mismatch!", "Retry"); delay(2000); return false; }

  p = finger.storeModel(id);
  if (p == FINGERPRINT_OK) { 
    updateLCD("SUCCESS!", "Stored ID #" + String(id)); 
    triggerBuzzer(100); delay(100); triggerBuzzer(100);
    delay(1500);
    
    updateLCD("SYNCING WITH...", "BACKEND...");
    setMemberFingerprint(id, "fin13");
    
  } else { 
    updateLCD("Store Error", "Failed"); 
    delay(2000); 
    return false; 
  }
  return true;
}

void setMemberFingerprint(int memberId, String fingerprintTemplate) {
    if (WiFi.status() != WL_CONNECTED) return;
    WiFiClientSecure client; client.setInsecure(); HTTPClient http;
    http.begin(client, "https://opti-sec.runasp.net/api/Members/set-fingerprint");
    http.addHeader("Content-Type", "application/json");
    
    StaticJsonDocument<200> doc;
    doc["memberId"]            = memberId;
    doc["fingerprintTemplate"] = fingerprintTemplate;
    
    String requestBody; serializeJson(doc, requestBody);
    int httpResponseCode = http.POST(requestBody);
    
    if (httpResponseCode == 200) {
        updateLCD("SYNC SUCCESS!", "Database Updated"); delay(1500);
    } else {
        updateLCD("SYNC FAILED!", "Code: " + String(httpResponseCode)); delay(1500);
    }
    http.end();
}

// ============================================================
// LOW-LEVEL PERIPHERAL CONTROLS & SECURITY ACTUATORS
// ============================================================
void releaseLock() {
  // فتح قفل البوابة وفصل خط الليزر والبازر تماماً لمدة 5 ثواني
  // في الـ Active Low: كتابة HIGH تعني تشغيل ريلاي البوابة، وفصل ريلاي الليزر والبازر.
  digitalWrite(LASER_RELAY, HIGH); 
  digitalWrite(BUZZER,      HIGH); // إطفاء ريلاي البازر فوراً
  isIntrusionAlarmActive = false; 

  updateLCD("WELCOME!", "GATE OPEN (5s)");
  delay(5000);                     

  // بعد الـ 5 ثواني: إعادة غلق القفل، وإعادة تشغيل جدار الليزر لحماية المكان (LOW)
  digitalWrite(LASER_RELAY, LOW);  
  lcd.clear();
  silentAlarmActive = false; 
  currentState = GATE_IDLE;
}

void triggerEmergencyAlarm() {
  isGateLockedOut = true;
  lockOutStartTime = millis();
  
  Serial.println("\n=============================================");
  Serial.println("WARNING: GATE LOCKED - EMERGENCY TRIGGERED");
  Serial.println("=============================================\n");
}

void checkPerimeterSecurity() {
  if (analogRead(LDR_PIN) < ldrThreshold) { 
    isIntrusionAlarmActive = true;
    intrusionAlarmStartTime = millis();
  }
}

void handleIntrusionAlarm() {
  unsigned long currentMillis = millis();
  if (currentMillis - intrusionAlarmStartTime >= intrusionAlarmDuration) {
    isIntrusionAlarmActive = false; 
    digitalWrite(BUZZER, HIGH); // إطفاء ريلاي البازر العكسي بعد انتهاء المدة
    lcd.clear(); 
    return;
  }
  static unsigned long lastIntrusionLCD = 0;
  if (currentMillis - lastIntrusionLCD >= 1000) {
    lcd.setCursor(0, 0); lcd.print("!! INTRUSION !!");
    lcd.setCursor(0, 1); lcd.print("ALARM ACTIVE... ");
    lastIntrusionLCD = currentMillis;
  }
  // وميض صوتي متقطع (تصفير سريع جداً تفعيل وفصل الريلاي العكسي)
  if (currentMillis - lastBuzzerToggle >= 100) {
    buzzerState = !buzzerState;
    digitalWrite(BUZZER, buzzerState ? LOW : HIGH); // LOW = صوت، HIGH = سكوت
    lastBuzzerToggle = currentMillis;
  }
}

void handleLockOut() {
  unsigned long currentMillis = millis();
  unsigned long elapsedTime = currentMillis - lockOutStartTime;
  if (elapsedTime >= lockOutDuration) {
    isGateLockedOut = false; remainingAttempts = 4;
    digitalWrite(BUZZER, HIGH); // إطفاء ريلاي البازر
    currentPIN = ""; lcd.clear();
    currentState = GATE_IDLE; return;
  }
  unsigned long remainingSeconds = (lockOutDuration - elapsedTime) / 1000;
  static unsigned long lastLCDUpdate = 0;
  if (currentMillis - lastLCDUpdate >= 1000) {
    lcd.setCursor(0, 0); lcd.print(" SYSTEM LOCKED! ");
    lcd.setCursor(0, 1); lcd.print("Time Left: ");
    if (remainingSeconds < 10) lcd.print("0");
    lcd.print(remainingSeconds); lcd.print("s        ");
    lastLCDUpdate = currentMillis;
  }
  // تصفير متقطع لحالة قفل البوابة
  if (currentMillis - lastBuzzerToggle >= 150) {
    buzzerState = !buzzerState;
    digitalWrite(BUZZER, buzzerState ? LOW : HIGH); // LOW = صوت، HIGH = سكوت
    lastBuzzerToggle = currentMillis;
  }
}

void maintainWifi() {
  if (WiFi.status() != WL_CONNECTED && millis() - lastWifiCheck > 10000) {
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD); lastWifiCheck = millis();
  }
}

void updateLCD(String top, String bottom) {
  lcd.clear(); lcd.setCursor(0, 0); lcd.print(top); lcd.setCursor(0, 1); lcd.print(bottom);
}

// دالة التصفير المؤقت المحدثة للريلاي العكسي
void triggerBuzzer(int duration) {
  digitalWrite(BUZZER, LOW); // تشغيل الريلاي العكسي (صوت)
  delay(duration); 
  digitalWrite(BUZZER, HIGH); // إطفاء الريلاي العكسي (سكوت)
}

void sendDataToFirebase(String token, String nextStep) {
  if (Firebase.ready()) {
    FirebaseJson json; 
    json.set("sessionToken", token.c_str());
    json.set("nextStep", nextStep.c_str());
    Firebase.RTDB.updateNode(&f1, "/", &json); 
  }
}