# 📘 Opti-Sec Backend — Technical Specification Document

> **Version:** 1.0  
> **Date:** 2026-05-30  
> **Base URL:** `https://<your-server>/api`  
> **Auth Route (exception):** `https://<your-server>/Auth` _(no `/api` prefix)_

---

# 1. System Overview

**Opti-Sec** is a multi-layer physical security system that protects gates (doors/entrances) using a 3-step verification process:

1. **Password** → Entered on a physical keypad (embedded device)
2. **AI Face Recognition** → Camera captures a face, an external AI service identifies the person
3. **Fingerprint** → Biometric verification confirms identity

The system consists of four main components that communicate through this backend API:

| Component | Technology | Role |
|-----------|-----------|------|
| **Mobile App** | Flutter / Android / iOS | Gate management, emergency monitoring, remote gate control |
| **Backend API** | ASP.NET Core 9 + SQL Server | Central orchestration, session management, notifications |
| **AI Module** | Python (external service) | Face recognition, member training |
| **Embedded Device** | ESP32 / Arduino | Keypad, camera, fingerprint sensor, buzzer, gate lock |

---

# 2. Architecture Overview

## 2.1 High-Level Communication Flow

```
┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│   Mobile App     │◄─────►│   Backend API    │◄─────►│   AI Module      │
│   (Flutter)      │  JWT  │   (ASP.NET Core) │  HTTP │   (Python)       │
│                  │ + WS  │                  │       │                  │
└──────────────────┘       └────────┬─────────┘       └──────────────────┘
                                    │
                                    │ HTTP Polling
                                    │
                           ┌────────▼─────────┐
                           │  Embedded Device  │
                           │  (ESP32)          │
                           └──────────────────┘
```

## 2.2 Gate Access — Full Workflow (Step by Step)

```
STEP 1: PASSWORD
━━━━━━━━━━━━━━━━
Device → POST /api/Device/validate-password
         { gateId, password, deviceId, timestamp }
         
Backend validates password:
  ✅ Correct     → Creates GateSession, returns sessionToken + "CaptureImage" command
  ⚠️ Silent Alarm → Creates GateSession (marked silent), returns same as Correct 
                    (device doesn't know — backend notifies mobile app silently)
  ❌ Wrong       → Increments attempt counter
                    If attempts >= 3 → EMERGENCY (buzzer + notification)

STEP 2: AI FACE RECOGNITION
━━━━━━━━━━━━━━━━━━━━━━━━━━━
Device captures image → sends to AI Module directly
AI Module → POST /api/AICallback/recognition-result
            { sessionToken, isAuthorized, confidenceScore, matchedMemberId, image }

Backend processes:
  ✅ Authorized (score >= 0.85) → Moves to fingerprint step
  🔄 Low confidence             → Retry (up to 3 attempts)
  ❌ Max attempts reached       → EMERGENCY (buzzer + notification)

STEP 3: FINGERPRINT VERIFICATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Device → POST /api/Device/verify-fingerprint
         { sessionToken, memberId, fingerprintTemplate, deviceId }

Backend verifies:
  ✅ Match       → ACCESS GRANTED 🎉 (gate opens)
  ❌ Mismatch    → Retry (up to 3 attempts), then EMERGENCY
```

## 2.3 Command Delivery (Backend → Device)

The backend issues commands to the device (open gate, activate buzzer, etc.) via two methods:

1. **Inline Commands** — Returned directly in API responses as `Commands` object
2. **Polling Endpoint** — Device calls `GET /api/Device/pending-commands?gateId=X` every 2-3 seconds

```
Backend creates DeviceCommand (DB) → Status: Pending
Device polls pending-commands      → Status: Sent  
Device executes + acknowledges     → Status: Acknowledged
```

---

# 3. Authentication & Session Concept

## 3.1 User Authentication (Mobile App)

| Field | Description |
|-------|-------------|
| **Method** | JWT Bearer Token |
| **Header** | `Authorization: Bearer <token>` |
| **Token Lifetime** | Configured in appsettings (default ~30 min) |
| **Refresh Token** | 14-day lifetime, stored per user, rotated on refresh |
| **Roles** | `Admin` — manages clients/roles; `Client` — manages gates/members |

## 3.2 Gate Session Lifecycle

A `GateSession` is created when a **correct password** is entered at a gate. It tracks the entire verification flow.

