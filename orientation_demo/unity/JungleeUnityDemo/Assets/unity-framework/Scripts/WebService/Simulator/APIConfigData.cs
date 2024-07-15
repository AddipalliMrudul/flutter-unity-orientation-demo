using UnityEngine;

namespace XcelerateGames.WebServices
{
    public enum APIErrorType
    {
        None,
        BeforeHittingServer,
        AfterHittingServer
    }

    [System.Serializable]
    public class APISimulatorData
    {
        //Must be between 0 & 100
        [Range(0, 100)] public int failProbability = 0;
        public float delay = 0f;
        public APIErrorType APIErrorType = APIErrorType.None;
    }

    [System.Serializable]
    public class APIConfigData
    {
        public int maxRetries = 3;
        public float timeOut = 30;
    }

    [System.Serializable]
    public class APIConfig
    {
        public string endpoint;
        public APIConfigData config = null;
        public APISimulatorData simulation = null;
    }
}
