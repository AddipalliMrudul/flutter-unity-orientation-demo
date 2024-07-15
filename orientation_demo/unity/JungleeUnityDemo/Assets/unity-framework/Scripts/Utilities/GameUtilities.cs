using System;
using UnityEngine;

namespace XcelerateGames
{
    public static class GameUtilities
    {
        //private static int? keyboardHeight;
        public static int GetRelativeKeyboardHeight(RectTransform rectTransform, bool includeInput)
        {
            int keyboardHeight = GetKeyboardHeight(includeInput, 0);
            float screenToRectRatio = Screen.height / rectTransform.rect.height;
            float keyboardHeightRelativeToRect = keyboardHeight / screenToRectRatio;

            return (int)keyboardHeightRelativeToRect;
        }

        public static int GetKeyboardHeight(bool includeInput, int defaultH)
        {
#if UNITY_EDITOR
            return defaultH;
#elif UNITY_ANDROID
            using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
                AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
                AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
                if (view == null || dialog == null)
                    return 0;
                var decorHeight = 0;
                if (includeInput)
                {
                    AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                    if (decorView != null)
                        decorHeight = decorView.Call<int>("getHeight");
                }
                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    view.Call("getWindowVisibleDisplayFrame", rect);
                    return Screen.height - rect.Call<int>("height") + decorHeight;
                }
            }
#elif UNITY_IOS
            return (int)TouchScreenKeyboard.area.height;
#else
            return defaultH;
#endif
        }
        public static string FormatToIndianCurrency(this decimal val, string separator = " ", bool canAddRuppeeSymbol = false)
        {
            string postFixValue = string.Empty;
            (val, postFixValue) = GetNumInUSFormat(val, separator);
            return $"{Math.Round(val, 2)}{postFixValue}";
        }

        private static (decimal, string) GetNumInUSFormat(decimal arg1, string separator)
        {
            //if (arg1 > 999999999)
            //    return (arg1 / 1000000000, $"{separator}B");
            if (arg1 > 9999999)
                return (arg1 / 10000000, $"{separator}Cr");
            //else if (arg1 > 999999)
            //    return (arg1 / 1000000, $"{separator}M");
            else if (arg1 > 99999)
                return (arg1 / 100000, $"{separator}L");
            //else if (arg1 > 999)
            //    return (arg1 / 1000, $"{separator}K");
            return (arg1, "");
        }
    }
}