```
┌────────────┐    ┌────────────┐    ┌─────────────────┐    ┌────────────┐
│  Password  │───►│  AI Face   │───►│  Fingerprint    │───►│ Completed  │
│  Pending   │    │  Pending   │    │  Pending        │    │ (Granted)  │
└────────────┘    └────────────┘    └─────────────────┘    └────────────┘
      │                 │                   │
      ▼                 ▼                   ▼
  [Failed]          [Failed]           [Failed]
  [Emergency]       [Emergency]        [Emergency]
                                       [Expired ←── Hangfire job (5 min timeout)]
```

### Key Session Fields

| Field | Type | Description |
|-------|------|-------------|
| `SessionToken` | `Guid` (v7) | Unique session ID — passed between all steps |
| `GateId` | `int` | Which gate this session belongs to |
| `DeviceId` | `string` | Physical device identifier |
| `Status` | `SessionStatus` | Current state (`PasswordPassed`, `AIPassed`, etc.) |
| `Result` | `SessionResult` | Final outcome (`Pending`, `Granted`, `DeniedAI`, `Expired`, etc.) |
| `StartedAt` | `DateTime` | When the session was created |
| `CompletedAt` | `DateTime?` | When the session ended |

### Session Timeout
A Hangfire recurring job runs **every minute** and expires any session where:
- `Result == Pending` AND `StartedAt < 5 minutes ago`

---

# 4. API Endpoints

## 🔐 Authentication Endpoints

> **Base Route:** `/Auth` _(note: no `/api` prefix)_  
> **Auth Required:** ❌ None

---

### 4.1 Login

**`POST /Auth/login`**

**Purpose:** Authenticate a user (Admin or Client) and receive JWT + refresh tokens.

**Request Body:**
```json
{
  "email": "admin@optisec.com",
  "password": "P@ssw0rd123"
}
```

**Success Response (200):**
```json
{
  "id": "0193a5d2-...",
  "email": "admin@optisec.com",
  "fName": "John",
  "lName": "Doe",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expireIn": 30,
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl...",
  "refreshTokenExpiration": "2026-06-13T12:00:00Z",
  "roles": ["Client"]
}
```

**Error Responses:**
| Code | Reason |
|------|--------|
| 400 | Invalid credentials |
| 423 | User locked out (5 failed login attempts, 5 min lockout) |

---

### 4.2 Refresh Token

**`POST /Auth/refresh`**

**Purpose:** Exchange an expired JWT for a new one using a valid refresh token.

**Request Body:**
```json
{
  "token": "eyJhbGciOi... (expired JWT)",
  "refreshToken": "dGhpcyBpcyBh..."
}
```

**Success Response (200):** Same shape as Login response with new tokens.

---

### 4.3 Revoke Refresh Token

**`POST /Auth/revoke-refresh-token`**

**Purpose:** Invalidate a refresh token (logout).

**Request Body:**
```json
{
  "token": "eyJhbGciOi...",
  "refreshToken": "dGhpcyBpcyBh..."
}
```

**Success Response (200):** Empty

---

### 4.4 Forget Password

**`POST /Auth/forget-password`**

**Purpose:** Send a 6-digit OTP code to the user's email for password reset.

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Success Response (200):**
```json
{
  "email": "user@example.com"
}
```

> [!NOTE]
> Returns success even if the email doesn't exist (to prevent email enumeration attacks).

---

### 4.5 Reset Password

**`POST /Auth/reset-password`**

**Purpose:** Reset password using the OTP code received via email.

**Request Body:**
```json
{
  "email": "user@example.com",
  "code": "482901",
  "newPassword": "NewP@ssw0rd123"
}
```

**Success Response (200):** Empty

---

## 👤 Client Management (Admin Only)

> **Base Route:** `/api/Clients`  
> **Auth Required:** ✅ `Bearer Token` with role `Admin`

---

### 4.6 Create Client

**`POST /api/Clients`**

**Purpose:** Register a new client (building owner). Creates an Identity user with the `Client` role.

**Content-Type:** `multipart/form-data`

**Request Fields:**
| Field | Type | Required |
|-------|------|----------|
| `FName` | string | ✅ |
| `LName` | string | ✅ |
| `Email` | string | ✅ |
| `UserName` | string | ✅ |
| `Password` | string | ✅ (min 8 chars, upper+lower+digit+special) |
| `PhoneNumber` | string | ✅ |
| `Image` | file | ✅ |

