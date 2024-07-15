#if UNITY_ANDROID && !UNITY_EDITOR
#define IS_ANDROID
#endif
///Source:https://gist.github.com/aVolpe/707c8cf46b1bb8dfb363, https://gist.github.com/ruzrobert/d98220a3b7f71ccc90403e041967c46b#file-vibration-cs
using System;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Vibration class for Haptic feedback. Works only on Android platform
    /// </summary>
    public static class Vibration
    {
        private static AndroidJavaObject vibrator = null;
        private static AndroidJavaClass vibrationEffectClass = null;
        private static string mMainActivity = "com.unity3d.player.UnityPlayer";
        private const string mStateKey = "DEVICE_VIBRATE";
#if IS_ANDROID
        private static int defaultAmplitude = 255;
        private static int repeatPattern = -1;
#endif
        private static int ApiLevel = 1;
        private static bool doesSupportVibrationEffect => ApiLevel >= 26;
        private static bool isInitialized = false;

        private static bool mIsVibrate = true;
        public static bool VibrationState
        {
            get { return mIsVibrate; }
            set
            {
                mIsVibrate = value;
                PlayerPrefs.SetInt(mStateKey, mIsVibrate == true ? 1 : 0);
                PlayerPrefs.Save();
                if (mIsVibrate)
                    Vibrate(50);
            }
        }

        /// <summary>
        /// Must initialize with the activity name to play haptic feedback
        /// </summary>
        /// <param name="mainActivity">Name of the activity that will be used to play vibration</param>
        public static void Initialize(string mainActivity = null)
        {
            //Debug.Log($"LTS: Vibration Initialize started : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

            mIsVibrate = PlayerPrefs.GetInt(mStateKey, 1) == 1 ? true : false;

            if (!mainActivity.IsNullOrEmpty())
            {
                mMainActivity = mainActivity;
#if !LIVE_BUILD && !BETA_BUILD
                XDebug.Log($"Vibration:Using activity: {mMainActivity}");
#endif
            }

            if (isInitialized)
                return;
#if IS_ANDROID
            using (AndroidJavaClass androidVersionClass = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                ApiLevel = androidVersionClass.GetStatic<int>("SDK_INT");
                Debug.Log($"ApiLevel: {ApiLevel}");
            }

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass(mMainActivity))
            {
                if (unityPlayer == null)
                    XDebug.LogException($"Vibration::Failed to find activity: {mMainActivity}");
                else
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        if (currentActivity != null)
                        {
                            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                            if (doesSupportVibrationEffect)
                            {
                                vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                                defaultAmplitude = Mathf.Clamp(vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE"), 1, 255);
                            }
                        }
                        else
                            XDebug.LogError($"Vibration:Could not MainActivity: {mMainActivity}");
                    }
                }
            }
            if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibration Initialize");

            isInitialized = true;
#endif
            //Debug.Log($"LTS: Vibration Initialize ended : {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

        }

        /// <summary>
        /// Play haptic feedback for the given duration
        /// </summary>
        /// <param name="milliseconds">Duration</param>
        /// <param name="amplitude">amplitude level</param>
        /// <param name="cancel">cancel any vibration being played already</param>
        public static void Vibrate(long milliseconds, int amplitude = -1, bool cancel = false)
        {
            if (!mIsVibrate)
                return;
#if IS_ANDROID
            if (HasVibrator())
            {
                if (cancel) Cancel();
                if (doesSupportVibrationEffect)
                {
                    //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrate 1");

                    if (amplitude == -1 || !HasAmplitudeControl())
                        amplitude = defaultAmplitude;
                    else
                        amplitude = Mathf.Clamp(amplitude, 0, 255);
                    using (AndroidJavaObject effect = CreateEffectOneShot(milliseconds, amplitude))
                    {
                        vibrator.Call("vibrate", effect);
                        //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrate 2");
                    }
                }
                else
                {
                    //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrate 3");
                    if (vibrator != null)
                        vibrator.Call("vibrate", milliseconds);
                }
            }
            else
            {
                if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrater not found, using default vibraton");
                DefaultVibration();
            }
#else
            DefaultVibration();
#endif
        }

        /// <summary>
        /// Play haptic feedback for the given pattern
        /// </summary>
        /// <param name="pattern">Pattern of vibrations</param>
        /// <param name="cancel">cancel any vibration being played already</param>
        public static void Vibrate(long[] pattern, bool cancel = false)
        {
            if (!mIsVibrate)
                return;
#if IS_ANDROID
            
            if (HasVibrator())
            {
                if (cancel) Cancel();
                if (doesSupportVibrationEffect)
                {
                    //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrate 1");

                    using (AndroidJavaObject effect = CreateEffectWaveform(pattern,repeatPattern))
                    {
                        vibrator.Call("vibrate", effect);
                        //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrate 2");
                    }
                }
                else
                {
                    //if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Vibrate 3");
                    if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Does Not Support Vibration Effect");
                }
            }
            else
            {
                // if (Debug.Log(Debug.Mask.Game)) Debug.Log("Vibrater not found");
                if (XDebug.CanLog(XDebug.Mask.Game)) Debug.Log("Dose Not Has a Vibrator");
            }
#endif
        }

        /// <summary>
        /// Used internally if the haptic feedback isnt supported. Android OS 10 is minimum required OS for haptic feddback
        /// </summary>
        public static void DefaultVibration()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// Cancel vibration
        /// </summary>
        public static void Cancel()
        {
#if IS_ANDROID
            if (HasVibrator())
                vibrator.Call("cancel");
#endif
        }

        /// <summary>
        /// Return true if device supports amplitude control
        /// </summary>
        /// <returns>true if amplitude control available else false</returns>
        public static bool HasAmplitudeControl()
        {
            if (HasVibrator() && doesSupportVibrationEffect)
            {
                return vibrator.Call<bool>("hasAmplitudeControl"); // API 26+ specific
            }
            else
            {
                return false; // no amplitude control below API level 26
            }
        }

        /// <summary>
        /// Check if vibration is available on device
        /// </summary>
        /// <returns>true if available else false</returns>
        public static bool HasVibrator()
        {
            return vibrator != null && vibrator.Call<bool>("hasVibrator");
        }

        /// <summary>
        /// Wrapper for public static VibrationEffect createOneShot (long milliseconds, int amplitude). API >= 26
        /// </summary>
        private static AndroidJavaObject CreateEffectOneShot(long milliseconds, int amplitude)
        {
            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude);
        }

        private static AndroidJavaObject CreateEffectWaveform(long[] pattern, int repeat)
        {
            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
        }
    }
}