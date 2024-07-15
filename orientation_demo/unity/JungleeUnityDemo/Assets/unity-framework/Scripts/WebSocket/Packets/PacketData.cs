using Newtonsoft.Json;

namespace XcelerateGames.Socket
{
    public class PacketError
    {
        [JsonProperty("code")] public string code;
        [JsonProperty("msg")] public string msg;
    }
}