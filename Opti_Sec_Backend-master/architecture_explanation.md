# OptiSec Architecture — Why We Built Everything This Way

> This document explains the **reasoning** behind every architectural decision in the OptiSec Smart Biometric Security Gate System. Use it for your **graduation defense** to justify every design choice.

---

## Table of Contents

1. [Overall Architecture](#1-overall-architecture)
2. [Why These Enums](#2-why-these-enums)
3. [Why These Entities](#3-why-these-entities)
4. [Why We Modified Existing Entities](#4-why-we-modified-existing-entities)
5. [Why These Services](#5-why-these-services)
6. [Why The GateAccessOrchestrator Pattern](#6-why-the-gateaccessorchestrator-pattern)
7. [Why SignalR Hub](#7-why-signalr-hub)
8. [Why These Controllers](#8-why-these-controllers)
9. [Why These DTOs](#9-why-these-dtos)
10. [Why These EF Configurations](#10-why-these-ef-configurations)
11. [Why The Silent Alarm Design](#11-why-the-silent-alarm-design)
12. [Why The 3-Attempt Escalation Rule](#12-why-the-3-attempt-escalation-rule)
13. [Why These Settings Classes](#13-why-these-settings-classes)
14. [Why These Error Classes](#14-why-these-error-classes)
15. [Why This DI Registration Order](#15-why-this-di-registration-order)

---

## 1. Overall Architecture

### Why Vertical Slice Architecture?

We organized the code by **feature** (e.g., `PasswordServices/`, `EmergencyServices/`, `SessionServices/`) instead of by technical layer (e.g., all interfaces in one folder, all implementations in another).

**Reason:** In a security system, each feature has its own lifecycle. When you need to fix a bug in password validation, you go to `PasswordServices/` and find EVERYTHING there — the interface and the implementation. You don't jump between 5 folders. This is critical for a **graduation project** because:
- It's easier to explain during defense ("this folder handles passwords, this one handles emergencies")
- It's easier to test each feature independently
- It follows **Clean Architecture** principles used by enterprise companies

### Why the Orchestrator Pattern?

Instead of putting all logic in one big controller, we created the `GateAccessOrchestrator` which coordinates between 6 smaller services.

**Reason:** The gate access workflow has **3 sequential steps** (Password → AI → Fingerprint), and each step can trigger side effects (notifications, emergencies, device commands). If we put all this in a controller:
- The controller would be 500+ lines
- Testing would require mocking HTTP context
- Changing one step would risk breaking others

With the Orchestrator:
- Each service does ONE thing well (Single Responsibility Principle)
- The orchestrator just **coordinates** the flow
- You can test each service independently

---

## 2. Why These Enums

### `SessionStatus` — Why 10 values with specific numbers?

```csharp
PasswordPending = 0,    // Start
PasswordPassed = 10,    // After Step 1
AIPending = 20,         // Waiting for AI
AIPassed = 30,          // After Step 2
FingerprintPending = 40, // Waiting for fingerprint
FingerprintPassed = 50, // After Step 3
Completed = 100,        // Success
Failed = -1,            // Normal failure
Emergency = -2,         // Emergency triggered
Expired = -3            // Session timed out
```

**Reason:** We use **gaps between numbers** (0, 10, 20, 30...) so that if we need to add a sub-state later (e.g., `PasswordRetrying = 5`), we can insert it without renumbering everything. Negative numbers for failures make it easy to check: `if (status < 0)` → something went wrong. This is a common enterprise pattern.

### `PasswordStatus` — Why only 3 values?

```csharp
Correct = 0,
SilentAlarm = 1,
Wrong = 2
```

**Reason:** From the device's perspective, there are only 3 possible outcomes when someone enters a password. The device doesn't need to know about attempt counts or emergencies — that's the backend's job. We keep enums **minimal** and specific to their domain.

### `EmergencyType` — Why 4 types?

```csharp
PasswordBreach,      // 3 wrong passwords
AIFailed,            // 3 unrecognized faces
FingerprintFailed,   // 3 fingerprint mismatches
LaserIntrusion       // Laser beam broken
```

**Reason:** Each emergency type requires **different handling**:
- `PasswordBreach` → Someone is guessing passwords → Activate buzzer + lock gate
- `AIFailed` → Unknown person → Capture image + alert owner
- `FingerprintFailed` → Someone passed face check but not fingerprint → Possible spoofing attempt
- `LaserIntrusion` → Physical bypass attempt → Immediate critical alert

By having separate types, the mobile app can show **different UI** for each (red icon for intrusion, yellow for password breach, etc.)

### `CommandType` — Why separate from `CommandStatus`?

**Reason:** `CommandType` describes **WHAT** to do (open gate, activate buzzer). `CommandStatus` describes **WHERE in delivery** the command is (pending, sent, acknowledged). These are two different concerns. A command can be `OpenGate` + `Pending` or `OpenGate` + `Acknowledged`. Mixing them into one enum would create an explosion of values like `OpenGatePending`, `OpenGateSent`, `BuzzerPending`, etc.

### `NotificationType` — Why 14 values?

**Reason:** Each notification type maps to a **different UI treatment** in the mobile app:
- `SilentAlarm` → Special hidden alert with red background
- `Emergency` → Full-screen alert with sound
- `PasswordSuccess` → Green toast notification
- `GateOpened` → Simple info badge

If we used a generic "Notification" without types, the mobile app wouldn't know how to display each one differently.

### `AccessMethod` — Why track how access happened?

**Reason:** For **audit reports**. The client (house owner) needs to know: "Did they enter through password + face + fingerprint (normal flow)? Or did the owner open remotely from the app (Remote)? Or was it a manual override?" This is critical for security compliance and for the graduation project's analytics dashboard.

### `AITrainingStatus` — Why does Member need this?

**Reason:** AI face recognition requires **training** — the AI model needs to learn each member's face. This enum tracks whether:
- `NotTrained` → Member just registered, can't use face recognition yet
- `Pending` → Training started, waiting for AI service
- `Trained` → Ready for recognition
- `Failed` → Training failed, needs retry

Without this, we wouldn't know if a member can use the gate or not.

---

## 3. Why These Entities

### `GateSession` — Why is this the most important entity?

**Reason:** The gate access workflow has **3 steps that happen at different times**:
1. Password entered (second 0)
2. AI processes face (seconds 3-10)
3. Fingerprint scanned (seconds 15-20)

Without `GateSession`, we have NO way to connect these 3 steps together. When the AI sends back results, we need to know: "Which password attempt does this belong to? Which gate? Did they use the silent alarm?"

`GateSession` is the **glue** that holds the entire workflow together. It stores:
- `SessionToken` (GUID) → The device sends this with every step so we can match requests
- `IsSilentAlarm` → Set in Step 1, needed in Step 3 to create the right AccessLog
- `MemberId` → Set in Step 2 (AI identifies who), used in Step 3 (fingerprint must match same person)
- `AIAttemptCount` / `FingerprintAttemptCount` → Track retries per session

### `PasswordAttempt` — Why store every attempt?

**Reason:** Security audit requirement. If someone tries to brute-force a gate's password, we need a **forensic trail** of:
- When each attempt happened (`AttemptedAt`)
- From which device (`DeviceId`)
- Whether it triggered an emergency (`TriggeredEmergency`)
- The hashed password they tried (`PasswordHashAttempt`) — for pattern analysis

This is also needed for the **sliding window** logic: "How many wrong attempts in the last 5 minutes?"

### `EmergencyEvent` — Why separate from AccessLog?

**Reason:** An emergency is NOT the same as an access attempt:
- An access attempt is "someone tried to enter"
- An emergency is "something dangerous happened that needs human response"

Emergencies need:
- `IsResolved` / `ResolvedAt` / `ResolvedByUserId` → Tracking who resolved it and when
- `BuzzerActivated` / `BuzzerDurationSeconds` → Managing the buzzer
- `Severity` → Prioritizing alerts

Access logs don't need any of this. Mixing them would violate the **Single Responsibility Principle**.

### `DeviceCommand` — Why store commands in the database?

**Reason:** Three critical reasons:
1. **Audit trail** → "Who told the gate to open at 3 AM?" → Check DeviceCommands table
2. **Acknowledgment tracking** → We need to know if the device actually received and executed the command. If `Status` stays `Sent` for too long, something is wrong.
3. **Source tracking** → Was the gate opened by the backend (automated) or by the mobile app (human)? This matters for security reviews.

### `Notification` — Why not just send and forget?

**Reason:** Push notifications (FCM) can **fail**. The phone might be offline, the FCM token might be expired, or Google's servers might be down. By storing every notification:
- `IsSent` / `RetryCount` → We can retry failed notifications via a background job
- `ErrorMessage` → We can debug delivery failures
- History → The mobile app can show a "notification center" listing past alerts

### `AIValidationLog` — Why log every AI attempt?

**Reason:** The AI service might:
- Return a false positive (recognizes wrong person) → We need `ConfidenceScore` to review
- Be slow (`ResponseTimeMs` → monitoring)
- Fail completely (`ErrorMessage` → debugging)
- Return raw data that we need for analysis (`AIRawResponseJson`)

This log is essential for **improving the AI model** over time and for proving to the graduation committee that you have proper AI integration auditing.

### `FingerprintValidationLog` — Why log fingerprint attempts?

**Reason:** Cross-validation audit. If the AI says "this is Member #5" but the fingerprint doesn't match Member #5, we have a **security incident**. The log stores:
- `ExpectedMemberId` → Who the AI identified
- `IsMatch` → Did the fingerprint confirm it?
- `FailureReason` → Why it failed (template mismatch, no template on file, etc.)

This is critical for the **biometric cross-validation** requirement you specified.

---

## 4. Why We Modified Existing Entities

### Gate — Why add `PasswordHash` and `SilentAlarmHash`?

**Reason:** Each gate needs its own password. We store **SHA256 hashes**, never plain text, because:
- If the database is compromised, attackers can't read passwords
- SHA256 is a one-way function — you can verify but never reverse
- Two separate hashes because the silent alarm password MUST be different from the normal password

### Gate — Why add `DeviceId` and `DeviceApiKey`?

**Reason:** Device authentication. When the ESP32/Raspberry Pi sends a request, we need to verify it's actually our device, not someone spoofing requests. `DeviceId` identifies the device, `DeviceApiKey` authenticates it. This prevents:
- Fake gate requests from unauthorized devices
- Man-in-the-middle attacks
- Replay attacks (combined with timestamps)

### Gate — Why add `MaxFailedAttempts = 3`?

**Reason:** Different gates might need different security levels. A bank vault gate might allow only 1 attempt, while a home gate allows 3. Making this configurable **per gate** instead of hardcoded is enterprise-grade design.

### Member — Why add `FaceEmbedding` and `AITrainingStatus`?

**Reason:** `FaceEmbedding` stores the AI-generated face vector — a mathematical representation of the member's face. This is needed for:
- Faster recognition (compare vectors instead of images)
- Offline capability (if the AI service is temporarily down)

`AITrainingStatus` prevents the system from trying to recognize a member whose face hasn't been trained yet.

### AccessLog — Why add `IsSilentAlarm`, `AccessMethod`, `GateSessionId`?

**Reason:**
- `IsSilentAlarm` → When reviewing access logs, the owner needs to see which entries were under duress. This flag is **only visible in the mobile app**, never sent to the device.
- `AccessMethod` → For analytics: "70% of accesses are through normal flow, 20% are remote opens"
- `GateSessionId` → Links the access log to the full session history, so you can see the complete workflow that led to this access.

### ApplicationUser — Why add `FcmToken`?

**Reason:** Firebase Cloud Messaging needs a **device token** to send push notifications to a specific phone. When the user logs into the mobile app, the app sends its FCM token to the backend. We store it on the user so we can send notifications whenever security events happen, even when the app is in the background.

---

## 5. Why These Services

### `SessionService` — Why separate from the Orchestrator?

**Reason:** Session management (create, update step, complete) is a **CRUD concern**. The orchestrator is a **workflow concern**. The orchestrator says "the password was correct, create a session." The SessionService does the actual database work. This separation means:
- You can test session creation without testing the whole workflow
- You can reuse SessionService in other places (e.g., a admin dashboard that views sessions)

### `PasswordService` — Why not just compare strings in the Orchestrator?

**Reason:** Password validation involves:
1. Hashing the input with SHA256
2. Comparing against both normal and silent alarm hashes
3. Counting recent failed attempts in a time window
4. Recording the attempt in the database
5. Determining if emergency should trigger

This is too much logic for the Orchestrator. By isolating it:
- Password hashing logic is in ONE place (security best practice)
- The sliding window counting logic is testable
- If you change hashing algorithm (e.g., SHA256 → bcrypt), you change ONE file

### `EmergencyService` — Why a dedicated service?

**Reason:** Emergencies are triggered from **4 different places**:
1. Password breach (3 wrong passwords)
2. AI failure (3 unrecognized faces)
3. Fingerprint failure (3 mismatches)
4. Laser intrusion

Without a service, you'd duplicate emergency creation code in 4 places. With `EmergencyService.TriggerEmergencyAsync()`, every caller uses the same logic, ensuring consistent:
- Buzzer activation
- Severity assignment
- Database recording

### `NotificationService` — Why not just call SignalR directly?

**Reason:** Notification dispatch involves:
1. Finding the gate's owner (Client → User)
2. Creating a Notification record in the database
3. Pushing via SignalR to connected clients
4. (Future) Pushing via FCM for background notifications

If controllers called SignalR directly, they'd need to know about all these steps. The `NotificationService` encapsulates all of this. It also provides specialized methods like `SendSilentAlarmAsync()` which sets the right priority, title, and type automatically.

### `DeviceCommandService` — Why not just write to Firebase directly?

**Reason:** Every command sent to a device must be:
1. Recorded in the database (audit trail)
2. Sent to Firebase RTDB (actual delivery)
3. Tracked for acknowledgment

The service ensures all 3 happen consistently. It also provides a clean API:
- `SendOpenGateAsync()` instead of manually constructing JSON payloads
- `AcknowledgeCommandAsync()` for the device to confirm receipt

### `FingerprintService` — Why a separate service for one comparison?

**Reason:** Today it's a simple string comparison. Tomorrow it might involve:
- Biometric template matching algorithms
- Hardware security module (HSM) integration
- Fingerprint quality scoring
- Anti-spoofing detection

By isolating it behind an interface, you can swap the implementation without touching ANY other code. This is the **Open/Closed Principle** — open for extension, closed for modification.

---

## 6. Why The GateAccessOrchestrator Pattern

### The Orchestrator is the "Brain" of the System

```
ValidatePasswordAsync()     → Step 1
ProcessAIResultAsync()      → Step 2
VerifyFingerprintAsync()    → Step 3
HandleLaserIntrusionAsync() → Emergency
```

**Why this pattern?**

1. **Single point of workflow control** → If you need to change the order of steps (e.g., fingerprint before AI), you change ONE file.

2. **Transaction consistency** → Each method handles the complete flow for its step: validate → log → update session → notify → return commands. No partial states.

3. **Clear error boundaries** → If Step 2 fails, the orchestrator decides whether to retry, escalate, or abort. Controllers don't make these decisions.

4. **Testability** → Mock the 6 sub-services and test every workflow path:
   - Correct password → session created
   - Silent alarm → session created + hidden notification
   - 3rd wrong password → emergency + buzzer
   - AI authorized → proceed to fingerprint
   - Fingerprint match → gate opens

### Why does the Orchestrator return DTOs, not Results?

**Reason:** The device needs specific instructions: "activate camera", "capture fingerprint", "activate buzzer for 30 seconds". A generic `Result<bool>` can't carry this information. The response DTOs (`PasswordValidationResponse`, `FingerprintVerificationResponse`) are specifically designed to tell the device exactly what to do next.

---

## 7. Why SignalR Hub

### Why SignalR instead of polling?

**Reason:** Security events need **instant** delivery. If the mobile app polled every 5 seconds:
- 5 seconds of delay before seeing an intrusion alert
- Wasted bandwidth from constant HTTP requests
- Server load from thousands of polling clients

SignalR provides:
- **Instant push** → Event appears in < 100ms
- **Efficient** → Single persistent WebSocket connection
- **Group-based** → Each client only receives events for their own gates

### Why `Groups` based on Gate IDs?

```csharp
await Groups.AddToGroupAsync(Context.ConnectionId, $"gate-{gateId}");
```

**Reason:** When an emergency happens at Gate #5, we only want to notify the OWNER of Gate #5, not all connected users. By grouping connections by gate ID:
- `hubContext.Clients.Group("gate-5").EmergencyAlert(...)` → Only Gate 5's owner sees it
- One client can subscribe to multiple gates (if they own multiple)
- New connections automatically join their gates on connect

### Why a strongly-typed hub (`Hub<IGateHubClient>`)?

**Reason:** Without the interface, you'd write:
```csharp
await Clients.Group("gate-5").SendAsync("EmergencyAlert", data); // string-based, no compile-time check
```

With the interface:
```csharp
await Clients.Group("gate-5").EmergencyAlert(data); // compile-time checked, no typos possible
```

If you rename a method, the compiler catches all callers. This prevents runtime errors where you send `"EmergncyAlert"` (typo) and the client never receives it.

---

## 8. Why These Controllers

### `DeviceController` — Why anonymous (no `[Authorize]`)?

**Reason:** The ESP32/Raspberry Pi device **doesn't have a JWT token**. It's not a human user — it's an embedded device. It authenticates differently:
- Using `DeviceId` + `DeviceApiKey` in the request body
- (Future) Using an API key header middleware

Making it `[Authorize]` would require the device to obtain and refresh JWT tokens, which is impractical for embedded systems with limited resources.

### `AICallbackController` — Why anonymous?

**Reason:** The AI service is a **separate microservice** (Python/Flask). It calls our backend when face recognition completes. This is a **service-to-service** call, not a user-initiated request. It uses:
- API key authentication (in headers)
- Shared `SessionToken` (GUID) to identify which session the result belongs to

### `MobileCommandsController` — Why `[Authorize(Roles = "Client")]`?

**Reason:** Only the **gate owner** (Client) should be able to:
- See gate statuses
- Stop buzzers
- Open gates remotely
- View emergencies
- View notifications

The Admin shouldn't open someone's gate. Regular members shouldn't stop someone's buzzer. Role-based authorization ensures only the owner controls their gates.

### Why separate controllers instead of one big one?

**Reason:** Each controller serves a **different consumer**:
- `DeviceController` → Consumed by ESP32 (embedded C++)
- `AICallbackController` → Consumed by Python AI service
- `MobileCommandsController` → Consumed by Flutter/React Native app

They have different:
- Authentication methods
- Request/response formats
- Rate limiting requirements
- Error handling needs

Mixing them would make the controller impossible to maintain.

---

## 9. Why These DTOs

### Why `DeviceCommandsDto` in every response?

```csharp
public record DeviceCommandsDto(
    bool OpenGate = false,
    bool ActivateCamera = false,
    bool ActivateBuzzer = false,
    ...
);
```

**Reason:** The device is **simple** — it reads the response and executes commands. By including a `Commands` object in every response, the device code is trivial:

```cpp
if (response.commands.openGate) openGate();
if (response.commands.activateBuzzer) activateBuzzer(response.commands.buzzerDurationSeconds);
if (response.commands.activateCamera) startCamera();
```

The device doesn't need to understand workflow logic. It just executes commands.

### Why `SessionToken` (GUID) instead of session ID (int)?

**Reason:** Security. If we used `sessionId = 42`, an attacker could:
- Guess other session IDs (41, 43, 44...)
- Send fingerprint results for someone else's session

A GUID v7 (`019d0158-5874-7a35-8457-344e6faccb52`) is:
- Practically unguessable (128-bit random)
- Time-ordered (v7 includes timestamp for sorting)
- Globally unique (no collisions across instances)

### Why separate Request and Response DTOs?

**Reason:** The data you SEND is different from the data you RECEIVE:
- `PasswordValidationRequest` has `Password` (sensitive, incoming)
- `PasswordValidationResponse` has `SessionToken`, `NextStep`, `Commands` (outgoing)

If we used one DTO for both, we'd have nullable fields everywhere, and there's a risk of accidentally exposing sensitive data in responses.

---

## 10. Why These EF Configurations

### Why `HasIndex` on composite keys?

```csharp
builder.HasIndex(x => new { x.GateId, x.AttemptedAt });  // PasswordAttempt
builder.HasIndex(x => new { x.GateId, x.IsResolved });   // EmergencyEvent
builder.HasIndex(x => new { x.IsSent, x.RetryCount });   // Notification
```

**Reason:** These are the exact columns we **query by most often**:
- "How many failed attempts at Gate 5 in the last 5 minutes?" → `GateId + AttemptedAt`
- "Any unresolved emergencies at Gate 5?" → `GateId + IsResolved`
- "Any unsent notifications to retry?" → `IsSent + RetryCount`

Without indexes, these queries do **full table scans** which get slower as the table grows. With indexes, they're instant.

### Why `DeleteBehavior.Cascade` for Gate → GateSession?

**Reason:** If a gate is deleted, all its sessions, password attempts, and emergencies are meaningless. Cascade delete cleans them up automatically. This prevents orphaned records.

### Why `DeleteBehavior.SetNull` for GateSession → Member?

**Reason:** If a member is deleted, we still want to keep the session history for **audit purposes**. Setting the `MemberId` to null instead of deleting the session preserves the security audit trail. You can see "someone accessed Gate 5 at 3 PM" even if that member was later removed.

### Why `DeleteBehavior.Restrict` for AIValidationLog → Gate?

**Reason:** AI validation logs should **never be silently deleted** when a gate is removed. These logs are forensic evidence. `Restrict` forces you to explicitly handle the logs before deleting the gate, preventing accidental data loss.

### Why unique index on `SessionToken`?

```csharp
builder.HasIndex(x => x.SessionToken).IsUnique();
```

**Reason:** Two reasons:
1. **Performance** → We query by `SessionToken` for every Step 2 and Step 3 request. A unique index makes this O(log n) instead of O(n).
2. **Data integrity** → Guarantees no two sessions can have the same token, even in race conditions.

---

## 11. Why The Silent Alarm Design

### The Most Critical Security Feature

```
Normal Password  → Response: { success: true, passwordStatus: "Correct" }
Silent Alarm     → Response: { success: true, passwordStatus: "Correct" }  // IDENTICAL!
```

**Reason:** If someone is **holding a gun to the homeowner's head** and forcing them to open the gate, the response to the device MUST be identical. If the device showed "SILENT ALARM" or behaved differently in any way:
- The attacker would know the alarm was triggered
- The homeowner's life would be in danger

So the backend:
1. Creates a normal session (device sees normal behavior)
2. Marks `session.IsSilentAlarm = true` (only in database)
3. Sends a **hidden high-priority notification** to the owner's phone
4. The access log is marked `IsSilentAlarm = true` for later review

The attacker sees: gate opening normally. The police see: silent alarm notification on the owner's phone.

---

## 12. Why The 3-Attempt Escalation Rule

### Progressive Security Response

```
Attempt 1: Wrong password → Normal notification
Attempt 2: Wrong password → High priority notification  
Attempt 3: Wrong password → EMERGENCY (buzzer + critical alert)
```

**Reason:** 
- **1 wrong attempt** → Might be a typo. No need to panic.
- **2 wrong attempts** → Suspicious. Alert the owner with higher priority.
- **3 wrong attempts** → Almost certainly an intruder. Full emergency response.

This applies to ALL 3 steps (password, AI, fingerprint) consistently. The `MaxFailedAttempts = 3` is configurable per gate because different security levels need different thresholds.

### Why a sliding time window (5 minutes)?

```csharp
private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(5);
```

**Reason:** If someone entered 1 wrong password today and 1 wrong password tomorrow, that's probably two different people making typos. But 3 wrong passwords in 5 minutes is clearly a brute-force attempt. The window prevents false alarms from legitimate mistakes spread over time.

---

## 13. Why These Settings Classes

### `FirebaseSettings` — Why a POCO class?

```csharp
public class FirebaseSettings
{
    public string ProjectId { get; set; }
    public string ServiceAccountKeyPath { get; set; }
    public string RealtimeDatabaseUrl { get; set; }
}
```

**Reason:** ASP.NET Core's `IOptions<T>` pattern binds configuration from `appsettings.json` to strongly-typed classes. Without this:
```csharp
// Bad: magic strings, no compile-time checking
var projectId = configuration["Firebase:ProjectId"];
```

With this:
```csharp
// Good: strongly-typed, IDE autocomplete, refactoring support
var projectId = firebaseSettings.Value.ProjectId;
```

### `AIServiceSettings` — Why `ConfidenceThreshold = 0.85`?

**Reason:** 85% confidence means the AI is "pretty sure" this is the right person. Below 85%, there's too much risk of false positives (letting in the wrong person). Above 95% would cause too many false negatives (rejecting the right person in bad lighting). 85% is the industry standard for face recognition in controlled environments (like a gate with a dedicated camera).

This is **configurable** so you can adjust during testing: lower it for demos, raise it for production.

---

## 14. Why These Error Classes

### Why static readonly Error fields?

```csharp
public static class SessionErrors
{
    public static readonly Error NotFound = new("Session.NotFound", "...", 404);
    public static readonly Error Expired = new("Session.Expired", "...", 410);
}
```

**Reason:** This follows the existing codebase's `Error` pattern (from `Abstractions/Error.cs`). Benefits:
- **Consistent error format** across the entire API (code + description + status code)
- **Reusable** → Multiple services can return `SessionErrors.NotFound`
- **No magic strings** → `SessionErrors.NotFound` instead of `new Error("not found")`
- **HTTP status codes baked in** → No need to decide the status code at the controller level

### Why HTTP 410 (Gone) for expired sessions?

**Reason:** 404 means "never existed." 410 means "existed but is no longer available." An expired session DID exist — it just timed out. Using the correct HTTP status code helps the device distinguish between:
- "Session not found" (wrong token → retry with new password)
- "Session expired" (took too long → start completely over)

---

## 15. Why This DI Registration Order

```csharp
// Core services first
services.AddScoped<ISessionService, SessionService>();
services.AddScoped<IPasswordService, PasswordService>();
services.AddScoped<IEmergencyService, EmergencyService>();
services.AddScoped<INotificationService, NotificationService>();
services.AddScoped<IDeviceCommandService, DeviceCommandService>();
services.AddScoped<IFingerprintService, FingerprintService>();

// Orchestrator LAST (depends on all above)
services.AddScoped<IGateAccessOrchestrator, GateAccessOrchestrator>();
```

**Reason:** While DI registration order doesn't technically matter in ASP.NET Core, we register **dependencies before dependents** for code readability. The Orchestrator depends on all 6 services above it, so listing it last makes the dependency graph obvious.

### Why `Scoped` instead of `Singleton` or `Transient`?

- **Scoped** → One instance per HTTP request. This is correct because our services use `ApplicationDbContext`, which is also scoped. If services were `Singleton`, they'd hold stale database contexts. If `Transient`, multiple services in the same request might use different DbContext instances, breaking transactions.

### Why `AddSignalR()` is registered here?

**Reason:** SignalR needs to be registered **before** the app is built (`builder.Build()`). By including it in `AddDependencies()`, it follows the same pattern as all other service registrations, keeping `Program.cs` clean.

---

## Summary: Architecture Principles Applied

| Principle | Where Applied |
|-----------|--------------|
| **Single Responsibility** | Each service does ONE thing (PasswordService validates passwords, EmergencyService handles emergencies) |
| **Open/Closed** | New emergency types can be added without changing existing code |
| **Dependency Inversion** | Controllers depend on interfaces (IGateAccessOrchestrator), not implementations |
| **Separation of Concerns** | DTOs separate transport from domain, Services separate logic from persistence |
| **Defense in Depth** | 3-step validation (password + face + fingerprint), not just one |
| **Audit Everything** | Every attempt, every command, every notification is logged |
| **Fail Secure** | On error, the gate stays CLOSED (never fails open) |
| **Silent Alarm** | Most critical safety feature — response is indistinguishable from normal |
| **Progressive Escalation** | 1 fail = warning, 2 = high alert, 3 = emergency |

> [!TIP]
> During your graduation defense, focus on explaining the **Silent Alarm** design and the **3-step biometric cross-validation**. These are the features that make OptiSec stand out from a simple password-based gate system.
