using System.Collections.Generic;
using Newtonsoft.Json;

namespace XcelerateGames.WebServices
{
    //Base class for any API params
    [System.Serializable]
    public class WSParams
    {
    }

    public class WebRequestData
    {
        //To be changed per call
        [JsonIgnore] public string api { get; set; }
        [JsonIgnore] public string ticks { get; set; }
        [JsonIgnore] public string secure { get; set; }

        [JsonProperty("data")] public string data { get; set; }
        [JsonProperty] public string meta { get; set; }

        [JsonIgnore] public Dictionary<string, string> requestHeaders { get; set; }
    }

    [System.Serializable]
    public class WSResponse
    {
    }


    [System.Serializable]
    public class WSError
    {
        public string error = null;
        public int code = 200;
    }
}