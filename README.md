# OPTI-SEC: Optimized Biometric Access Control Platform

## 1. System Pipeline Architecture

The system coordinates edge hardware logic, neural processing nodes, and database servers across a distributed multi-layer framework:

```
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
```

### The Cross-Matching Logic Mathematical Vector
To evaluate biometric cohesion and stop identity identity impersonation, the AI model generates a 128-dimensional floating point representation vector ($V_{live}$) compared directly via Euclidean Distance against the pre-enrolled user registry template array ($V_{stored}$):

$$d(V_{live}, V_{stored}) = \sqrt{\sum_{i=1}^{128} (V_{live, i} - V_{stored, i})^2}$$

Access proceeds only if:
$$\min(d) < 	au \quad 	ext{where } 	au = 0.50$$

---

## 2. Hardware Specification Matrix

| Peripheral Component | GPIO Protocol Pin | Logic Level | Operational Scope |
| :--- | :--- | :--- | :--- |
| **ESP32-S3 Core Dual-Core** | Central Core Master | 3.3V | Orchestrates local FSM state loops and Wi-Fi networks |
| **R307 Optical Biometric** | GPIO16 (RX2), GPIO17 (TX2) | 3.3V (Strict) | Collects 512-byte fingerprint ridge matrices over UART |
| **4x4 Matrix Keypad** | GPIO12, 13, 14, 27, 26, 33, 32 | 3.3V / 5V | Handles character collection with software debouncing |
| **ESP32-CAM Processing Unit**| GPIO4 (Trigger), I2C Shared | 5V Supply | Captures VGA 640x480 compressed JPEGs upon trigger pulse |
| **I2C 16x2 Textual LCD** | GPIO22 (SDA), GPIO23 (SCL) | 5V | Displays state text strings on address location `0x27` |
| **2-Channel Relay Board** | GPIO18 (Gate Switch), GPIO25 | Active-LOW | Switches 12V Solenoid locks and high-output alarm buzzers |
| **Laser Tripwire Barrier** | GPIO34 (Analog Read ADC) | 3.3V Divider | Perimeter perimeter protection. Triggers if `analogRead < 500` |

---

## 3. Docker Container Configuration Layout

The backend infrastructure is containerized as an isolated multi-container architecture using Docker Compose:

*   **smart-gate-backend** (`mcr.microsoft.com/dotnet/aspnet:9.0`): Listens on Host Port `5000`. Drives core API routes.
*   **smart-gate-ai** (`python:3.11-slim`): Listens on Internal Port `8000`. Runs FastAPI server executing facial embeddings.
*   **smart-gate-db** (`mcr.microsoft.com/mssql/server:2022`): Relational DB hidden from public network interfaces.

### Multi-Container Boot Orchestration Diagram
```yaml
version: '3.8'

services:
  smart-gate-db:
    image: mcr.microsoft.com/mssql/server:2022
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

---

## 4. Local Execution & Deployment Guide

### Embedded Firmware Compilation
1. Load `esp32FinalCode/esp32FinalCode.ino` inside the Arduino IDE.
2. Validate that the Espressif Board Manager framework version `3.x` is active.
3. Include required drivers: `Adafruit_Fingerprint.h`, `LiquidCrystal_I2C.h`, and `ArduinoJson.h`.
4. Adjust `WIFI_SSID` and `BACKEND_URL` application settings inside the configuration segment.
5. Compile and flash onto the target ESP32-S3 hardware.

### Full Multi-Container Stack Initialization
Execute the unified build and startup parameter via the repository root directory:
```bash
docker-compose up --build -d
```
Verify container verification metrics by running:
```bash
docker ps
```

---

## 5. Graduation Project Engineering Team
Developed as a partial fulfillment of the Bachelor's Degree requirements in Computer Science & Information Technology at **South Valley University, Faculty of Computers and Information (June 2026)**.

*   **Ahmed Ali** (AI Engineering & Embedded Architecture Lead)
*   **Sama Ahmed** (Information Technology)
*   **Ahmed Gamal** (Information Technology)
*   **Salma Abd-EL-Rehiem** (Information Technology)
*   **Ahmed Ibrahim** (Information Technology)
*   **Alaa Ahmed** (Computer Science)
*   **Ahmed Mostafa** (Information Technology)
*   **Shahd Mohamed** (Information Technology)
*   **Marwa Hassan** (Computer Science)

**Under the Academic Supervision of:**
*   **Dr. Amal Rashed** (Assistant Professor)
*   **Dr. Eman** (Assistant Teacher)