# Opti_Sec_Backend — API Endpoints Documentation

> **System Overview:** Opti_Sec_Backend is a physical security gate access system with **3-step multi-factor authentication** (Password → AI Face Recognition → Fingerprint), emergency handling, real-time notifications via SignalR, and mobile app control.

---

## Table of Contents

- [Security Workflow Overview](#security-workflow-overview)
- [Authentication & Authorization Summary](#authentication--authorization-summary)
- [1. AuthController — `Auth/`](#1-authcontroller--auth)
- [2. ClientsController — `api/Clients`](#2-clientscontroller--apiclients)
- [3. GatesController — `api/Gates`](#3-gatescontroller--apigates)
- [4. MembersController — `api/Members`](#4-memberscontroller--apimembers)
- [5. RolesController — `api/Roles`](#5-rolescontroller--apiroles)
- [6. DeviceController — `api/Device`](#6-devicecontroller--apidevice)
- [7. AICallbackController — `api/AICallback`](#7-aicallbackcontroller--apiaicallback)
- [8. AccessLogsController — `api/AccessLogs`](#8-accesslogscontroller--apiaccesslogs)
- [9. MobileCommandsController — `api/MobileCommands`](#9-mobilecommandscontroller--apimobilecommands)
- [SignalR Hub — GateHub](#signalr-hub--gatehub)
- [Enums Reference](#enums-reference)

---

## Security Workflow Overview

The system follows a strict **3-step gate access flow** orchestrated by the `GateAccessOrchestrator`:

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────────────┐     ┌──────────────┐
│  STEP 1          │     │  STEP 2               │     │  STEP 3                  │     │  RESULT       │
│  Password Entry  │────▶│  AI Face Recognition  │────▶│  Fingerprint Verify      │────▶│  Access       │
│  (Device)        │     │  (AI Service Callback) │     │  (Device)                │     │  Granted/     │
│                  │     │                        │     │                          │     │  Denied       │
│  POST /Device/   │     │  POST /AICallback/     │     │  POST /Device/           │     │               │
│  validate-       │     │  recognition-result    │     │  verify-fingerprint      │     │               │
│  password        │     │                        │     │                          │     │               │
└─────────────────┘     └──────────────────────┘     └─────────────────────────┘     └──────────────┘
```

### How It Works:

1. **Password Step:** A person enters a password on the physical device. The device sends it to the backend. The backend checks if it's the correct password, a silent alarm code, or wrong.
   - ✅ **Correct** → Creates a session, tells the device to activate the camera for face scan.
   - 🔕 **Silent Alarm** → Same response as correct (device must not reveal the alarm), but sends a critical silent alarm notification to the owner.
   - ❌ **Wrong** → Tracks failed attempts. After 3 failures within 5 minutes → triggers emergency, activates buzzer.

2. **AI Face Recognition Step:** The device captures a face image and sends it to an external AI service. The AI service calls back the backend with the result.
   - ✅ **Authorized** (confidence ≥ 0.85) → Advances to fingerprint step, tells device to capture fingerprint.
   - ❌ **Unauthorized** → Retries up to max attempts, then triggers emergency.

3. **Fingerprint Step:** The device captures a fingerprint and sends it to the backend for verification against the matched member's stored template.
   - ✅ **Match** → Access granted! Gate opens, access log created, success notification sent.
   - ❌ **No Match** → Retries up to max attempts, then triggers emergency + buzzer.

### Emergency Handling:
- **Password Breach:** 3 wrong passwords in 5 minutes → emergency + buzzer.
- **AI Failed:** Max AI attempts failed → emergency.
- **Fingerprint Failed:** Max fingerprint attempts failed → emergency + buzzer.
- **Laser Intrusion:** Physical intrusion detected → Critical emergency + 30s buzzer + unauthorized access log.

---

## Authentication & Authorization Summary

| Controller | Default Auth | Notes |
|-----------|-------------|-------|
| `AuthController` | ❌ None | Public — login/register endpoints |
| `AICallbackController` | ❌ None | Called by external AI service |
| `DeviceController` | ❌ None | Called by physical IoT devices |
| `ClientsController` | 🔒 Admin Role | Some endpoints override to `[AllowAnonymous]` |
| `RolesController` | 🔒 Admin Role | Full admin access required |
| `GatesController` | 🔒 Client Role | All operations scoped to the authenticated client |
| `MembersController` | 🔒 Client Role | Some endpoints override to `[AllowAnonymous]` |
| `AccessLogsController` | 🔒 Client Role | `CheckOrCreate` is `[AllowAnonymous]` |
| `MobileCommandsController` | 🔒 Client Role | Mobile app commands for gate control |

---

## 1. AuthController — `Auth/`

> Handles user authentication: login, token refresh, password reset.
> **No authentication required** for any of these endpoints.

---

### 1.1 `POST Auth/login`

**Purpose:** Authenticates a user with email and password, returns a JWT token + refresh token.

**How it works:**
1. Receives the user's email and password.
2. Uses ASP.NET Identity `SignInManager` to validate credentials.
3. If valid, generates a JWT access token and a refresh token (14-day expiry).
4. Returns user info along with both tokens.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

**Success Response (200):**
```json
{
  "id": "user-guid",
  "email": "user@example.com",
  "fName": "John",
  "lName": "Doe",
  "token": "eyJhbGciOi...",
  "expireIn": 30,
  "refreshToken": "random-refresh-token-string",
  "refreshTokenExpiration": "2026-06-11T20:00:00Z",
  "roles": ["Client"]
}
```

**Error Response:** Problem details with error description.

---

### 1.2 `POST Auth/refresh`

**Purpose:** Refreshes an expired JWT using a valid refresh token.

**How it works:**
1. Receives the expired JWT token and the refresh token.
2. Validates the refresh token is active and not expired.
3. Revokes the old refresh token and issues a new JWT + new refresh token.

**Request Body:**
```json
{
  "token": "expired-jwt-token",
  "refreshToken": "current-refresh-token"
}
```

**Success Response (200):** Same shape as the login response with new tokens.

---

### 1.3 `POST Auth/revoke-refresh-token`

**Purpose:** Revokes a refresh token so it can no longer be used.

**How it works:**
1. Finds the refresh token in the database.
2. Marks it as revoked with a timestamp.
3. The token can no longer be used to obtain new JWTs.

**Request Body:**
```json
{
  "token": "jwt-token",
  "refreshToken": "refresh-token-to-revoke"
}
```

**Success Response (200):** Empty OK response.

---

### 1.4 `POST Auth/forget-password`

**Purpose:** Initiates the forgot-password flow by sending a reset code to the user's email.

**How it works:**
1. Looks up the user by email.
2. Generates a 6-digit OTP code.
3. Stores the code with a 60-minute expiry.
4. Sends the code to the user's email via SMTP.

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

---

### 1.5 `POST Auth/reset-password`

**Purpose:** Resets the user's password using the OTP code received via email.

**How it works:**
1. Validates the email, code, and checks the code hasn't expired.
2. Uses ASP.NET Identity to reset the password.
3. Clears the stored OTP code.

**Request Body:**
```json
{
  "email": "user@example.com",
  "code": "123456",
  "newPassword": "NewSecurePassword123"
}
```

**Success Response (200):** Empty OK response.

---

## 2. ClientsController — `api/Clients`

> CRUD operations for clients (end-users who own gates and members).
> **Requires Admin role** unless otherwise specified.

---

### 2.1 `POST api/Clients`

**Purpose:** Creates a new client with a user account.

**How it works:**
1. Receives client data as form data (supports file upload for profile image).
2. Creates a new `ApplicationUser` in ASP.NET Identity.
3. Assigns the "Member" role to the user.
4. Uploads the profile image to `wwwroot/Images/`.
5. Creates the `Client` entity linked to the user.
6. Sends a welcome email to the client.

**Auth:** 🔒 Admin

**Request (multipart/form-data):**
| Field | Type | Required |
|-------|------|----------|
| `fName` | string | ✅ |
| `lName` | string | ✅ |
| `email` | string | ✅ |
| `userName` | string | ✅ |
| `password` | string | ✅ |
| `phoneNumber` | string | ✅ |
| `image` | file | ✅ |

**Success Response (200):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "userName": "johndoe",
  "phoneNumber": "01012345678",
  "imageUrl": "/Images/guid-filename.jpg"
}
```

---

### 2.2 `GET api/Clients?search={query}`

**Purpose:** Retrieves all clients with optional search filtering.

**How it works:**
1. Queries all non-deleted clients.
2. If `search` is provided, filters by first name or last name (contains match).
3. Returns a simplified list with id, name, and image.

**Auth:** 🔒 Admin

**Query Parameters:**
| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `search` | string | ❌ | Filter clients by name |

**Success Response (200):**
```json
[
  { "id": 1, "name": "John Doe", "imageUrl": "/Images/img.jpg" },
  { "id": 2, "name": "Jane Smith", "imageUrl": "/Images/img2.jpg" }
]
```

---

### 2.3 `GET api/Clients/all-clients-ai`

**Purpose:** Returns all clients formatted for the AI service consumption.

**How it works:**
1. Returns ALL clients including soft-deleted ones (AI needs the full picture).
2. Includes `isDeleted` flag so the AI knows which are active.
3. No authentication required since the AI service calls this.

**Auth:** 🔓 AllowAnonymous

**Success Response (200):**
```json
[
  { "id": 1, "name": "John Doe", "userName": "johndoe", "imageUrl": "/Images/img.jpg", "isDeleted": false },
  { "id": 2, "name": "Jane Smith", "userName": "janesmith", "imageUrl": "/Images/img2.jpg", "isDeleted": true }
]
```

---

### 2.4 `GET api/Clients/{id}`

**Purpose:** Retrieves a single client by their ID.

**How it works:**
1. Looks up the client by ID.
2. Returns full client details including email, username, phone, and image.

**Auth:** 🔒 Admin

**Route Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Client ID |

**Success Response (200):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "userName": "johndoe",
  "phoneNumber": "01012345678",
  "imageUrl": "/Images/img.jpg"
}
```

---

### 2.5 `PUT api/Clients/{id}`

**Purpose:** Updates an existing client's information.

**How it works:**
1. Finds the client by ID.
2. Updates the provided fields (name, email, username, phone).
3. If a new image is uploaded, replaces the old one.
4. Saves changes to the database.

**Auth:** 🔒 Admin

**Request (multipart/form-data):**
| Field | Type | Required |
|-------|------|----------|
| `name` | string | ✅ |
| `email` | string | ✅ |
| `userName` | string | ✅ |
| `phoneNumber` | string | ✅ |
| `image` | file | ❌ |

**Success Response (204):** No Content.

---

### 2.6 `DELETE api/Clients/{id}`

**Purpose:** Permanently deletes a client and all associated data.

**How it works:**
1. Finds the client by ID.
2. **Hard deletes** the client entity, all its members, and the linked user account.
3. Removes the profile image from disk.

**Auth:** 🔒 Admin

**Success Response (204):** No Content.

---

### 2.7 `GET api/Clients/clients-count`

**Purpose:** Returns the total count of active (non-deleted) clients.

**Auth:** 🔒 Admin

**Success Response (200):**
```json
5
```

---

## 3. GatesController — `api/Gates`

> CRUD operations for security gates. All operations are **scoped to the authenticated client** (extracted from JWT).
> **Requires Client role.**

---

### 3.1 `POST api/Gates`

**Purpose:** Creates a new gate for the current authenticated client.

**How it works:**
1. Extracts the `userId` from the JWT token.
2. Creates a new gate linked to the client's account.
3. Gate names must be globally unique.
4. Each gate stores a `PasswordHash` and `SilentAlarmHash` for the access workflow.

**Auth:** 🔒 Client

**Request Body:**
```json
{
  "name": "Main Entrance",
  "location": "Building A, Floor 1"
}
```

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Main Entrance",
  "location": "Building A, Floor 1"
}
```

---

### 3.2 `GET api/Gates`

**Purpose:** Gets all gates belonging to the current authenticated client.

**How it works:**
1. Extracts `userId` from JWT.
2. Queries all non-deleted gates for the client.

**Auth:** 🔒 Client

**Success Response (200):**
```json
[
  { "id": 1, "name": "Main Entrance", "location": "Building A" },
  { "id": 2, "name": "Back Door", "location": "Building A" }
]
```

---

### 3.3 `GET api/Gates/{id}`

**Purpose:** Gets a specific gate by ID for the current client.

**Auth:** 🔒 Client

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Main Entrance",
  "location": "Building A, Floor 1"
}
```

---

### 3.4 `PUT api/Gates/{id}`

**Purpose:** Updates a specific gate's name and/or location.

**Auth:** 🔒 Client

**Request Body:**
```json
{
  "name": "Main Entrance (Updated)",
  "location": "Building B, Floor 2"
}
```

**Success Response (204):** No Content.

---

### 3.5 `DELETE api/Gates/{id}`

**Purpose:** Soft-deletes a gate for the current client.

**How it works:**
1. Sets `IsDeleted = true` on the gate.
2. The gate data is preserved for audit purposes but excluded from active queries.

**Auth:** 🔒 Client

**Success Response (204):** No Content.

---

### 3.6 `GET api/Gates/count-gate`

**Purpose:** Returns the count of active (non-deleted) gates for the current client.

**Auth:** 🔒 Client

**Success Response (200):**
```json
3
```

---

## 4. MembersController — `api/Members`

> CRUD operations for members (people authorized to access gates).
> All operations are **scoped to the authenticated client**.
> **Requires Client role** unless otherwise specified.

---

### 4.1 `POST api/Members`

**Purpose:** Creates a new member under the current client's account.

**How it works:**
1. Receives member data as form data (includes profile photo for AI training).
2. Uploads the image to `wwwroot/Images/`.
3. Creates the member entity linked to the client.
4. Username must be globally unique across all members.
5. The image is later used by the AI service for face recognition training.

**Auth:** 🔒 Client

**Request (multipart/form-data):**
| Field | Type | Required |
|-------|------|----------|
| `fName` | string | ✅ |
| `lName` | string | ✅ |
| `userName` | string | ✅ |
| `phone` | string | ✅ |
| `image` | file | ✅ |

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Ahmed Ali",
  "userName": "ahmed_ali",
  "phone": "01098765432",
  "imageUrl": "/Images/guid-filename.jpg"
}
```

---

### 4.2 `GET api/Members?search={query}`

**Purpose:** Gets all members for the current client with optional search.

**How it works:**
1. Queries all non-deleted members for the client.
2. If `search` is provided, filters by first or last name.

**Auth:** 🔒 Client

**Success Response (200):**
```json
[
  { "id": 1, "name": "Ahmed Ali", "imageUrl": "/Images/img.jpg" },
  { "id": 2, "name": "Sara Mohamed", "imageUrl": "/Images/img2.jpg" }
]
```

---

### 4.3 `GET api/Members/all-members-ai`

**Purpose:** Returns all members formatted for the AI service.

**How it works:**
1. Returns ALL members including soft-deleted ones.
2. The AI service uses this to sync its face recognition models.

**Auth:** 🔓 AllowAnonymous

**Success Response (200):**
```json
[
  { "id": 1, "name": "Ahmed Ali", "userName": "ahmed_ali", "imageUrl": "/Images/img.jpg", "isDeleted": false }
]
```

---

### 4.4 `GET api/Members/{id}`

**Purpose:** Gets a specific member by ID for the current client.

**Auth:** 🔒 Client

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Ahmed Ali",
  "userName": "ahmed_ali",
  "phone": "01098765432",
  "imageUrl": "/Images/img.jpg"
}
```

---

### 4.5 `PUT api/Members/{id}`

**Purpose:** Updates a member's information.

**Auth:** 🔒 Client

**Request (multipart/form-data):**
| Field | Type | Required |
|-------|------|----------|
| `name` | string | ✅ |
| `userName` | string | ✅ |
| `phone` | string | ✅ |
| `image` | file | ❌ |

**Success Response (204):** No Content.

---

### 4.6 `DELETE api/Members/{id}`

**Purpose:** Soft-deletes a member.

**How it works:**
Sets `IsDeleted = true`. Data is preserved for audit trail and access logs.

**Auth:** 🔒 Client

**Success Response (204):** No Content.

---

### 4.7 `GET api/Members/member-count`

**Purpose:** Returns the count of active members for the current client.

**Auth:** 🔒 Client

**Success Response (200):**
```json
7
```

---

### 4.8 `POST api/Members/set-fingerprint`

**Purpose:** Registers a fingerprint template for a member.

**How it works:**
1. Receives the member ID and fingerprint template string.
2. Validates that the fingerprint template is unique across all members (no duplicates).
3. Stores the template on the member entity for later verification during the access flow.

**Auth:** 🔓 AllowAnonymous (called by the physical device)

**Request Body:**
```json
{
  "memberId": 1,
  "fingerprintTemplate": "base64-encoded-fingerprint-template-data"
}
```

**Success Response (200):** Empty OK response.

---

## 5. RolesController — `api/Roles`

> Manages application roles (Admin, Client, etc.).
> **Requires Admin role.**

---

### 5.1 `GET api/Roles?includeDisabled={bool}`

**Purpose:** Gets all application roles.

**How it works:**
1. Queries roles from ASP.NET Identity `RoleManager`.
2. If `includeDisabled` is true, includes soft-deleted/disabled roles.

**Auth:** 🔒 Admin

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `includeDisabled` | bool | false | Include disabled roles |

**Success Response (200):**
```json
[
  { "id": "guid-1", "name": "Admin", "isDeleted": false },
  { "id": "guid-2", "name": "Client", "isDeleted": false }
]
```

---

### 5.2 `GET api/Roles/{id}`

**Purpose:** Gets a single role by its ID.

**Auth:** 🔒 Admin

**Success Response (200):**
```json
{
  "id": "guid-1",
  "name": "Admin",
  "isDeleted": false
}
```

---

### 5.3 `POST api/Roles`

**Purpose:** Creates a new role.

**Auth:** 🔒 Admin

**Request Body:**
```json
{
  "name": "Supervisor"
}
```

**Success Response (201):** Created — returns the new role with a `Location` header.

---

### 5.4 `PUT api/Roles/{id}`

**Purpose:** Updates an existing role's name.

**Auth:** 🔒 Admin

**Request Body:**
```json
{
  "name": "SuperAdmin"
}
```

**Success Response (204):** No Content.

---

### 5.5 `PUT api/Roles/{id}/Toggle`

**Purpose:** Toggles a role's enabled/disabled status.

**How it works:**
Flips the `IsDeleted` flag. Disabled roles cannot be assigned to new users.

**Auth:** 🔒 Admin

**Success Response (204):** No Content.

---

## 6. DeviceController — `api/Device`

> Endpoints called by the **physical IoT security device** (embedded hardware).
> **No authentication required** (device communicates directly).

---

### 6.1 `POST api/Device/validate-password` — ⚡ STEP 1

**Purpose:** Validates a password entered on the physical device. This is the **first step** of the 3-step security workflow.

**How it works:**
1. Receives the gate ID, entered password, timestamp, and device identifier.
2. Hashes the password (SHA256) and compares it against the gate's stored hashes.
3. **Three possible outcomes:**

| Outcome | What Happens | Device Commands |
|---------|-------------|-----------------|
| ✅ Correct Password | Creates a new `GateSession`, advances to AI step | `ActivateCamera = true` (start face scan) |
| 🔕 Silent Alarm | Same response as correct (device must NOT know), but sends critical notification to owner | `ActivateCamera = true` (same as correct) |
| ❌ Wrong Password | Tracks the failed attempt. After 3 failures in 5 min → triggers emergency | `ActivateBuzzer = true` (on emergency only) |

**Request Body:**
```json
{
  "gateId": 1,
  "password": "entered-password",
  "timestamp": "2026-05-28T20:00:00Z",
  "deviceId": "device-001"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "sessionToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "passwordStatus": "Correct",
  "nextStep": "AI Face Recognition",
  "attemptNumber": 1,
  "remainingAttempts": 2,
  "emergency": false,
  "commands": {
    "openGate": false,
    "activateCamera": true,
    "activateBuzzer": false,
    "stopBuzzer": false,
    "captureFingerprint": false,
    "buzzerDurationSeconds": 0,
    "delaySeconds": 0,
    "expectedMemberId": null
  }
}
```

---

### 6.2 `POST api/Device/verify-fingerprint` — ⚡ STEP 3

**Purpose:** Verifies a fingerprint after AI face recognition confirms a member. This is the **third and final step**.

**How it works:**
1. Receives session token, gate ID, matched member ID, fingerprint template, and device ID.
2. Looks up the session by token, validates it's at the fingerprint step.
3. Does a direct string comparison of the fingerprint template against the member's stored template.
4. Cross-validates that the member ID matches the one identified by AI in Step 2.
5. **Outcomes:**

| Outcome | What Happens | Device Commands |
|---------|-------------|-----------------|
| ✅ Match | Access granted! Creates authorized access log, completes session | `OpenGate = true` |
| ❌ No Match | Tracks attempt. After max attempts → emergency | `ActivateBuzzer = true` (on emergency) |

**Request Body:**
```json
{
  "sessionToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gateId": 1,
  "memberId": 5,
  "fingerprintTemplate": "base64-fingerprint-data",
  "deviceId": "device-001"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "accessGranted": true,
  "memberName": "Ahmed Ali",
  "attemptNumber": 1,
  "remainingAttempts": 2,
  "emergency": false,
  "commands": {
    "openGate": true,
    "activateCamera": false,
    "activateBuzzer": false,
    "stopBuzzer": false,
    "captureFingerprint": false,
    "buzzerDurationSeconds": 0,
    "delaySeconds": 0,
    "expectedMemberId": null
  }
}
```

---

### 6.3 `POST api/Device/laser-intrusion`

**Purpose:** Handles laser sensor intrusion detection events from the physical device.

**How it works:**
1. A laser tripwire sensor detects physical intrusion at a gate.
2. Triggers a **Critical** severity emergency event.
3. Activates the buzzer for 30 seconds.
4. Creates an unauthorized access log entry.
5. Sends an emergency alert to the gate owner via SignalR and push notification.

**Request Body:**
```json
{
  "gateId": 1,
  "deviceId": "device-001",
  "timestamp": "2026-05-28T20:30:00Z"
}
```

**Success Response (200):**
```json
{
  "received": true,
  "emergencyId": 12,
  "commands": {
    "openGate": false,
    "activateCamera": false,
    "activateBuzzer": true,
    "stopBuzzer": false,
    "captureFingerprint": false,
    "buzzerDurationSeconds": 30,
    "delaySeconds": 0,
    "expectedMemberId": null
  }
}
```

---

### 6.4 `POST api/Device/heartbeat`

**Purpose:** Receives periodic heartbeat pings from the embedded device to monitor connectivity.

**How it works:**
1. The device sends a ping at regular intervals.
2. The server acknowledges with the current server time.
3. This is used to track device online/offline status.

**Request Body:**
```json
{
  "gateId": 1,
  "deviceId": "device-001",
  "timestamp": "2026-05-28T20:00:00Z"
}
```

**Success Response (200):**
```json
{
  "received": true,
  "serverTime": "2026-05-28T20:00:01Z"
}
```

---

### 6.5 `POST api/Device/acknowledge-command`

**Purpose:** Device acknowledges that it received and processed a command sent from the backend.

**How it works:**
1. The backend sends commands to devices (open gate, activate buzzer, etc.).
2. The device calls this endpoint to confirm it received the command.
3. Updates the command status from `Pending`/`Sent` to `Acknowledged` with a timestamp.
4. A `CommandAcknowledged` event is pushed to the gate's SignalR group.

**Request Body:**
```json
{
  "commandId": 42,
  "deviceId": "device-001"
}
```

**Success Response (200):**
```json
{
  "acknowledged": true
}
```

---

## 7. AICallbackController — `api/AICallback`

> Endpoints called by the **external AI face recognition service** to report results back.
> **No authentication required.**

---

### 7.1 `POST api/AICallback/recognition-result` — ⚡ STEP 2

**Purpose:** Receives the face recognition result from the AI service. This is the **second step** of the access workflow.

**How it works:**
1. The AI service processes a face image captured by the device camera.
2. It calls this endpoint with the recognition result.
3. The `GateAccessOrchestrator` processes the result:
   - If `isAuthorized` is true and confidence ≥ 0.85 → advances session to fingerprint step.
   - If not authorized → retries (up to max attempts) or triggers emergency.
4. Returns commands for the device (e.g., capture fingerprint, retry camera).

**Request Body:**
```json
{
  "sessionToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gateId": 1,
  "isAuthorized": true,
  "confidenceScore": 0.92,
  "matchedMemberId": 5,
  "imageUrl": "/Images/captured-face.jpg",
  "processingTimeMs": 1500
}
```

**Success Response (200):**
```json
{
  "received": true,
  "nextStep": "Fingerprint Verification",
  "memberId": 5,
  "commands": {
    "openGate": false,
    "activateCamera": false,
    "activateBuzzer": false,
    "stopBuzzer": false,
    "captureFingerprint": true,
    "buzzerDurationSeconds": 0,
    "delaySeconds": 0,
    "expectedMemberId": 5
  }
}
```

---

### 7.2 `POST api/AICallback/training-complete`

**Purpose:** Receives a callback when the AI service finishes training a member's face recognition model.

**How it works:**
1. When a new member is created with a photo, the AI service trains a face model.
2. Upon completion, the AI service calls this endpoint.
3. If training was successful:
   - Sets the member's `AITrainingStatus` to `Trained`.
   - Stores the `EmbeddingVector` (face encoding).
   - Records `LastTrainedAt` timestamp.
4. If training failed: Sets status to `Failed`.

**Request Body:**
```json
{
  "memberId": 5,
  "success": true,
  "embeddingVector": "serialized-face-embedding-data",
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

**Error Response (404):** If the member ID doesn't exist.

---

## 8. AccessLogsController — `api/AccessLogs`

> Manages access log entries (records of who accessed which gate and when).
> **Requires Client role** unless otherwise specified.

---

### 8.1 `POST api/AccessLogs`

**Purpose:** Creates or checks an access log entry.

**How it works:**
1. Receives gate ID, optional username, fingerprint template, and image.
2. If a username and fingerprint are provided, validates the member exists and fingerprint matches.
3. Creates an authorized or unauthorized access log entry accordingly.

**Auth:** 🔓 AllowAnonymous

**Request (multipart/form-data):**
| Field | Type | Required |
|-------|------|----------|
| `gateId` | int | ✅ |
| `userName` | string | ❌ |
| `fingerprintTemplate` | string | ❌ |
| `image` | file | ❌ |

**Success Response (200):** Empty OK response.

---

### 8.2 `GET api/AccessLogs/authorized`

**Purpose:** Gets all **authorized** (successful) access logs for the current client's gates.

**How it works:**
1. Extracts `userId` from JWT.
2. Queries access logs where `IsAuthorized = true` for the client's gates.
3. Returns results ordered by most recent first.

**Auth:** 🔒 Client

**Success Response (200):**
```json
[
  {
    "id": 1,
    "name": "Ahmed Ali",
    "gateName": "Main Entrance",
    "imageUrl": "/Images/face-capture.jpg",
    "dateOnly": "2026-05-28",
    "timeOnly": "20:15:30"
  }
]
```

---

### 8.3 `GET api/AccessLogs/unauthorized`

**Purpose:** Gets all **unauthorized** (denied/intrusion) access logs for the current client's gates.

**How it works:**
Same as authorized, but filters for `IsAuthorized = false`.

**Auth:** 🔒 Client

**Success Response (200):** Same shape as authorized logs.

---

## 9. MobileCommandsController — `api/MobileCommands`

> Endpoints for the **mobile app** to monitor and control gates in real-time.
> **Requires Client role.**

---

### 9.1 `GET api/MobileCommands/gates/status`

**Purpose:** Gets real-time status of all gates belonging to the current client.

**How it works:**
1. Queries the database directly for all of the client's gates.
2. For each gate, returns:
   - Current status (Online/Offline/Maintenance/Emergency).
   - Any pending session token (if an access session is in progress).
   - Whether the buzzer is currently active (has an unresolved emergency with buzzer).
   - Count of unresolved emergencies.
   - Last heartbeat timestamp.

**Auth:** 🔒 Client

**Success Response (200):**
```json
[
  {
    "gateId": 1,
    "name": "Main Entrance",
    "status": "Online",
    "currentSessionToken": null,
    "buzzerActive": false,
    "activeEmergencies": 0,
    "lastHeartbeat": "2026-05-28T20:00:00Z"
  }
]
```

---

### 9.2 `GET api/MobileCommands/sessions?page={page}&pageSize={size}`

**Purpose:** Gets paginated session history for all of the client's gates.

**How it works:**
1. Queries all gate sessions for the client's gates.
2. Ordered by most recent first.
3. Includes detailed information about each step's pass/fail status.

**Auth:** 🔒 Client

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page |

**Success Response (200):**
```json
[
  {
    "id": 1,
    "sessionToken": "a1b2c3d4-...",
    "status": "Completed",
    "result": "Granted",
    "memberName": "Ahmed Ali",
    "isSilentAlarm": false,
    "startedAt": "2026-05-28T20:00:00Z",
    "completedAt": "2026-05-28T20:01:30Z",
    "passwordPassed": true,
    "aiPassed": true,
    "fingerprintPassed": true
  }
]
```

---

### 9.3 `GET api/MobileCommands/emergencies?activeOnly={bool}`

**Purpose:** Gets emergency events for the client's gates.

**How it works:**
1. Queries all emergency events for the client's gates.
2. If `activeOnly=true`, filters to only unresolved emergencies.
3. Ordered by most recent first.

**Auth:** 🔒 Client

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `activeOnly` | bool | false | Only show unresolved emergencies |

**Success Response (200):**
```json
[
  {
    "id": 1,
    "gateId": 1,
    "gateName": "Main Entrance",
    "type": "PasswordBreach",
    "severity": "High",
    "description": "3 failed password attempts detected",
    "buzzerActivated": true,
    "occurredAt": "2026-05-28T20:10:00Z",
    "isResolved": false,
    "resolvedAt": null
  }
]
```

---

### 9.4 `POST api/MobileCommands/emergencies/{emergencyId}/resolve`

**Purpose:** Resolves (dismisses) an emergency event.

**How it works:**
1. Finds the emergency by ID.
2. Marks it as resolved with the current timestamp and the resolving user's ID.

**Auth:** 🔒 Client

**Route Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `emergencyId` | int | Emergency event ID |

**Success Response (200):**
```json
{
  "resolved": true
}
```

---

### 9.5 `POST api/MobileCommands/gates/{gateId}/stop-buzzer`

**Purpose:** Sends a command to stop the buzzer on a specific gate.

**How it works:**
1. Creates a `StopBuzzer` device command in the database.
2. Sends the command to the device (via Firebase Realtime DB — planned).
3. Sends a `BuzzerStopped` notification via SignalR.

**Auth:** 🔒 Client

**Success Response (200):**
```json
{
  "stopped": true
}
```

---

### 9.6 `POST api/MobileCommands/gates/{gateId}/open`

**Purpose:** Remotely opens a gate from the mobile app.

**How it works:**
1. Creates an `OpenGate` device command in the database.
2. Sends the command to the physical device.
3. Sends a `GateOpened` notification via SignalR.

**Auth:** 🔒 Client

**Success Response (200):**
```json
{
  "opened": true
}
```

---

### 9.7 `POST api/MobileCommands/register-fcm-token`

**Purpose:** Registers or updates the Firebase Cloud Messaging (FCM) token for push notifications.

**How it works:**
1. Extracts `userId` from JWT.
2. Finds the user in the database.
3. Updates their `FcmToken` field.
4. This token is used to send push notifications to the user's mobile device.

**Auth:** 🔒 Client

**Request Body:**
```json
{
  "fcmToken": "firebase-cloud-messaging-token-string",
  "platform": "android"
}
```

**Success Response (200):**
```json
{
  "registered": true
}
```

**Error Response (404):** If the user doesn't exist.

---

### 9.8 `GET api/MobileCommands/notifications?page={page}&pageSize={size}`

**Purpose:** Gets paginated notification history for the current user.

**How it works:**
1. Queries all notifications for the authenticated user.
2. Ordered by most recent first.
3. Includes notification type, priority, title, body, and related gate info.

**Auth:** 🔒 Client

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page |

**Success Response (200):**
```json
[
  {
    "id": 1,
    "type": "FingerprintSuccess",
    "priority": "Normal",
    "title": "Access Granted",
    "body": "Ahmed Ali accessed Main Entrance",
    "isSent": true,
    "createdAt": "2026-05-28T20:01:30Z",
    "sentAt": "2026-05-28T20:01:31Z",
    "gateId": 1,
    "gateName": "Main Entrance"
  }
]
```

---

## SignalR Hub — GateHub

> Real-time communication hub at `/hubs/gate`. Requires authentication.

### Connection

```
URL: wss://your-domain/hubs/gate?access_token={JWT}
```

### Server Methods (Client → Server)

| Method | Parameters | Description |
|--------|-----------|-------------|
| `SubscribeToGate` | `int gateId` | Subscribe to real-time updates for a specific gate |
| `UnsubscribeFromGate` | `int gateId` | Unsubscribe from a gate's updates |

> **Auto-subscribe:** On connection, the hub automatically subscribes the user to ALL their gates.

### Client Methods (Server → Client)

| Event | Payload | When It Fires |
|-------|---------|---------------|
| `GateStatusChanged` | `gateId`, `status` | Gate goes online/offline/emergency |
| `SessionUpdated` | `GateSessionDto` | Access session progresses through steps |
| `EmergencyAlert` | `EmergencyAlertDto` | Emergency event triggered (intrusion, failed attempts) |
| `NotificationReceived` | `NotificationDto` | Any notification sent to the user |
| `CommandAcknowledged` | `commandId`, `commandType` | Device confirms command receipt |
| `BuzzerStatusChanged` | `gateId`, `isActive` | Buzzer turned on or off |

---

## Enums Reference

### Gate & Session

| Enum | Values |
|------|--------|
| **GateStatus** | `Online`, `Offline`, `Maintenance`, `Emergency` |
| **SessionStatus** | `PasswordPending(0)`, `PasswordPassed(10)`, `AIPending(20)`, `AIPassed(30)`, `FingerprintPending(40)`, `FingerprintPassed(50)`, `Completed(100)`, `Failed(-1)`, `Emergency(-2)`, `Expired(-3)` |
| **SessionStep** | `Password(1)`, `AI(2)`, `Fingerprint(3)`, `Complete(4)` |
| **SessionResult** | `Pending(0)`, `Granted(1)`, `DeniedPassword(2)`, `DeniedAI(3)`, `DeniedFingerprint(4)`, `EmergencyTriggered(5)`, `Expired(6)` |

### Security & Access

| Enum | Values |
|------|--------|
| **PasswordStatus** | `Correct(0)`, `SilentAlarm(1)`, `Wrong(2)` |
| **AccessMethod** | `Password`, `AI`, `Fingerprint`, `Manual`, `Remote` |
| **AITrainingStatus** | `NotTrained`, `Pending`, `Trained`, `Failed` |

### Emergency

| Enum | Values |
|------|--------|
| **EmergencyType** | `PasswordBreach`, `AIFailed`, `FingerprintFailed`, `LaserIntrusion` |
| **EmergencySeverity** | `Warning(0)`, `High(1)`, `Critical(2)` |

### Device Commands

| Enum | Values |
|------|--------|
| **CommandType** | `OpenGate`, `ActivateBuzzer`, `StopBuzzer`, `CaptureImage`, `CaptureFingerprint` |
| **CommandSource** | `Backend`, `MobileApp` |
| **CommandStatus** | `Pending`, `Sent`, `Acknowledged`, `Failed` |

### Notifications

| Enum | Values |
|------|--------|
| **NotificationType** | `PasswordSuccess`, `SilentAlarm`, `WrongPassword`, `Emergency`, `AIAuthorized`, `AIUnauthorized`, `FingerprintSuccess`, `FingerprintFailed`, `LaserIntrusion`, `GateOpened`, `BuzzerActivated`, `BuzzerStopped`, `MemberRegistered`, `FingerprintRegistered` |
| **NotificationPriority** | `Normal(0)`, `High(1)`, `Critical(2)` |

---

## Endpoint Count Summary

| Controller | Endpoints |
|-----------|-----------|
| AuthController | 5 |
| ClientsController | 7 |
| GatesController | 6 |
| MembersController | 8 |
| RolesController | 5 |
| DeviceController | 5 |
| AICallbackController | 2 |
| AccessLogsController | 3 |
| MobileCommandsController | 8 |
| **Total** | **49** |
