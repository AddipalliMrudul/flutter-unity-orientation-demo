using UnityEngine;

namespace XcelerateGames.UI
{
    public class NotchConfigData
    {
        public DeviceData[] devices;
    }

    public class DeviceData
    {
        public string deviceModel { get; set; }
        public DeviceConfig[] deviceConfigs;
    }

    public class DeviceConfig
    {
        public string id { get; set; }
        public Vector2 offset { get; set; }
    }
}
