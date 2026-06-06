namespace Opti_Sec_Backend.Contracts.Mobile;

// to connect the mobile app to the fcm token for push notifications
public record RegisterFcmTokenRequest(
    // the fcm token to register for push notifications
    string FcmToken,
    // the platform of the mobile device (e.g., "Android", "iOS") to handle platform-specific push notification logic if needed
    string Platform
);