**Success Response (200):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "userName": "johndoe",
  "phoneNumber": "+20123456789",
  "imageUrl": "/Images/abc123.jpg"
}
```

---

### 4.7 Get All Clients

**`GET /api/Clients?search=john`**

**Purpose:** List all active clients. Optional search by name.

**Success Response (200):**
```json
[
  { "id": 1, "name": "John Doe", "imageUrl": "/Images/abc.jpg" },
  { "id": 2, "name": "Jane Smith", "imageUrl": "/Images/def.jpg" }
]
```

---

### 4.8 Get All Clients (AI)

**`GET /api/Clients/all-clients-ai`**

**Auth Required:** ❌ (AllowAnonymous — used by AI module)

**Purpose:** Returns client data needed by the AI module for training/sync.

**Success Response (200):**
```json
[
  { "id": 1, "name": "John Doe", "userName": "johndoe", "imageUrl": "/Images/abc.jpg", "isDeleted": false }
]
```

---

### 4.9 Get Client by ID

**`GET /api/Clients/{id}`**

**Success Response (200):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "userName": "johndoe",
  "phoneNumber": "+20123456789",
  "imageUrl": "/Images/abc.jpg"
}
```

---

### 4.10 Update Client

**`PUT /api/Clients/{id}`**

**Content-Type:** `multipart/form-data`

**Request Fields:**
| Field | Type | Required |
|-------|------|----------|
| `Name` | string | ✅ |
| `Email` | string | ✅ |
| `UserName` | string | ✅ |
| `PhoneNumber` | string | ✅ |
| `Image` | file | ❌ (optional) |

**Success Response:** `204 No Content`

---

### 4.11 Delete Client

**`DELETE /api/Clients/{id}`**

**Purpose:** Soft-delete a client.

**Success Response:** `204 No Content`

---

### 4.12 Count Clients

**`GET /api/Clients/clients-count`**

**Success Response (200):**
```json
5
```

---

## 🚪 Gate Management (Client Only)

> **Base Route:** `/api/Gates`  
> **Auth Required:** ✅ `Bearer Token` with role `Client`

---

### 4.13 Create Gate

**`POST /api/Gates`**

**Purpose:** Create a new gate. The backend auto-generates a `DeviceApiKey`.

**Request Body:**
```json
{
  "name": "Main Entrance",
  "location": "Building A, Floor 1",
  "deviceId": "ESP32-001",
  "password": "1234",
  "silentAlarm": "9999"
}
```

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Main Entrance",
  "location": "Building A, Floor 1",
  "deviceId": "ESP32-001",
  "status": "Online",
  "maxFailedAttempts": 3,
  "password": "1234",
  "silentAlarm": "9999",
  "deviceApiKey": "aB3cD4eF5gH6iJ7kL8mN9oP0qRsTuVw"
}
```

---

### 4.14 Get All Gates

**`GET /api/Gates`**

**Success Response (200):**
```json
[
  { "id": 1, "name": "Main Entrance", "location": "Building A" },
  { "id": 2, "name": "Back Door", "location": "Building A" }
]
```

---

### 4.15 Get Gate by ID

**`GET /api/Gates/{id}`**

**Success Response (200):** Same shape as Create Gate response.

---

### 4.16 Update Gate

**`PUT /api/Gates/{id}`**

**Request Body:**
```json
{
  "name": "Main Entrance (Updated)",
  "location": "Building B",
  "deviceId": "ESP32-002",
  "password": "1234",
  "silentAlarm": "9999"
}
```

**Success Response:** `204 No Content`

> [!NOTE]
> Currently only `name`, `location`, and `deviceId` are updated. Password/SilentAlarm changes are NOT applied during update.

---

### 4.17 Delete Gate

**`DELETE /api/Gates/{id}`**

**Success Response:** `204 No Content`

---

### 4.18 Count Gates

**`GET /api/Gates/count-gate`**

**Success Response (200):**
```json
3
```

---

## 👥 Member Management (Client Only)

> **Base Route:** `/api/Members`  
> **Auth Required:** ✅ `Bearer Token` with role `Client`

---

### 4.19 Create Member

**`POST /api/Members`**

**Content-Type:** `multipart/form-data`

**Purpose:** Register a new member (person authorized to access gates). Triggers AI face training automatically.

**Request Fields:**
| Field | Type | Required |
|-------|------|----------|
| `FName` | string | ✅ |
| `LName` | string | ✅ |
| `UserName` | string | ✅ |
| `Phone` | string | ✅ |
| `Image` | file | ✅ (face photo for AI training) |

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Ahmed Ali",
  "userName": "ahmedali",
  "phone": "+20100000000",
  "imageUrl": "/Images/member123.jpg"
}
```

---

### 4.20 Get All Members

**`GET /api/Members?search=ahmed`**

