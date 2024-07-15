#if AUTOMATION_ENABLED
using System;

namespace XcelerateGames
{
    [Serializable]
    public class PerformanceData
    {
        public MemoryData memory = null;
        public float batteryLevel = 0f;
        public int fps = 0;
        public int exceptionCount = 0;
        public bool lowMemory = false;
        public string logFilePath = null;
        public DeviceMeta deviceMeta = null;
    }

    [Serializable]
    public class MemoryData
    {
        public long usedHeapSize;
        public long usedMonoSize;
        public long reservedMemory;
        public long unusedReservedMemory;
    }

    [Serializable]
    public class DeviceMeta
    {
        public string unityVersion;
        public string platform;
        public string os;
        public string appVersion;
        public string buildNumber;
        public string deviceName;
        public string deviceModel;
        public string systemMemorySize;
        public string graphicsMemorySize;
        public string env;
    }
}
#endif //AUTOMATION_ENABLED