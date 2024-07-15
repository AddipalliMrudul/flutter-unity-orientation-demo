using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace XcelerateGames
{

    public static class MobileUtilities
    {
#if UNITY_IOS
	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void Alert(string title, string message);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public string GetLocale();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public int GetDeviceMemoryInUse();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public int GetDeviceFreeMemory();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public int GetDeviceTotalMemory();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public float GetCPUUsage();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public bool IsValidCustomURL(string customURL);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public string GetDeviceOSVersion();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void SetUpCookieSettings();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        extern static string uuidForDevice();

#elif UNITY_ANDROID
        static private AndroidJavaClass mPluginClass = null;

        static public void InitAndroid()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            mPluginClass = new AndroidJavaClass("com.androidutilities.Utilities");
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (jc != null)
            {
                AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                if (currentActivity == null)
                    Debug.LogError("Could not find Current Activity! This is bad");
                else
                {
                    Debug.Log("Initialized Android Utility.");
                    mPluginClass.CallStatic("Init", currentActivity);
                }
            }
            else
                Debug.LogError("Could not find JavaClass : com.unity3d.player.UnityPlayer");
        }

        static public void Alert(string title, string message)
        { }

        static public long GetDeviceTotalMemory()
        {
            if (mPluginClass == null)
                return 0;
            return (mPluginClass.CallStatic<long>("GetDeviceTotalMemory"));
        }

        static public string GetLocale()
        {
            if (mPluginClass == null)
                return "en-US";
            string locale = mPluginClass.CallStatic<string>("GetLocale");
            locale = locale.Replace('_', '-');
            return locale;
        }

        static public string getIPAddress()
        { return string.Empty; }

        static public string getMacAddress()
        { return string.Empty; }

        static public long GetDeviceMemoryInUse()
        {
            if (mPluginClass == null)
                return 0;
            return (mPluginClass.CallStatic<long>("GetMemoryInUse"));
        }

        static public long GetDeviceFreeMemory()
        {
            if (mPluginClass == null)
                return 0;
            return (mPluginClass.CallStatic<long>("GetAvailableMemory"));
        }

        static public bool IsValidCustomURL(string customURL)
        {
            if (mPluginClass == null)
            {
                Debug.LogError("IsValidCustomURL::mPluginClass is null");
                return false;
            }

            return (mPluginClass.CallStatic<bool>("IsAppInstalled", customURL));
        }

        static public string GetDeviceOSVersion()
        {
            return SystemInfo.operatingSystem;
            //if (mPluginClass == null)
            //    return string.Empty;
            //return (mPluginClass.CallStatic<string>("GetDeviceOSVersion"));
        }

#else
    static public string GetDeviceOSVersion()
    {
        return SystemInfo.operatingSystem;
    }

    static public int GetDeviceMemoryInUse()
	{ return 0; }

	static public int GetDeviceFreeMemory()
	{ return 0; }

	public static string GetLocale()
	{
#if NETFX_CORE
		return Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();
#else
		return "";
#endif
	}

	static public void Alert(string title, string message)
	{
	}

	static public bool IsValidCustomURL(string customURL)
	{
		return false;
	}

#endif

        public static long GetMemoryInUse()
        {
#if UNITY_EDITOR
            return UnityEngine.Profiling.Profiler.usedHeapSizeLong;
#elif UNITY_IOS
		return GetDeviceMemoryInUse();
#elif UNITY_ANDROID
		return GetDeviceMemoryInUse();
#else
		return 0;
#endif
        }

        public static string GetDeviceUUID()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return uuidForDevice();
#else
            XDebug.LogException("This function should be called only on device");
            return null;
#endif
        }

        //Give manager memory allocation
        //Return in bytes
        public static long GetGCMemory()
        {
            return System.GC.GetTotalMemory(false);
        }

        public static long GetFreeMemory()
        {
#if UNITY_EDITOR
            //For testing purspose only
            return 1024 * 1024 * 100;
#elif UNITY_IOS
		return GetDeviceFreeMemory();
#elif UNITY_ANDROID
		return GetDeviceFreeMemory();
#else
		return 0;
#endif
        }

        public static long GetUnityTotalDeviceMemory()
        {
            long mem = ((long)SystemInfo.graphicsMemorySize + (long)SystemInfo.systemMemorySize);

            // Sometimes mem will become negative when the memory is higher than 3Gb so we are clamping that number
            if (mem < 0) mem = 3 * 1024;
            return mem;
        }

        //Returns system memory in MB
        public static long GetSystemMemory()
        {
#if UNITY_EDITOR
            //If we have selected ForceSD option, then return only 500 MB RAM, this will force the game to load all SD variants
            if (UnityEditor.EditorPrefs.HasKey("ForceSD"))
                return 500;
#endif
            return SystemInfo.systemMemorySize;
        }

        public static float GetDeviceCPUUsage()
        {
#if UNITY_IOS && !UNITY_EDITOR
		return GetCPUUsage();
#else
            return 0;
#endif
        }

        public static string GetOSVersion()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		return GetDeviceOSVersion();
