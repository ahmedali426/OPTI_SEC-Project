namespace Opti_Sec_Backend.Settings;

public class AIServiceSettings
{
    public const string SectionName = "AIService";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string TrainingEndpoint { get; set; } = "/api/training/start";
}
