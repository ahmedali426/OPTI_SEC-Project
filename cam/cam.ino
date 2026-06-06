#include "esp_camera.h"
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include <Firebase_ESP_Client.h>
#include "esp_http_server.h"
#include "addons/TokenHelper.h"
#include "addons/RTDBHelper.h"

// --- 1. إعدادات شبكة الواي فاي والفايربيز ---
#define WIFI_SSID         "A13"
#define WIFI_PASSWORD     "123456789*"
#define API_KEY           "AIzaSyAJxudhGGSXfbpLjygTE4OZX_tCbZw2QWQ"
#define DATABASE_URL      "https://opti-sec-default-rtdb.europe-west1.firebasedatabase.app/"
#define USER_EMAIL        "ahmedali123@gmail.com"
#define USER_PASSWORD     "ahmedali123"

// رابط الاندبوينت المستهدف على Hugging Face حسب الصور
const char* ai_scan_gate_url = "https://ahmed-133-smart-gate-api.hf.space/scan-gate";

unsigned long lastFirebaseCheck = 0;
const unsigned long firebaseInterval = 1000; 
String currentNextStep = "";

// بافر تخزين آخر صورة لغرض الـ Web Check المحلي
uint8_t* last_captured_photo = NULL;
size_t last_photo_len = 0;

// --- 2. تعريفات أرجل الكاميرا الثابتة للبوردة (AI Thinker Mapping) ---
#define PWDN_GPIO_NUM    -1
#define RESET_GPIO_NUM   -1
#define XCLK_GPIO_NUM    15
#define SIOD_GPIO_NUM     4
#define SIOC_GPIO_NUM     5
#define Y9_GPIO_NUM      16
#define Y8_GPIO_NUM      17
#define Y7_GPIO_NUM      18
#define Y6_GPIO_NUM      12
#define Y5_GPIO_NUM      10
#define Y4_GPIO_NUM       8
#define Y3_GPIO_NUM       9
#define Y2_GPIO_NUM      11
#define VSYNC_GPIO_NUM    6
#define HREF_GPIO_NUM     7
#define PCLK_GPIO_NUM    13

FirebaseData f1;
FirebaseAuth auth;
FirebaseConfig config;
httpd_handle_t camera_httpd = NULL;

void updateFirebaseNextStep(String step) {
  if (Firebase.ready()) {
    Firebase.RTDB.setString(&f1, "/nextStep", step.c_str());
    Serial.println("[Firebase] nextStep updated to: " + step);
  }
}

// دالة سيرفر الويب لعرض الصورة والتأكد منها
static esp_err_t photo_web_handler(httpd_req_t *req) {
  if (last_captured_photo == NULL || last_photo_len == 0) {
    const char* no_photo_msg = "No photo captured yet! Please trigger the gate system first.";
    httpd_resp_set_type(req, "text/plain");
    httpd_resp_send(req, no_photo_msg, strlen(no_photo_msg));
    return ESP_OK;
  }
  httpd_resp_set_type(req, "image/jpeg");
  httpd_resp_send(req, (const char *)last_captured_photo, last_photo_len);
  return ESP_OK;
}

// 🚀 دالة البث والرفع المباشر لـ حزمة الـ Form-Data بالتوافق مع الـ Swagger (النسخة المصححة)
void captureAndScanGate() {
  Serial.println("\n[AI Camera] Trigger received! Starting 3 seconds countdown...");
  for (int i = 3; i > 0; i--) {
    Serial.printf("📸 Capturing face in: %d...\n", i);
    delay(1000); 
  }

  Serial.println("📸 FLASH! Capturing image now...");
  camera_fb_t *fb = esp_camera_fb_get();
  if (!fb) {
    Serial.println("❌ Camera capture failed!");
    updateFirebaseNextStep("CaptureImage"); 
    return;
  }

  uint8_t *jpg_buf = fb->buf;
  size_t jpg_len = fb->len;
  Serial.printf("Captured Frame Size: %d bytes\n", jpg_len);

  // تحديث الكاش المحلى لعرض اللقطة على صفحة الويب
  if (last_captured_photo != NULL) free(last_captured_photo);
  last_captured_photo = (uint8_t*)malloc(jpg_len);
  if (last_captured_photo != NULL) {
    memcpy(last_captured_photo, jpg_buf, jpg_len);
    last_photo_len = jpg_len;
    Serial.println("🎉 Last photo cached for Web Server!");
  }

  WiFiClientSecure client;
  client.setInsecure(); // لتخطي فحص شهادة الـ SSL وتوفير وقت المعالجة
  
  HTTPClient http;
  Serial.println("Connecting to Hugging Face AI Space...");
  
  if (http.begin(client, ai_scan_gate_url)) {
    String boundary = "--------------------------ESP32CAMBoundary";
    http.addHeader("Content-Type", "multipart/form-data; boundary=" + boundary);
    
    // تثبيت رأس الحقل "file" متوافق تماماً مع الـ Swagger
    String fileHeader = "--" + boundary + "\r\n";
    fileHeader += "Content-Disposition: form-data; name=\"file\"; filename=\"spoofing.jpeg\"\r\n";
    fileHeader += "Content-Type: image/jpeg\r\n\r\n";
    String bodyFooter = "\r\n--" + boundary + "--\r\n";
    
    int totalLength = fileHeader.length() + jpg_len + bodyFooter.length();
    http.addHeader("Content-Length", String(totalLength));
    
    Serial.println("Streaming multipart content to /scan-gate...");
    
    // الحل البديل والمستقر لحجز البافر الكلي وضخه عبر الـ POST الـ Public
    uint8_t* requestBodyBuf = (uint8_t*)malloc(totalLength);
    
    if (requestBodyBuf != NULL) {
      memcpy(requestBodyBuf, fileHeader.c_str(), fileHeader.length());
      memcpy(requestBodyBuf + fileHeader.length(), jpg_buf, jpg_len);
      memcpy(requestBodyBuf + fileHeader.length() + jpg_len, bodyFooter.c_str(), bodyFooter.length());
      
      int httpResponseCode = http.sendRequest("POST", requestBodyBuf, totalLength);
      
      if (httpResponseCode > 0) {
        String response = http.getString();
        Serial.print("🎉 [HTTP Code]: "); Serial.println(httpResponseCode);
        Serial.println("--- [AI Space Output Response] ---");
        Serial.println(response); 
        Serial.println("--------------------");
        
        // نجاح الرفع -> تسليم الراية ديناميكياً للماستر لتبدأ خطوة البصمة
        updateFirebaseNextStep("fingerprintCapture");
      } else {
        Serial.print("❌ HTTP Post failed, error string: ");
        Serial.println(http.errorToString(httpResponseCode).c_str());
        updateFirebaseNextStep("CaptureImage"); 
      }
      free(requestBodyBuf);
    } else {
      Serial.println("❌ Memory allocation failed for request body!");
      updateFirebaseNextStep("CaptureImage");
    }
    http.end();
  }
  esp_camera_fb_return(fb); 
}