**Success Response (200):**
```json
[
  { "id": 1, "name": "Ahmed Ali", "imageUrl": "/Images/member123.jpg" },
  { "id": 2, "name": "Sara Hassan", "imageUrl": "/Images/member456.jpg" }
]
```

---

### 4.21 Get All Members (AI)

**`GET /api/Members/all-members-ai`**

**Auth Required:** ❌ (AllowAnonymous — used by AI module)

**Success Response (200):**
```json
[
  { "id": 1, "name": "Ahmed Ali", "userName": "ahmedali", "imageUrl": "/Images/m1.jpg", "isDeleted": false }
]
```

---

### 4.22 Get Member by ID

**`GET /api/Members/{id}`**

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Ahmed Ali",
  "userName": "ahmedali",
  "phone": "+20100000000",
  "imageUrl": "/Images/member123.jpg"
}
```

---

### 4.23 Update Member

**`PUT /api/Members/{id}`**

**Content-Type:** `multipart/form-data`

**Request Fields:**
| Field | Type | Required |
|-------|------|----------|
| `Name` | string | ✅ |
| `UserName` | string | ✅ |
| `Phone` | string | ✅ |
| `Image` | file | ❌ (optional) |

**Success Response:** `204 No Content`

---

### 4.24 Delete Member

**`DELETE /api/Members/{id}`**

**Success Response:** `204 No Content`

---

### 4.25 Count Members

**`GET /api/Members/member-count`**

**Success Response (200):**
```json
4
```

---

### 4.26 Set Fingerprint Template

**`POST /api/Members/set-fingerprint`**

**Auth Required:** ❌ (AllowAnonymous — called by embedded device)

**Purpose:** Store or update a member's fingerprint template. Called by the embedded device during enrollment.

**Request Body:**
```json
{
  "memberId": 1,
  "fingerprintTemplate": "BASE64_ENCODED_TEMPLATE_DATA..."
}
```

**Success Response (200):** Empty

---

## 🔧 Device Endpoints (Embedded / IoT)

> **Base Route:** `/api/Device`  
> **Auth Required:** ❌ None (device-facing endpoints)

---

### 4.27 Validate Password (STEP 1)

**`POST /api/Device/validate-password`**

**Purpose:** The device sends the password entered on the keypad. The backend validates it and starts a session if correct.

**Request Body:**
```json
{
  "gateId": 1,
  "password": "1234",
  "timestamp": "2026-05-30T12:00:00Z",
  "deviceId": "ESP32-001"
}
```

**Response — Correct Password (200):**
```json
{
  "success": true,
  "sessionToken": "0193a5d2-7f3e-7abc-8def-1234567890ab",
  "passwordStatus": "Correct",
  "nextStep": "CaptureImage",
  "attemptNumber": 0,
  "remainingAttempts": 3,
  "emergency": false,
  "commands": {
    "openGate": false,
    "activateCamera": true,
    "activateBuzzer": false,
    "stopBuzzer": false,
    "captureFingerprint": false,
    "buzzerDurationSeconds": 0,
    "delaySeconds": 3,
    "expectedMemberId": null
  }
}
```

**Response — Wrong Password (200):**
```json
{
  "success": false,
  "sessionToken": null,
  "passwordStatus": "Wrong",
  "nextStep": null,
  "attemptNumber": 2,
  "remainingAttempts": 1,
  "emergency": false,
  "commands": { ... }
}
```

**Response — Wrong Password + Emergency (attempts >= 3) (200):**
```json
{
  "success": false,
  "sessionToken": null,
  "passwordStatus": "Wrong",
  "nextStep": null,
  "attemptNumber": 3,
  "remainingAttempts": 0,
  "emergency": true,
  "commands": {
    "activateBuzzer": true,
    "buzzerDurationSeconds": 30,
    ...
  }
}
```

**Response — Gate Not Found (200):**
```json
{
  "success": false,
  "sessionToken": null,
  "passwordStatus": "GateNotFound",
  "nextStep": null,
  "attemptNumber": 0,
  "remainingAttempts": 0,
  "emergency": false,
  "commands": { ... }
}
```

> [!IMPORTANT]
> The `commands` object tells the device what to do next. The device must execute these commands immediately.

---

### 4.28 Verify Fingerprint (STEP 3)

**`POST /api/Device/verify-fingerprint`**

**Purpose:** The device sends the captured fingerprint for verification against the member identified by AI.

**Request Body:**
```json
{
  "sessionToken": "0193a5d2-7f3e-7abc-8def-1234567890ab",
  "memberId": 5,
  "fingerprintTemplate": "BASE64_TEMPLATE...",
  "deviceId": "ESP32-001"
}
```

**Response — Access Granted (200):**
```json
{
  "success": true,
  "status": 0,
  "accessGranted": true,
  "memberName": "Ahmed Ali",
  "attemptNumber": 1,
  "remainingAttempts": 0,
  "emergency": false,
  "commands": {
    "openGate": true,
    ...
  }
}
```

**Response — Wrong Fingerprint (200):**
```json
{
  "success": false,
  "status": 3,
  "accessGranted": false,
  "memberName": null,
  "attemptNumber": 1,
  "remainingAttempts": 2,
  "emergency": false,
  "commands": { ... }
}
```

**FingerprintStatus enum values:**
| Value | Name | Description |
|-------|------|-------------|
| 0 | `Success` | Fingerprint matched |
| 1 | `InvalidSession` | Session not found |
| 2 | `MemberMismatch` | MemberId doesn't match session |
| 3 | `WrongFingerprint` | No match, retries remaining |
| 4 | `MaxAttemptsReached` | Emergency triggered |

---

### 4.29 Laser Intrusion Alert

**`POST /api/Device/laser-intrusion`**

**Purpose:** Laser sensor triggered — report intrusion attempt.

**Request Body:**
```json
{
  "gateId": 1,
  "deviceId": "ESP32-001",
  "timestamp": "2026-05-30T12:00:00Z"
}
```

**Success Response (200):**
```json
{
  "received": true,
  "emergencyId": 42,
  "commands": {
    "activateBuzzer": true,
    "buzzerDurationSeconds": 30,
    ...
  }
}
```

---

### 4.30 Poll Pending Commands

**`GET /api/Device/pending-commands?gateId=1`**

**Purpose:** Device polls this endpoint every 2-3 seconds to pick up commands issued by the mobile app or backend (e.g., "open gate", "stop buzzer").

**Query Parameters:**
| Param | Type | Required |
|-------|------|----------|
| `gateId` | int | ✅ |

**Success Response (200):**
```json
[
  {
    "commandId": 42,
    "commandType": "OpenGate",
    "payloadJson": "{\"openGate\":true}",
    "source": "MobileApp",
    "issuedAt": "2026-05-30T12:05:00Z"
  },
  {
    "commandId": 43,
    "commandType": "StopBuzzer",
    "payloadJson": "{\"stopBuzzer\":true}",
    "source": "MobileApp",
    "issuedAt": "2026-05-30T12:05:30Z"
  }
]
```

**Empty Response (no pending commands):**
```json
[]
```

**CommandType values:** `OpenGate`, `ActivateBuzzer`, `StopBuzzer`, `CaptureImage`, `CaptureFingerprint`

---

### 4.31 Acknowledge Command

**`POST /api/Device/acknowledge-command`**

**Purpose:** Device confirms it has executed a command. This updates the command status to `Acknowledged`.

**Request Body:**
```json
{
  "commandId": 42,
  "deviceId": "ESP32-001"
}
```

**Success Response (200):**
```json
{ "acknowledged": true }
```

---

### 4.32 Device Heartbeat

**`POST /api/Device/heartbeat`**

**Purpose:** Periodic health check from the device to confirm it's alive.

**Request Body:**
```json
{
  "gateId": 1,
  "deviceId": "ESP32-001",
  "timestamp": "2026-05-30T12:00:00Z"
}
```

**Success Response (200):**
```json
{
  "received": true,
  "serverTime": "2026-05-30T12:00:01Z"
}
```

---

## 🤖 AI Callback Endpoints

> **Base Route:** `/api/AICallback`  
> **Auth Required:** ❌ None (called by AI module)

---

### 4.33 AI Recognition Result (STEP 2)

**`POST /api/AICallback/recognition-result`**

**Content-Type:** `multipart/form-data`

**Purpose:** The AI module sends the face recognition result after analyzing the captured image.

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `SessionToken` | Guid | ✅ | The session token from Step 1 |
| `IsAuthorized` | bool | ✅ | Whether the face was recognized |
| `ConfidenceScore` | double | ✅ | 0.0 to 1.0 (threshold: 0.85) |
| `MatchedMemberId` | int? | ❌ | ID of the matched member (null if unknown) |
| `ProcessingTimeMs` | int | ✅ | How long AI took to process |
| `ImageUrl` | file | ❌ | The captured image |

**Response — Success (proceed to fingerprint) (200):**
```json
{
  "received": true,
  "status": 0,
  "nextStep": "CaptureFingerprint",
  "memberId": 5,
  "message": "AI verification successful",
  "commands": {
    "captureFingerprint": true,
    "expectedMemberId": 5,
    ...
  }
}
```

**Response — Retry needed (200):**
```json
{
  "received": true,
  "status": 2,
  "nextStep": "RetryCapture",
  "memberId": null,
  "message": "AI not confident - retry required",
  "commands": {
    "activateCamera": true,
    "delaySeconds": 3,
    ...
  }
}
```

**Response — Emergency (max attempts) (200):**
```json
{
  "received": true,
  "status": 3,
  "nextStep": null,
  "memberId": null,
  "message": "Maximum AI attempts reached - Emergency triggered",
  "commands": {
    "activateBuzzer": true,
    "buzzerDurationSeconds": 30,
    ...
  }
}
```

**AIRecognitionStatus enum:**
| Value | Name |
|-------|------|
| 0 | `Success` |
| 1 | `InvalidSession` |
| 2 | `Retry` |
| 3 | `EmergencyTriggered` |

---

### 4.34 AI Training Complete

**`POST /api/AICallback/training-complete`**

**Purpose:** AI module notifies the backend that face model training has completed for a member.

**Request Body:**
```json
{
  "memberId": 5,
  "success": true,
  "embeddingVector": "0.123,0.456,0.789,...",
  "errorMessage": null
}
```

**Success Response (200):**
```json
{
  "received": true,
  "status": "Trained"
}
```

---

## 📱 Mobile App Endpoints (Client Only)

> **Base Route:** `/api/MobileCommands`  
> **Auth Required:** ✅ `Bearer Token` with role `Client`

---

### 4.35 Get Gates Status

**`GET /api/MobileCommands/gates/status`**

**Purpose:** Real-time status of all gates belonging to the current user.

**Success Response (200):**
```json
[
  {
    "gateId": 1,
    "name": "Main Entrance",
    "status": "Online",
    "currentSessionToken": "0193a5d2-...",
    "buzzerActive": false,
    "activeEmergencies": 0,
    "lastHeartbeat": null
  }
]
```

---

### 4.36 Get Session History

**`GET /api/MobileCommands/sessions?page=1&pageSize=20`**

**Purpose:** Paginated history of all gate sessions.

**Success Response (200):**
```json
[
  {
    "id": 1,
    "sessionToken": "0193a5d2-...",
    "status": "Completed",
    "result": "Granted",
    "memberName": "Ahmed Ali",
    "isSilentAlarm": false,
    "startedAt": "2026-05-30T10:00:00Z",
    "completedAt": "2026-05-30T10:02:30Z",
    "passwordPassed": true,
    "aiPassed": true,
    "fingerprintPassed": true
  }
]
```

---

### 4.37 Get Emergencies

**`GET /api/MobileCommands/emergencies?activeOnly=true`**

**Purpose:** List all emergency events. Use `activeOnly=true` to show only unresolved.

**Success Response (200):**
```json
[
  {
    "id": 1,
    "gateId": 1,
    "gateName": "Main Entrance",
    "type": "PasswordBreach",
    "severity": "Critical",
    "description": "Multiple failed password attempts at Gate 1",
    "buzzerActivated": true,
    "occurredAt": "2026-05-30T11:30:00Z",
    "isResolved": false,
    "resolvedAt": null
  }
]
```

**EmergencyType values:** `PasswordBreach`, `AIFailed`, `FingerprintFailed`, `LaserIntrusion`  
**EmergencySeverity values:** `Low`, `Medium`, `High`, `Critical`

---

### 4.38 Resolve Emergency

**`POST /api/MobileCommands/emergencies/{emergencyId}/resolve`**

**Purpose:** Mark an emergency as resolved.

**Success Response (200):**
```json
{ "resolved": true }
```

---

### 4.39 Stop Buzzer

**`POST /api/MobileCommands/gates/{gateId}/stop-buzzer`**

**Purpose:** Send a command to stop the buzzer at a specific gate.

**Success Response (200):**
```json
{ "stopped": true }
```

---

### 4.40 Open Gate (Remote)

**`POST /api/MobileCommands/gates/{gateId}/open`**

**Purpose:** Remotely open a gate from the mobile app.

**Success Response (200):**
```json
{ "opened": true }
```

---

### 4.41 Register FCM Token

**`POST /api/MobileCommands/register-fcm-token`**

**Purpose:** Register or update the Firebase Cloud Messaging token for push notifications.

**Request Body:**
```json
{
  "fcmToken": "fMCtoken_abc123...",
  "platform": "Android"
}
```

**Success Response (200):**
```json
{ "registered": true }
```

---

### 4.42 Get Notifications

**`GET /api/MobileCommands/notifications?page=1&pageSize=20`**

**Purpose:** Paginated notification history for the current user.

**Success Response (200):**
```json
[
  {
    "id": 1,
    "type": "Emergency",
    "priority": "Critical",
    "title": "🚨 Emergency Alert",
    "body": "Intrusion detected at Gate 1!",
    "isSent": true,
    "createdAt": "2026-05-30T11:30:00Z",
    "sentAt": "2026-05-30T11:30:01Z",
    "gateId": 1,
    "gateName": "Main Entrance"
  }
]
```

**NotificationType values:** `PasswordSuccess`, `WrongPassword`, `SilentAlarm`, `AIAuthorized`, `AIFailed`, `FingerprintSuccess`, `FingerprintFailed`, `Emergency`, `GateOpened`, `BuzzerStopped`

---

## 📋 Access Log Endpoints

> **Base Route:** `/api/AccessLogs`  
> **Auth Required:** ✅ `Client` role (except POST)

---

### 4.43 Create/Check Access Log

**`POST /api/AccessLogs`**

**Auth Required:** ❌ (AllowAnonymous)

**Content-Type:** `multipart/form-data`

**Request Fields:**
| Field | Type | Required |
|-------|------|----------|
| `GateId` | int | ✅ |
| `UserName` | string | ❌ |
| `FingerprintTemplate` | string | ❌ |
| `Image` | file | ❌ |

**Success Response (200):** Empty

---

### 4.44 Get Authorized Access Logs

**`GET /api/AccessLogs/authorized`**

**Success Response (200):**
```json
[
  {
    "id": 1,
    "name": "Ahmed Ali",
    "gateName": "Main Entrance",
    "imageUrl": "/Images/access1.jpg",
    "dateOnly": "2026-05-30",
    "timeOnly": "10:02:30"
  }
]
```

---

### 4.45 Get Unauthorized Access Logs

**`GET /api/AccessLogs/unauthorized`**

**Success Response (200):** Same shape as authorized logs.

---

## 🛡️ Role Management (Admin Only)

> **Base Route:** `/api/Roles`  
> **Auth Required:** ✅ `Bearer Token` with role `Admin`

---

### 4.46 Get All Roles

**`GET /api/Roles?includeDisabled=false`**

---

### 4.47 Get Role by ID

**`GET /api/Roles/{id}`**

---

### 4.48 Create Role

**`POST /api/Roles`**

**Request Body:**
```json
{ "name": "Viewer" }
```

---

### 4.49 Update Role

**`PUT /api/Roles/{id}`**

**Request Body:**
```json
{ "name": "Viewer Updated" }
```

---

### 4.50 Toggle Role (Enable/Disable)

**`PUT /api/Roles/{id}/Toggle`**

---

# 5. Real-Time Events (SignalR)

> **Hub URL:** `wss://<server>/hubs/gate`  
> **Auth Required:** ✅ Bearer Token  
> **Protocol:** SignalR (WebSocket with fallback)

