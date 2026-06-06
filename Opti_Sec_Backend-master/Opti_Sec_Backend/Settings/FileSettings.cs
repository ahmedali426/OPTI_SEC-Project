namespace Opti_Sec_Backend.Settings;

public static class FileSettings
{
    public const int MaxFileSizeInMB = 1;
    public const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;

    // if the file is contain on this extension i will ignore it 
    // 4D-5A .exe , 2F-2A .js , D0-CF .msi 
    public static readonly string[] BlockedSignatures = ["4D-5A", "2F-2A", "D0-CF"];

    public static readonly string[] AllowedImagesExtensions = [".jpg", ".jpeg", ".png"];

}