void checkFirebaseTrigger() {
  if (!Firebase.ready()) return;

  if (Firebase.RTDB.getString(&f1, "/nextStep")) {
    currentNextStep = f1.stringData();
    if (currentNextStep == "CaptureImage") {
      Serial.println("[Firebase] nextStep changed to CaptureImage! Starting Stage 2...");
      Firebase.RTDB.setString(&f1, "/nextStep", "ProcessingImage");
      captureAndScanGate(); 
    }
  }
}

void startCameraWebServer() {
  httpd_config_t config_server = HTTPD_DEFAULT_CONFIG();
  config_server.server_port = 80;

  httpd_uri_t index_uri = {
    .uri       = "/",
    .method    = HTTP_GET,
    .handler   = photo_web_handler,
    .user_ctx  = NULL
  };
  
  if (httpd_start(&camera_httpd, &config_server) == ESP_OK) {
    httpd_register_uri_handler(camera_httpd, &index_uri);
    Serial.println("🌐 Local Web Server is live!");
  }
}

void setup() {
  Serial.begin(115200);

  camera_config_t config_cam;
  config_cam.ledc_channel = LEDC_CHANNEL_0;
  config_cam.ledc_timer   = LEDC_TIMER_0;
  config_cam.pin_d0       = Y2_GPIO_NUM;
  config_cam.pin_d1       = Y3_GPIO_NUM;
  config_cam.pin_d2       = Y4_GPIO_NUM;
  config_cam.pin_d3       = Y5_GPIO_NUM;
  config_cam.pin_d4       = Y6_GPIO_NUM;
  config_cam.pin_d5       = Y7_GPIO_NUM;
  config_cam.pin_d6       = Y8_GPIO_NUM;
  config_cam.pin_d7       = Y9_GPIO_NUM;
  config_cam.pin_xclk     = XCLK_GPIO_NUM;
  config_cam.pin_pclk     = PCLK_GPIO_NUM;
  config_cam.pin_vsync    = VSYNC_GPIO_NUM;
  config_cam.pin_href     = HREF_GPIO_NUM;
  config_cam.pin_sccb_sda = SIOD_GPIO_NUM;
  config_cam.pin_sccb_scl = SIOC_GPIO_NUM;
  config_cam.pin_pwdn     = PWDN_GPIO_NUM;
  config_cam.pin_reset    = RESET_GPIO_NUM;
  config_cam.xclk_freq_hz = 20000000;
  
  config_cam.pixel_format = PIXFORMAT_JPEG; 
  config_cam.frame_size   = FRAMESIZE_VGA;
  config_cam.fb_count     = 1;
  config_cam.fb_location  = CAMERA_FB_IN_PSRAM;
  config_cam.grab_mode    = CAMERA_GRAB_LATEST;

  if (esp_camera_init(&config_cam) != ESP_OK) {
    Serial.println("❌ Camera init failed!");
    return;
  }

  sensor_t *s = esp_camera_sensor_get();
  s->set_whitebal(s, 1);       
  s->set_exposure_ctrl(s, 1);  
  s->set_quality(s, 12); 
  s->set_framesize(s, FRAMESIZE_VGA);

  Serial.println("✅ Camera configuration approved!");

  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  Serial.print("Connecting to Wi-Fi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500); Serial.print(".");
  }
  Serial.println("\n✅ WiFi connected!");
  Serial.print("💡 Verification IP Link: http://"); Serial.println(WiFi.localIP());

  startCameraWebServer();

  config.api_key = API_KEY;
  auth.user.email = USER_EMAIL;
  auth.user.password = USER_PASSWORD;
  config.database_url = DATABASE_URL;
  config.token_status_callback = tokenStatusCallback;
  
  Firebase.reconnectNetwork(true);
  
  f1.setBSSLBufferSize(4096, 1024); 
  f1.setResponseSize(2048);
  Firebase.begin(&config, &auth);

  Serial.println("\n>>> ESP32-CAM System Synced & Online... <<<");
}

void loop() {
  if (millis() - lastFirebaseCheck >= firebaseInterval) {
    lastFirebaseCheck = millis();
    checkFirebaseTrigger(); 
  }
}