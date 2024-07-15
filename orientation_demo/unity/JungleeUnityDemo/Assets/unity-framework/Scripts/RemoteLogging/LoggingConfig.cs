using Newtonsoft.Json;

namespace XcelerateGames.RemoteLogging
{
    [System.Serializable]
    public class LoggingConfig
    {
        [JsonProperty] public bool enable = true;
        [JsonProperty] public bool showLog = true;
        [JsonProperty] public bool showWarning = true;
        [JsonProperty] public bool showError = true;
        [JsonProperty] public bool showStackTrace = true;
        [JsonProperty] public bool skipDuplicates = true;
    }
}