#else
            return "";
#endif
        }
        public static string GetDeviceLocale()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		return GetLocale();
#else
            return "en-US";
#endif
        }

        public static bool IsAppInstalled(string inIdetifier)
        {
            bool isInstalled = false;
#if UNITY_EDITOR
            isInstalled = false;
#elif UNITY_ANDROID
        isInstalled = IsValidCustomURL(inIdetifier);
#elif UNITY_IOS
		isInstalled = IsValidCustomURL(inIdetifier + "://");
#endif
            return isInstalled;
        }

        public static void LauchApp(string packageName)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                if (up != null)
                {
                    AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
                    if (ca != null)
                    {
                        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
                        if (packageManager != null)
                        {
                            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
                            if (launchIntent != null)
                            {
                                ca.Call("startActivity", launchIntent);
                                up.Dispose();
                                ca.Dispose();
                                packageManager.Dispose();
                                launchIntent.Dispose();
                            }
                            else
                                Debug.LogError("Could not find launchIntent");
                        }
                        else
                            Debug.LogError("Could not find PackageManager");
                    }
                    else
                        Debug.LogError("Could not find CurrentActivity");
                }
                else
                    Debug.LogError("Could not find UnityPlayer");
            }
            catch (Exception e)
            {
                XDebug.LogException($"Exception while launching the app {packageName}: {e.Message}\n {e.InnerException}");
            }
#endif //UNITY_ANDROID
        }

        public static bool IsDoubleTap()
        {
            bool result = false;
            float MaxTimeWait = 1;
            float VariancePosition = 1;

            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                float DeltaTime = Input.GetTouch(0).deltaTime;
                float DeltaPositionLenght = Input.GetTouch(0).deltaPosition.magnitude;

                if (DeltaTime > 0 && DeltaTime < MaxTimeWait && DeltaPositionLenght < VariancePosition)
                    result = true;
            }
            return result;
        }

        public static void ShowAppInfo()
        {
#if UNITY_ANDROID
            try
            {
                using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string packageName = currentActivityObject.Call<string>("getPackageName");

                    using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                    using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                    using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                    {
                        intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                        intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                        currentActivityObject.Call("startActivity", intentObject);
                    }
                }
            }
            catch (System.Exception ex)
            {
                XDebug.LogException(ex);
            }
#elif UNITY_IOS
        // Can implement similar functionality for iOS using the Unity iOS APIs
        // However, the exact code will differ, and we might need to use Objective-C code.
#else
        Debug.Log("Location services are not supported on this platform.");
#endif
        }


        public static void ShowRateUs()
        {
#if UNITY_ANDROID
            Application.OpenURL(string.Format("market://details?id={0}", Application.identifier));
#elif UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
#endif
        }

        public static int GetAndroidSDKVersion()
        {
            int sdkVerson = 0;
#if UNITY_ANDROID
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                sdkVerson = version.GetStatic<int>("SDK_INT");
            }
#endif
            return sdkVerson;
        }
    }
}