using UnityEngine;
using System;
using XcelerateGames.AssetLoading;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace XcelerateGames.Locale
{
    [Serializable]
    public class LocaleData
    {
        public string _Name = null;
        public Language _Language = Language.None;
        public string _AssetName = null;
        public int _Column = -1;
        public bool _Enabled = true;
        public bool _LoadFromResourcesFolder = false;
    }

    [CreateAssetMenu(fileName = "Localization", menuName = Utilities.MenuName + "Localization")]
    public class Localization : ScriptableObject
    {
        public Language _DefaultLanguage = Language.English;
        //This is the name of document in google drive
        public string _SheetName = null;
        //This is the name of worksheet, by default the worksheet name in Google Drive is Sheet1
        public string _WorkSheetName = "Sheet1";
        public LocaleData[] _Locales = null;

        private static Localization mInstance = null;
        private Language mLanguage = Language.None;
        private static Action<bool> OnReady = null;
        private Dictionary<string, string> mDictionary = null;

        public static Dictionary<string, string> Dictionary { get { return mInstance.mDictionary; } }

        public static Language LanguageCode
        {
            get
            {
                if (mInstance != null)
                    return mInstance.mLanguage;
                return Language.English;
            }
            set
            {
                if (mInstance.mLanguage != value && value != Language.None)
                {
                    mInstance.mLanguage = value;
                    mInstance.LoadLocalization(mInstance.mLanguage);
                }
            }
        }

        public static LocaleData[] Locales => Array.FindAll(mInstance._Locales, e => e._Enabled);

        public static void DeInit()
        {
            mInstance = null;
        }

        public static bool Init(Action<bool> callback)
        {
            FrameworkEventManager.LogEvent("unity_localization_init_started");

            if (XDebug.CanLog(XDebug.Mask.Resources))
                Debug.Log("Initializing localization");
            OnReady = callback;

            bool result = false;
            if (mInstance == null)
            {
                mInstance = ResourceManager.LoadFromResources<Localization>("Localization");
                if (mInstance == null)
                    XDebug.LogException("Could not find Localization asset under Resources folder");
                else
                {
                    result = true;
                    mInstance.mLanguage = (Language)PlayerPrefs.GetInt("SelectedLanguage", (int)Language.None);
#if USE_REVERIE
                    CreateConverter();
#endif
                    if (LanguageCode == Language.None)
                    {
                        LanguageCode = mInstance._DefaultLanguage;
                    }
                    mInstance.LoadLocalization(mInstance.mLanguage);
                }
            }
            else
                OnReady?.Invoke(true);
            return result;
        }

        public static string Get(string text)
        {
            try
            {
                if (mInstance != null && mInstance.mDictionary != null)
                {
                    if (mInstance.mDictionary.TryGetValue(text, out string locale))
                        return Convert(locale);
                }
                return text;
            }
            catch (Exception e)
            {
                XDebug.LogException($"Exception while getting value for key: {text}, Exception: {e.Message}");
                return text;
            }
        }

        public static string Get(string text, string defaultKey)
        {
            try
            {
                if (mInstance != null && mInstance.mDictionary != null)
                {
                    if (mInstance.mDictionary.TryGetValue(text, out string locale))
                        return Convert(locale);
                }
                return defaultKey;
            }
            catch (Exception e)
            {
                XDebug.LogException($"Exception while getting value for key: {text}, Exception: {e.Message}");
                return defaultKey;
            }
        }

        public static string Format(string key, params object[] args)
        {
            try
            {
                if (mInstance != null && mInstance.mDictionary != null)
                {
                    if (mInstance.mDictionary.TryGetValue(key, out string locale))
                        return string.Format(Convert(locale), args);
                }
                return key;
            }
            catch (Exception e)
            {
                XDebug.LogException($"Exception while getting value for key: {key}, Exception: {e.Message}");
                return key;
            }
        }

        public static void Set(string text)
        {
            try
            {
                mInstance.mDictionary = text.FromJson<Dictionary<string, string>>();
            }
            catch (Exception ex)
            {
                XDebug.LogException($"Failed to parse Localization text. Exception : {ex.Message}, {text}");
            }
        }

        private void LoadLocalization(Language language)
        {
            LocaleData localeData = Array.Find(_Locales, locale => locale._Language == language);
            if (localeData._LoadFromResourcesFolder)
            {
#if UNITY_EDITOR
                if (ResourceManager.pSimulateAssetBundles)
                    localeData._AssetName = Path.GetFileNameWithoutExtension(localeData._AssetName);
#endif
                TextAsset textAsset = ResourceManager.LoadFromResources<TextAsset>(localeData._AssetName);
                if (textAsset != null)
                    OnLocalizationLoaded(textAsset == null ? null : textAsset.text);
            }
            else
                ResourceManager.Load(localeData._AssetName, OnLocalizationLoaded, ResourceManager.ResourceType.Text);
        }

        private void OnLocalizationLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserdata)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;
            bool loaded = false;
            if (inEvent == ResourceEvent.COMPLETE)
            {
                XDebug.Log($"Loaded Localization file {inURL}", XDebug.Mask.Resources);
                ResourceManager.Unload(inURL);

                OnLocalizationLoaded((inObject as string));
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                XDebug.LogError($"Failed to load Localization file: {inURL}");
            }
            OnReady?.Invoke(loaded);
            OnReady = null;
        }

        private void OnLocalizationLoaded(string inData)
        {
            bool loaded = false;
            if (!inData.IsNullOrEmpty())
            {
                try
                {
                    mDictionary = inData.FromJson<Dictionary<string, string>>();
                    loaded = true;
                    PlayerPrefs.SetInt("SelectedLanguage", (int)mLanguage);

#if USE_REVERIE
                    UpdateLanguageID();
#endif
                    Broadcast("OnLocalize");
                }
                catch (Exception ex)
                {
                    XDebug.LogError("May be failed to parse Localization file, retrying : " + ", " + ex.Message);
                }
            }
            OnReady?.Invoke(loaded);
            OnReady = null;
        }

        private void Broadcast(string funcName)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                Canvas[] cavasList = Resources.FindObjectsOfTypeAll<Canvas>();
                for (int i = 0; i < cavasList.Length; ++i)
                {
                    if (cavasList[i] != null)
                        cavasList[i].BroadcastMessage(funcName, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        #region Reverie
#if USE_REVERIE
        private static Unity8bitConverter.IConverter mConverter = null;
        private static int mLanguageId = -1;

        private static void CreateConverter()
        {
            mConverter = new Unity8bitConverter.UnicodeConverter();
            mConverter.Initialize();
            UpdateLanguageID();
        }

        private static void UpdateLanguageID()
        {
            mLanguageId = GetLanguageID(LanguageCode);
        }

        private static string Convert(string text)
        {
            if (LanguageCode != Language.English)
            {
                if (mConverter == null)
                    CreateConverter();
                return mConverter.Convert(text, mLanguageId);
            }
            return text;
        }

        private static int GetLanguageID(Language selectedLanguage)
        {
            switch (selectedLanguage)
            {
                case Language.Kannada:
                    return Unity8bitConverter.common.RevLangConstants.Lang_Kannada;
                case Language.Hindi:
                    return Unity8bitConverter.common.RevLangConstants.Lang_Hindi;
                case Language.Telugu:
                    return Unity8bitConverter.common.RevLangConstants.Lang_Telugu;
            }
            return Unity8bitConverter.common.RevLangConstants.Lang_English;
        }

        //Use this function only in language selection UI where we need to show all languages with their respective fonts
        public static string Get(string text, Language selectedLanguage)
        {
            switch (selectedLanguage)
            {
                case Language.Kannada:
                    return mConverter.Convert(text, Unity8bitConverter.common.RevLangConstants.Lang_Kannada);
                case Language.Hindi:
                    return mConverter.Convert(text, Unity8bitConverter.common.RevLangConstants.Lang_Hindi);
                case Language.Telugu:
                    return mConverter.Convert(text, Unity8bitConverter.common.RevLangConstants.Lang_Telugu);
            }
            return text;
        }
#else
        private static string Convert(string text)
        {
            return text;
        }

#endif //USE_REVERIE

        #endregion Reverie

        #region Editor Only code
#if UNITY_EDITOR
        private static Dictionary<string, List<string>> mAllLanguages = null;
        public static Dictionary<string, List<string>> dictionary
        {
            get
            {
                if (mAllLanguages == null)
                {
                    mAllLanguages = new Dictionary<string, List<string>>();
                    List<string> languages = new List<string>();
                    Localization localization = ResourceManager.LoadFromResources<Localization>("Localization");
                    foreach (LocaleData localeData in localization._Locales)
                    {
                        string textAsset = null;
                        if (localeData._LoadFromResourcesFolder)
                            textAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(localeData._AssetName)).text;
                        else
                            textAsset = File.ReadAllText(PlatformUtilities.GetAssetDirectoryForPlatform(PlatformUtilities.GetCurrentPlatform()) + localeData._AssetName);
                        Dictionary<string, string> dict = textAsset.FromJson<Dictionary<string, string>>();
                        languages.Add(localeData._Language.ToString());
                        foreach (KeyValuePair<string, string> valuePair in dict)
                        {
                            if (!mAllLanguages.ContainsKey(valuePair.Key))
                                mAllLanguages.Add(valuePair.Key, new List<string>());
                            mAllLanguages[valuePair.Key].Add(valuePair.Value);
                        }
                    }

                    knownLanguages = languages.ToArray();
                }
                return mAllLanguages;
            }
        }

        [MenuItem(Utilities.MenuName + "Localization/Refresh")]
        public static void RefreshLocalization()
        {
            mAllLanguages = null;
        }

        public static string[] knownLanguages = null;
#endif //UNITY_EDITOR
        #endregion Editor Only code
    }
}