On connection, the client is automatically subscribed to all gates owned by their account.

## 5.1 Events (Server → Client)

| Event | Payload | When |
|-------|---------|------|
| `NotificationReceived` | `NotificationDto` | Any notification (password, AI, fingerprint, etc.) |
| `EmergencyAlert` | `EmergencyAlertDto` | Emergency or silent alarm triggered |
| `GateStatusChanged` | `(gateId, status)` | Gate goes online/offline |
| `SessionUpdated` | `GateSessionDto` | Session step changes |
| `CommandAcknowledged` | `(commandId, commandType)` | Device acknowledges a command |
| `BuzzerStatusChanged` | `(gateId, isActive)` | Buzzer activated/stopped |

## 5.2 Client → Server Methods

| Method | Parameters | Purpose |
|--------|-----------|---------|
| `SubscribeToGate` | `int gateId` | Subscribe to a specific gate's events |
| `UnsubscribeFromGate` | `int gateId` | Unsubscribe from a gate |

## 5.3 SignalR Payload Schemas

**NotificationDto:**
```json
{
  "id": 1,
  "type": "Emergency",
  "priority": "Critical",
  "title": "🚨 Emergency Alert",
  "body": "Intrusion detected!",
  "createdAt": "2026-05-30T11:30:00Z",
  "gateId": 1
}
```

