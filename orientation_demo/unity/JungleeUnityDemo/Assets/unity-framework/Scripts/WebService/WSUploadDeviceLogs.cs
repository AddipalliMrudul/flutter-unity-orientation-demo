using Newtonsoft.Json;
using XcelerateGames;
using XcelerateGames.WebServices;

namespace JungleeGames.WebServices
{
    public static class WSUploadDeviceLogs
    {
        public class WSDeviceLogsParams: WSParams
        {
            [JsonProperty, JsonConverter(typeof(XGStringEnumConverter))] public Game game = Game.None;
            public long userId = 0;
            public long gameId = 0;
            public long tableId = 0;
            public byte[] data = null;
        }

        public class Response
        {
            public bool success;
            public string error;
        }

        public static WebRequestV2 Upload(WebServiceEventHandler callback, WSDeviceLogsParams parmeters, object userData = null, bool useCompression = false)
        {
            //TODO:Add prod URL here
            string url = "http://172.25.7.233:5001/UploadDeviceLogs";
            //Use local server for debugging purpose only
            //url = "http://127.0.0.1:5001/UploadDeviceLogs";
            parmeters.data = LogConsole.GetLogs();
            WebRequestV2 webRequest = WebRequestV2.Create(url, parmeters, callback, userData, useCompression, true);
            webRequest.StartCoroutine(webRequest.Execute(WebRequestType.POST));
            return webRequest;
        }
    }
}