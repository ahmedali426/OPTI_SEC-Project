# OPTI-SEC: Optimized Biometric Access Control Platform

OPTI-SEC (Optimized Biometric Access Control System) is an enterprise-grade, multi-factor hardware security infrastructure designed to eliminate credential forgery, presentation attacks, and the lack of real-time administrative visibility inherent in traditional entry systems. Driven by an ESP32-S3 local system architecture, the platform mandates synchronous processing across three distinct verification matrices (PIN, Facial Liveness, and Biometric Fingerprint) before granting access.

---

## 1. System Pipeline Architecture

The system coordinates edge hardware logic, neural processing nodes, and database servers across a distributed multi-layer framework:
[ User Approaches Gate ]
│
▼

[ Phase 1: PIN Entry ] ──► ESP32-S3 Master ──► HTTP POST ──► .NET 9 Backend (Hash Check)
│

┌────────────┴────────────┐
▼                         ▼

[ Valid PIN ]             [ Duress PIN (9999) ]
│                         │
▼                         ▼

[ Activate Camera ]       [ Normal Flow + Silent Alert ]
│
▼

[ Phase 2: AI Face Check ] ──► ESP32-CAM ─────► HTTP POST ───────────────► AI Service Container
│
(Liveness + Embeddings)
│
▼

[ Phase 3: Biometric Match ] ─► R307 UART Scan ─► HTTP POST ──► Backend Matcher ◄───┘
│
▼

[ Cross-Match Matrix ]
│
▼

[ Open Gate Relay LOW ]

### The Cross-Matching Logic Mathematical Vector
To evaluate biometric cohesion and stop identity impersonation, the AI microservice extracts facial features into a 128-dimensional floating-point representation vector ($V_{live}$). This vector is evaluated using Euclidean Distance against the pre-enrolled user registry template array ($V_{stored}$):

$$d(V_{live}, V_{stored}) = \sqrt{\sum_{i=1}^{128} (V_{live, i} - V_{stored, i})^2}$$

Access proceeds to the final biometric phase only if the minimum distance falls below the security threshold parameter:
$$\min(d) < \tau \quad \text{where } \tau = 0.50$$

---

## 2. Hardware Specification & Pin Mapping

The hardware subsystem utilizes a dual-core ESP32-S3 as the main controller, communicating with dedicated peripherals over UART, I2C, and digital General-Purpose Input/Output (GPIO) pins.

| Peripheral Component | GPIO Protocol Pin | Logic Level | Operational Scope |
| :--- | :--- | :--- | :--- |
| **ESP32-S3 Core** | Central Core Master | 3.3V | Orchestrates local Finite State Machine (FSM) loops, network handshake, and asynchronous HTTP client routines. |
| **R307 Optical Biometric** | GPIO16 (RX2), GPIO17 (TX2) | 3.3V (Strict) | Collects 512-byte fingerprint ridge matrices over UART at 57600 baud rate. |
| **4x4 Matrix Keypad** | GPIO12, 13, 14, 27, 26, 33, 32 | 3.3V / 5V | Handles user character collection with software debouncing algorithms. |
| **ESP32-CAM Unit** | GPIO4 (Trigger), I2C Shared | 5V Supply | Captures VGA 640x480 compressed JPEGs upon hardware trigger pulse from the master core. |
| **I2C 16x2 Textual LCD** | GPIO22 (SDA), GPIO23 (SCL) | 5V | Displays real-time state text strings on address location `0x27`. |
| **Laser Tripwire Barrier** | GPIO34 (Analog Read ADC) | 3.3V Divider | Perimeter protection loop; triggers breach alert if `analogRead < 500`. |
| **2-Channel Relay Board** | GPIO18 (Gate Switch), GPIO25 | Active-LOW | Switches 12V Solenoid locks and high-output physical alarm buzzers. |

---

## 3. Subsystem Software Deep Dive

### AI Microservice & Presentation Attack Detection (PAD)
The computer vision subsystem runs on a containerized FastAPI application. It features a deep learning model trained to detect presentation attacks (spoofing via printed photos, masks, or digital screens).
* **Liveness Analysis**: Employs a custom ResNet-based binary classifier that analyzes micro-textures of the captured skin surface to distinguish between a real human face and a spoofing medium.
* **Database Differentiation**: Images processed by the AI system are dynamically sorted and logged into separate storage schemas. Verified profiles are registered under the `Authorized` database table, while failed verification attempts or unknown faces trigger entries into the `Unauthorized` table for auditing.