**EmergencyAlertDto:**
```json
{
  "gateId": 1,
  "type": "LaserIntrusion",
  "severity": "Critical",
  "description": "Intrusion detected at Gate 1",
  "occurredAt": "2026-05-30T11:30:00Z"
}
```

**GateSessionDto:**
```json
{
  "id": 1,
  "sessionToken": "0193a5d2-...",
  "gateId": 1,
  "status": "AIPassed",
  "currentStep": "Fingerprint",
  "memberName": "Ahmed Ali",
  "isSilentAlarm": false,
  "startedAt": "2026-05-30T10:00:00Z"
}
```

---

# 6. Enum Reference

## SessionStatus
| Value | Name | Description |
|-------|------|-------------|
| 0 | `PasswordPending` | Waiting for password |
| 10 | `PasswordPassed` | Password validated |
| 20 | `AIPending` | Waiting for AI result |
| 30 | `AIPassed` | AI recognized the face |
| 40 | `FingerprintPending` | Waiting for fingerprint |
| 50 | `FingerprintPassed` | Fingerprint matched |
| 100 | `Completed` | Session fully completed |
| -1 | `Failed` | Session failed |
| -2 | `Emergency` | Emergency triggered |
| -3 | `Expired` | Session timed out (5 min) |

## SessionResult
| Value | Name |
|-------|------|
| 0 | `Pending` |
| 1 | `Granted` |
| 2 | `DeniedPassword` |
| 3 | `DeniedAI` |
| 4 | `DeniedFingerprint` |
| 5 | `EmergencyTriggered` |
| 6 | `Expired` |

