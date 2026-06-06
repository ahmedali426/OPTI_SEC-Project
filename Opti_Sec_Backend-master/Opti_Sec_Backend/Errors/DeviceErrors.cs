namespace Opti_Sec_Backend.Errors;

public static class DeviceErrors
{
    public static readonly Error InvalidDevice =
        new("Device.InvalidDevice", "Invalid device ID or API key", StatusCodes.Status401Unauthorized);

    public static readonly Error GateNotFound =
        new("Device.GateNotFound", "Gate not found for this device", StatusCodes.Status404NotFound);

    public static readonly Error CommandNotFound =
        new("Device.CommandNotFound", "Device command not found", StatusCodes.Status404NotFound);
}