### .NET 9.0 Core Web API Backend
The enterprise layer is structured using Clean Architecture principles, ensuring strict separation of concerns across Domain, Application, Infrastructure, and Presentation layers[cite: 1].
* **Security Mechanisms**: Password PINs are salted and hashed using Argon2id before database verification.
* **Silent Duress Protocol**: If a user inputs the emergency override PIN (`9999`), the system triggers Case 3 execution logic[cite: 1]. The physical gate opens normally to prevent hostage escalation or danger to the user, while the backend immediately despatches a hidden background notification to administrative mobile devices[cite: 1].
* **Asynchronous Processing**: Intensive operations such as log generation and transaction archival are offloaded using Hangfire background workers[cite: 1].

### Flutter Mobile Application
The client subsystem is a cross-platform mobile application driven by the BLoC (Business Logic Component) pattern for predictive state management[cite: 1].
* **Role-Based Access Control (RBAC)**: Splits workflows into Admin and Client interfaces[cite: 1]. Admins can view access logs, manage user enrollments, and manually override gate locks[cite: 1].
* **Real-time Synchronization**: Connects to Firebase Realtime Database to receive instantaneous security breach alerts and stream network state logs[cite: 1].

---

## 4. Web API Endpoints Mapping

### Authentication & Gateways
* `POST /api/v1/auth/verify-pin`
    * Description: Receives and validates the hashed 4x4 keypad input sequence[cite: 1]. Triggers Silent Duress flow if code matches emergency parameters[cite: 1].
* `POST /api/v1/biometrics/face-check`
    * Description: Processes incoming multi-part form data containing the captured image from the ESP32-CAM[cite: 1]. Communicates internally with the AI FastAPI container[cite: 1].
* `POST /api/v1/biometrics/verify-fingerprint`
    * Description: Concludes cross-matching logic by checking the retrieved R307 character buffer token against relational entity keys[cite: 1].

### Administrative Controls
* `GET /api/v1/admin/logs/authorized`
    * Description: Fetches paginated tracking records of validated entry events[cite: 1].
* `GET /api/v1/admin/logs/unauthorized`
    * Description: Fetches flagged threat profiles, matching timestamps, and associated images captured during failed access attempts[cite: 1].
* `POST /api/v1/admin/gate/override`
    * Description: Forces a remote state change to trigger manual relay operation from the mobile application[cite: 1].

---

## 5. Docker Container Configuration Layout

The entire platform deployment suite is orchestrated using Docker Compose to ensure isolation, security, and reproducible builds across staging and production environments[cite: 1]:

```yaml
version: '3.8'

services:
  smart-gate-db:
    image: [mcr.microsoft.com/mssql/server:2022](https://mcr.microsoft.com/mssql/server:2022)
    environment:
      - SA_PASSWORD=YourSecure_SA_Pass123!
      - ACCEPT_EULA=Y
    volumes:
      - mssql_data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourSecure_SA_Pass123!", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  smart-gate-backend:
    image: optisec-backend:latest
    ports:
      - "5000:80"
    depends_on:
      smart-gate-db:
        condition: service_healthy
    volumes:
      - shared_images:/app/wwwroot/Images

  smart-gate-ai:
    image: optisec-ai:latest
    ports:
      - "8000:8000"
    volumes:
      - shared_images:/app/images
    restart: always

volumes:
  mssql_data:
  shared_images:

```

## 6. Local Execution & Deployment Guide

### Embedded Firmware Compilation
1. Load `esp32FinalCode/esp32FinalCode.ino` inside the Arduino IDE.
2. Ensure that the Espressif Board Manager framework version `3.x` is active.
3. Include required drivers: `Adafruit_Fingerprint.h`, `LiquidCrystal_I2C.h`, and `ArduinoJson.h`.
4. Adjust `WIFI_SSID` and `BACKEND_URL` application settings inside the configuration segment.
5. Compile and flash onto the target ESP32-S3 hardware.

### Full Multi-Container Stack Initialization
Execute the unified build and startup parameter via the repository root directory:
```bash
docker-compose up --build -d
docker ps
```

## 7. Graduation Project Engineering Team

Developed as a partial fulfillment of the Bachelor's Degree requirements in Computer Science & Information Technology at **Qena University, Faculty of Computers and Information (June 2026)**.

* **Ahmed Ali** (AI Engineer & Integration and MLOps Engineer & Embedded Architecture)
* **Sama Ahmed** (Embedded Architecture)
* **Ahmed Gamal** (UI/UX Designer)
* **Salma Abd-EL-Rehiem** (AI Engineer)
* **Ahmed Ibrahim** (Backend Developer)
* **Alaa Ahmed** (Mobile Developer)
* **Ahmed Mostafa** (Embedded Architecture Lead)
* **Shahd Mohamed** (Embedded Architecture)
* **Marwa Hassan** (Mobile Developer)

**Under the Academic Supervision of:**
* **Dr. Amal Rashed** (Assistant Professor)
* **Dr. Eman** (Assistant Teacher)