## CommandType
`OpenGate`, `ActivateBuzzer`, `StopBuzzer`, `CaptureImage`, `CaptureFingerprint`

## CommandStatus
`Pending` → `Sent` → `Acknowledged` | `Failed`

## GateStatus
`Online`, `Offline`, `Maintenance`

## AITrainingStatus
`NotTrained`, `Training`, `Trained`, `Failed`

---

# 7. Integration Quick-Start Guides

## 7.1 For Mobile Developers (Flutter)

```
1. POST /Auth/login                          → Get JWT token
2. Store token, use in Authorization header
3. Connect to SignalR hub at /hubs/gate       → Real-time events
4. GET /api/MobileCommands/gates/status       → Show gate dashboard
5. GET /api/MobileCommands/notifications      → Show notification feed
6. POST /api/MobileCommands/gates/{id}/open   → Remote open button
7. POST /api/MobileCommands/register-fcm-token → Enable push notifications
```

## 7.2 For AI Engineers (Python)

```
1. GET /api/Members/all-members-ai            → Get all members for training
2. Train face recognition model
3. POST /api/AICallback/training-complete      → Report training status
4. When image received from device:
   POST /api/AICallback/recognition-result     → Send recognition result
```

## 7.3 For Embedded/IoT Developers (ESP32)

