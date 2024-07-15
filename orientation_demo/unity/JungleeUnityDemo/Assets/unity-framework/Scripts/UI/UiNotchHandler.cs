using System;
using XcelerateGames.AssetLoading;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiNotchHandler : MonoBehaviour
    {
        [SerializeField] private string _Id = null;

        [SerializeField] private ScreenOrientation[] _OrientationsToHandle = new ScreenOrientation[] {ScreenOrientation.LandscapeLeft};

        private Vector3 mDefaultOffset = Vector3.zero;

        private static DeviceData mDeviceData = null;

        private bool ContainsOrientation(ScreenOrientation orientation)
        {
            return _OrientationsToHandle.Contains(orientation);
        }

        void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            mDefaultOffset = transform.localPosition;

            if (NeedsAdjustment())
            {
                Orientation.OnOrientationChange += OnOrientationChange;
                OnOrientationChange(Screen.orientation);
            }
#endif
        }

        private void OnDestroy()
        {
            Orientation.OnOrientationChange -= OnOrientationChange;
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            if (NeedsAdjustment())
            {
                Adjust(orientation);
            }
        }

        private void Adjust(ScreenOrientation orientation)
        {
            DeviceConfig deviceConfig = GetConfig();
            if (deviceConfig != null)
            {
                if(ContainsOrientation(orientation))
                    transform.localPosition = mDefaultOffset + new Vector3(deviceConfig.offset.x, deviceConfig.offset.y, 0);
                else
                    transform.localPosition = mDefaultOffset;
            }
            else
                Debug.LogError($"Could not find config for id : {_Id} for {gameObject.GetObjectPath()}", this);
        }

        [ContextMenu("Reload")]
        private void Reload()
        {
            mDeviceData = null;
            Adjust(Screen.orientation);
        }

        private bool NeedsAdjustment()
        {
            //Check if the UI needs adjustment on this device
            //return (GameDataConfig.GetString("notch_devices", string.Empty).Contains(SystemInfo.deviceModel));
            string[] temp = { "MacBookPro16,1", "OnePlus ONEPLUS A6013", "OnePlus ONEPLUS A6010",
            "MacBookPro11,3", "Xiaomi Redmi 6 Pro", "80XL(LENOVO)", "asus ASUS_Z01RD",
            "motorola moto g(7) power", "HUAWEI LYA-L29", "HUAWEI EML-L29", "Xiaomi Redmi Note 7"};
            return temp.Contains(SystemInfo.deviceModel);
        }


        private DeviceConfig GetConfig()
        {
            try
            {
                if (mDeviceData == null)
                {
                    string assetName = "notch_device_config_" + PlatformUtilities.GetCurrentPlatform();
                    TextAsset textAsset = ResourceManager.LoadFromResources<TextAsset>(assetName);
                    if (textAsset != null)
                    {
                        NotchConfigData notchConfigData = textAsset.text.FromJson<NotchConfigData>();
                        mDeviceData = Array.Find(notchConfigData.devices, e => e.deviceModel.Contains(SystemInfo.deviceModel));
                    }
                    else
                    {
                        XDebug.LogException("Failed to load " + assetName + " from Resources folder.");
                        return null;
                    }
                }
                if (mDeviceData != null)
                {
                    return Array.Find(mDeviceData.deviceConfigs, e => e.id == _Id);
                }
                else
                    XDebug.LogException("Could not find DeviceData for " + SystemInfo.deviceModel);
                return null;
            }
            catch (Exception e)
            {
                XDebug.LogException(e);
                return null;
            }
        }
    }
}
