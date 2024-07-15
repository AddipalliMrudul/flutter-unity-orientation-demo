using UnityEngine;
using UnityEngine.Profiling;
using XcelerateGames.AssetLoading;
using XcelerateGames.Testing;

namespace XcelerateGames
{
    public class MemoryManager : MonoBehaviour, IExposeData
    {
#if AUTOMATION_ENABLED
        private PerformanceData mPerformanceData = new PerformanceData();
#endif

        [SerializeField] float _UnloadWaitTime = 10f;
        //Should we destroy on load?
        [SerializeField] bool _DontDestroyOnLoad = false;

        private static MemoryManager mInstance = null;

#if AUTOMATION_ENABLED
        public object ExposeData => mPerformanceData;
#else
        public object ExposeData => null;
#endif

        private void Awake()
        {
            if (mInstance == null)
            {
                mInstance = this;
                if(_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
                Application.lowMemory += ReceivedMemoryWarning;

#if AUTOMATION_ENABLED
            mPerformanceData.memory = new MemoryData();
            AddMeta();
            
            enabled = false;
            InvokeRepeating("UpdateData", 0, 1); 
#endif
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if(mInstance == this)
            {
                Application.lowMemory -= ReceivedMemoryWarning;
            }
        }

        private void ReceivedMemoryWarning()
        {
            XDebug.LogException("MemoryManager RECEIVED LOW MEMORY WARNING!");
#if AUTOMATION_ENABLED
            mPerformanceData.lowMemory = true;
#endif
            ResourceManager.UnloadUnusedAssets();
            enabled = true;
        }

        private void Update()
        {
            if (_UnloadWaitTime > 0f)
                _UnloadWaitTime -= Time.deltaTime;
            else
                enabled = false;
        }

#if AUTOMATION_ENABLED

        private void AddMeta()
        {
            mPerformanceData.deviceMeta = new DeviceMeta();
            mPerformanceData.deviceMeta.unityVersion = Application.unityVersion.ToString();
            mPerformanceData.deviceMeta.platform = Application.platform.ToString();
            mPerformanceData.deviceMeta.os = PlatformUtilities.GetOSVersion();
            mPerformanceData.deviceMeta.appVersion = ProductSettings.GetProductVersion();
            mPerformanceData.deviceMeta.deviceName = SystemInfo.deviceName;
            mPerformanceData.deviceMeta.deviceModel = SystemInfo.deviceModel;
            mPerformanceData.deviceMeta.systemMemorySize = SystemInfo.systemMemorySize + " MB";
            mPerformanceData.deviceMeta.graphicsMemorySize = SystemInfo.systemMemorySize + " MB";
            mPerformanceData.deviceMeta.buildNumber = ProductSettings.pInstance._BuildNumber;
            mPerformanceData.deviceMeta.env = PlatformUtilities.GetEnvironment().ToString();
        }

        private void UpdateData()
        {
            mPerformanceData.batteryLevel = SystemInfo.batteryLevel;
            mPerformanceData.exceptionCount = LogConsole.pNumExceptions;
            if (LogConsole.pInstance != null)
                mPerformanceData.logFilePath = LogConsole.FilePath;

            mPerformanceData.memory.usedHeapSize = Profiler.usedHeapSizeLong;
            mPerformanceData.memory.usedMonoSize = Profiler.GetMonoUsedSizeLong();
            mPerformanceData.memory.reservedMemory = Profiler.GetTotalReservedMemoryLong();
            mPerformanceData.memory.unusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong();
        }
#endif
    }
}