```
LOOP (every 2-3 sec):
  GET /api/Device/pending-commands?gateId=X    → Check for commands
  Execute any commands received
  POST /api/Device/acknowledge-command         → Acknowledge each

ON KEYPAD INPUT:
  POST /api/Device/validate-password           → Send password
  If success → activate camera → wait for AI

ON FINGERPRINT SCAN:
  POST /api/Device/verify-fingerprint          → Send fingerprint
  If commands.openGate → unlock door

ON LASER BREAK:
  POST /api/Device/laser-intrusion             → Report intrusion

PERIODIC (every 30 sec):
  POST /api/Device/heartbeat                   → Health check
```

---

# 8. DeviceCommandsDto — Command Object Reference

Every device-facing response includes a `commands` object. The device should inspect each flag:

```json
{
  "openGate": false,           // Unlock the gate
  "activateCamera": false,     // Turn on camera for image capture  
  "activateBuzzer": false,     // Activate alarm buzzer
  "stopBuzzer": false,         // Stop alarm buzzer
  "captureFingerprint": false, // Activate fingerprint scanner
  "buzzerDurationSeconds": 0,  // How long to buzz (seconds)
  "delaySeconds": 0,           // Wait before executing
  "expectedMemberId": null     // Which member to expect for fingerprint
}
```

---

# 9. Background Jobs (Hangfire)

| Job ID | Schedule | Description |
|--------|----------|-------------|
| `expire-stale-sessions` | Every minute | Marks `GateSessions` older than 5 minutes (still `Pending`) as `Expired` |

**Dashboard:** Available at `/jobs` for monitoring job status.

---

# 10. Error Response Format

All errors follow the RFC 7807 Problem Details format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110",
  "title": "Not Found",
  "status": 404,
  "detail": "Gate not found",
  "errors": {}
}
```

Validation errors (FluentValidation):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Password": ["'Password' must be at least 8 characters."]
  }
}
```
