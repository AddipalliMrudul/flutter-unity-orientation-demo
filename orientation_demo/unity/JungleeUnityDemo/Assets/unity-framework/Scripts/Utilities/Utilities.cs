using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.UI;

namespace XcelerateGames
{
    public static class Utilities
    {
        public const string MenuName = "XcelerateGames/";

        private static string mUniqueCode = null;

        public static string HashText(string text)
        {
            string salt = "MySecretSalt";
            SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider();
            byte[] textWithSaltBytes = System.Text.Encoding.UTF8.GetBytes(string.Concat(text, salt));
            byte[] hashedBytes = hasher.ComputeHash(textWithSaltBytes);
            hasher.Clear();
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Returns the MD5 HASH for the input string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetMd5Hash(string input)
        {
#if NETFX_CORE
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
#else
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
#endif
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));

            return sb.ToString();
        }


        public static string MD5Hash(string text)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(text);
            return MD5Hash(inputBytes);
        }

        public static string MD5Hash(byte[] byteArray)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(byteArray);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }

        public static UiBase GetUiBase(Transform transform)
        {
            UiBase uiBase = null;
            while (transform != null)
            {
                uiBase = transform.GetComponent<UiBase>();
                if (uiBase != null)
                    break;
                else
                    transform = transform.parent;
            }

            return uiBase;
        }

        public static string FormatBytes(long bytes)
        {
            double sign = bytes >= 0 ? 1D : -1D;
            bytes = bytes >= 0 ? bytes : -bytes;
            string val = null;

            if (bytes < 1000)
                val = (bytes * sign).ToString() + " bytes";
            else if (bytes < 1000000)
                val = ((bytes / 1000D) * sign).ToString("0.0").StripDecimalPoints() + " KB";
            else if (bytes < 1000000000)
                val = ((bytes / 1000000D) * sign).ToString("0.0").StripDecimalPoints() + " MB";
            else
                val = ((bytes / 1000000000D) * sign).ToString("0.0").StripDecimalPoints() + " GB";
            return val;
        }

        public static string FormatNumber(long number)
        {
            number = number >= 0 ? number : -number;

            if (number < 1000000)
                return number.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"));
            else
                return ((double)number / 1000000.0d).ToString() + "M";
        }

        public static Transform FindChildTransform(GameObject obj, string tname)
        {
            return FindChildTransform(obj, tname, false);
        }

        /// <summary>
        /// Converts a given number to the equivalent string representation in indian currency with ₹ symbol.
        /// </summary>
        /// <param name="val">value to be formatted</param>
        /// <returns>formatted value</returns>
        /// @example
        /// 1 -> ₹ 1.00
        /// 10 -> ₹ 10.00
        /// 15 -> ₹ 15.00
        /// 168 -> ₹ 168.00
        /// 1030 -> ₹ 1,030.00
        /// 12001 -> ₹ 12,001.00
        /// 135600 -> ₹ 1,35,600.00
        public static string FormatToIndianCurrency(float val, bool dontShowDecimalPointifZero = true)
        {
            return FormatToIndianCurrency((decimal)val, dontShowDecimalPointifZero);
        }

        //TODO: We all should use double data type in place of float for currency across all the games
        /// <summary>
        /// Converts a given number to the equivalent string representation in indian currency with ₹ symbol.
        /// </summary>
        /// <param name="val">Double value to be formatted</param>
        /// <returns>formatted value</returns>
        /// @example
        /// 1 -> ₹ 1.00
        /// 10 -> ₹ 10.00
        /// 15 -> ₹ 15.00
        /// 168 -> ₹ 168.00
        /// 1030 -> ₹ 1,030.00
        /// 12001 -> ₹ 12,001.00
        /// 135600 -> ₹ 1,35,600.00
        public static string FormatToIndianCurrency(decimal val, bool dontShowDecimalPointifZero = true)
        {
            CultureInfo hindi = new CultureInfo("hi-IN");
            string formattedValue = string.Format(hindi, "{0:c}", val);
            if (dontShowDecimalPointifZero)
            {
                string[] data = formattedValue.Split('.');
                bool nonZeroValueFound = false;
                if (data.Length == 2)
                {
                    for (int i = 0; i < data[1].Length; ++i)
                    {
                        if (data[1][i] != '0')
                        {
                            nonZeroValueFound = true;
                            break;
                        }
                    }
                }
                formattedValue = (nonZeroValueFound ? formattedValue : data[0]);
            }
            return formattedValue.Replace(" ", "");
        }

        public static Transform FindChildTransform(GameObject obj, string tname, bool inactive)
        {
            if (obj.name == tname)
                return obj.transform;

            Transform Res;
            foreach (Transform t in obj.transform)
            {
                if (inactive || t.gameObject.activeSelf)
                {
                    Res = FindChildTransform(t.gameObject, tname, inactive);
                    if (Res != null)
                        return Res;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if the users device language is in our supported languages list. if yes, return that language, else default to English.
        /// </summary>
        /// <returns></returns>
        public static string GetLanguage()
        {
            //string langId = MobileUtilities.GetDeviceLocale();
            string language = "en";

            //if (ProductConfig.pInstance.Locale != null)
            //{
            //    foreach (LSG.Locale localeInfo in ProductConfig.pInstance.Locale)
            //    {
            //        if (localeInfo.ID == langId)
            //        {
            //            language = localeInfo.Language;
            //            break;
            //        }
            //        else
            //        {
            //            if (localeInfo.Variant == null)
            //                continue;

            //            foreach (string varient in localeInfo.Variant)
            //            {
            //                if (varient == langId)
            //                {
            //                    language = localeInfo.Language;
            //                    break;
            //                }
            //            }
            //            if (langId.StartsWith(localeInfo.ID))
            //            {
            //                language = localeInfo.Language;
            //                break;
            //            }
            //        }
            //    }
            //}
            //if (string.IsNullOrEmpty(language))
            //    language = Localization.language;

            Debug.Log("Sys Lang : " + Application.systemLanguage + ",  Locale :  " + language);
            return language;
        }

        /// <summary>
        /// Saves the date & time of the first app launch
        /// </summary>
        public static void SetFirstLaunchDate()
        {
            UnityEngine.PlayerPrefs.SetString("FIRST_LAUNCH_DATE", DateTime.UtcNow.ToString());
        }

        /// <summary>
        /// Gets the date & time of the first app launch, It return false if this is the first launch
        /// </summary>
        public static bool GetFirstLaunchDate(ref DateTime inDateTime)
        {
            if (UnityEngine.PlayerPrefs.HasKey("FIRST_LAUNCH_DATE"))
            {
                inDateTime = DateTime.Parse(UnityEngine.PlayerPrefs.GetString("FIRST_LAUNCH_DATE"));
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns Unique ID, this guid is used as guest account key.
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueID()
        {
            bool useDeviceID = true;

#if LIVE_BUILD || DEMO_BUILD || BETA_BUILD
            useDeviceID = true;
#endif
            if (!useDeviceID)
            {
                string uid = "";
                if (UnityEngine.PlayerPrefs.HasKey(GameConfig.Guestkey))
                {
                    uid = UnityEngine.PlayerPrefs.GetString(GameConfig.Guestkey);
                    if (!string.IsNullOrEmpty(uid))
                    {
                        XDebug.Log("Returning Unique ID : " + uid);
                        return uid;
                    }
                }

                Guid UID = Guid.NewGuid();
                uid = UID.ToString();
                UnityEngine.PlayerPrefs.SetString(GameConfig.Guestkey, uid);
                XDebug.Log("Returning New Unique ID : " + uid);
                return uid;
            }
            else
            {
#if UNITY_IOS && !UNITY_EDITOR
                return MobileUtilities.GetDeviceUUID();
#else
                if (!UnityEngine.PlayerPrefs.HasKey(GameConfig.Guestkey))
                    UnityEngine.PlayerPrefs.SetString(GameConfig.Guestkey, UnityEngine.SystemInfo.deviceUniqueIdentifier);
                return UnityEngine.SystemInfo.deviceUniqueIdentifier;
#endif
            }
        }

        /// <summary>
        /// Destroys all components of given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void DestroyComponents<T>() where T : Component
        {
            T[] csss = Resources.FindObjectsOfTypeAll<T>();
            if (csss != null)
            {
                foreach (T cs in csss)
                {
                    XDebug.Log("Destroying : " + cs.name);
                    UnityEngine.Object.DestroyImmediate(cs, true);
                }
            }
        }

        /// <summary>
        /// http://www.codeproject.com/Articles/14403/Generating-Unique-Keys-in-Net
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueKey(bool useCached = true)
        {
            if (!string.IsNullOrEmpty(mUniqueCode) && useCached)
                return mUniqueCode;
            int maxSize = 7;
            //int minSize = 5;
            char[] chars = new char[62];
            string a = "abcdefghijklmnopqrstuvwxyz1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
                result.Append(chars[b % (chars.Length)]);

            if (useCached)
            {
                mUniqueCode = result.ToString();
                return mUniqueCode;
            }
            return result.ToString();
        }

        public static string GetFirstName(string inName)
        {
            if (string.IsNullOrEmpty(inName))
                return string.Empty;
            return inName.Split(' ')[0];
        }

        public static void SendEmail(string subject)
        {
            string email = RemoteSettings.GetString("SupportMailID", "altaf.navalur@gmail.com");
            string body = "\n\n<-------Please type above this line------->";
            body += "\n\n-------------------------------------";
            body += "\nDevice : " + SystemInfo.deviceModel;
            body += "\nOS : " + MobileUtilities.GetOSVersion();
            body += "\nVersion : " + ProductSettings.GetProductVersion();
            body += "\nBuild : " + ProductSettings.pInstance._BuildNumber;

            //body += "\nCoins : " + UserData.pInstance.pCoins;//
            body += "\nGuest UUID : " + GetUniqueID();
            body = GetEscapeURL(body);

            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        public static string GetEscapeURL(string url)
        {
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }

        public static string TryReadValue(Dictionary<string, object> data, string key)
        {
            string ret = "";
            object dictData = new object();
            if (data.TryGetValue(key, out dictData))
                ret = (dictData != null) ? dictData.ToString() : "";
            return ret;
        }

        public static IEnumerator WaitAndDelete(float waitTime, GameObject inObject)
        {
            yield return new WaitForSeconds(waitTime);
            if (inObject != null)
                GameObject.Destroy(inObject);
        }

        public static IEnumerator WaitAndActivate(float waitTime, GameObject inObject, bool active)
        {
            yield return new WaitForSeconds(waitTime);
            inObject.SetActive(active);
        }

        public static IEnumerator WaitAndActivateAfterFrameEnd(GameObject inObject, bool active)
        {
            yield return new WaitForEndOfFrame();
            inObject.SetActive(active);
        }

        public static IEnumerator WaitAndActivateAfterFrameEnd(GameObject inObject, bool active, Action callback)
        {
            yield return new WaitForEndOfFrame();
            inObject.SetActive(active);
            callback?.Invoke();
        }

        public static bool HasChanged(string key, string saveData)
        {
            bool canSave = true;

            string newHash = MD5Hash(saveData);
            if (UnityEngine.PlayerPrefs.HasKey(key))
            {
                string prevHash = UnityEngine.PlayerPrefs.GetString(key);
                XDebug.Log("CanSave : " + prevHash + ", " + newHash + ", Updated?? " + canSave);
                canSave = (prevHash != newHash);
                if (canSave)
                    UnityEngine.PlayerPrefs.SetString(key, newHash);
            }
            else
                UnityEngine.PlayerPrefs.SetString(key, newHash);

            return canSave;
        }

        public static string Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private const string mKey = "eTewr4545!_=(45()&8-+-=ehuyur5u";
        public static string EncryptOrDecrypt(string text)
        {
            var result = new StringBuilder();

            for (int c = 0; c < text.Length; c++)
                result.Append((char)((uint)text[c] ^ (uint)mKey[c % mKey.Length]));

            return result.ToString();
        }

        public static string EncryptAndEncode(string text)
        {
            return Encode(EncryptOrDecrypt(text));
        }

        public static string DecryptAndDecode(string text)
        {
            text = text.Replace("\"", "");
            return EncryptOrDecrypt(Decode(text));
        }

        /// <summary>
        /// Returns an array of intergers from a comma separated string.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="separatorCahr"></param>
        /// <returns></returns>
        public static List<int> ConvertStringToIntArray(string data, char separatorCahr = ',')
        {
            List<int> items = new List<int>();
            string[] ints = data.Split(separatorCahr);
            for (int i = 0; i < ints.Length; ++i)
                items.Add(int.Parse(ints[i]));

            return items;
        }

        /// <summary>
        /// Returns an array of floats from a comma separated string.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="separatorCahr"></param>
        /// <returns></returns>
        public static List<float> ConvertStringToFloatArray(string data, char separatorCahr = ',')
        {
            List<float> items = new List<float>();
            string[] ints = data.Split(separatorCahr);
            for (int i = 0; i < ints.Length; ++i)
                items.Add(float.Parse(ints[i]));

            return items;
        }

        public static int GetRandomIndex(List<int> inArray)
        {
            int total = 0;
            int i = 0;
            for (i = 0; i < inArray.Count; ++i)
                total += inArray[i];
            int randNum = UnityEngine.Random.Range(0, total);
            total = 0;
            i = 0;
            while (total < randNum)
                total += inArray[i++];

            return Mathf.Max(0, i - 1);
        }

        public static T Instantiate<T>(object inObject, string inName, Transform inParent = null) where T : MonoBehaviour
        {
            try
            {
                GameObject obj = AddChild(inParent, inObject as GameObject);
                obj.name = inName;
                return obj.GetComponent<T>();
            }
            catch (Exception e)
            {
                XDebug.LogException($"{e}, Name: {inName}");
                return null;
            }
        }

        public static GameObject AddChild(Transform parent, GameObject prefab)
        {
            GameObject go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent) as GameObject;
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
            if (go != null && parent != null)
            {
                Transform t = go.transform;
                t.localScale = Vector3.one;
            }
            return go;
        }

        public static int GetQualityIndexByName(string qualityName)
        {
            return Array.FindIndex(QualitySettings.names, e => e.Equals(qualityName, StringComparison.OrdinalIgnoreCase));
        }

        public static void SendMessageUpwards(GameObject go, string methodName, int depth, SendMessageOptions options = SendMessageOptions.DontRequireReceiver)
        {
            int depthCount = 0;
            Transform trans = go.transform.parent;
            while (depthCount < depth)
            {
                trans.gameObject.SendMessage(methodName, options);
                trans = trans.parent;
                depthCount++;
            }
        }

        public static bool Equals(string arg1, string arg2)
        {
            return arg1.Equals(arg2, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetNumInUSFormat(int arg1)
        {
            if (arg1 > 999999999)
            {
                int reminder = arg1 % 1000000000;

                if (reminder == 0)
                    return string.Format("{0:0} B", (float)arg1 / 1000000000);
                else
                    return string.Format("{0:0.0} B", (float)arg1 / 1000000000);
            }
            else if (arg1 > 9999999)
            {
                int reminder = arg1 % 10000000;

                if (reminder == 0)
                    return string.Format("{0:0}Cr", (float)arg1 / 10000000);
                else
                    return string.Format("{0:0.0}Cr", (float)arg1 / 10000000);
            }
            else if (arg1 > 999999)
            {
                int reminder = arg1 % 1000000;

                if (reminder == 0)
                    return string.Format("{0:0}M", (float)arg1 / 1000000);
                else
                    return string.Format("{0:0.0}M", (float)arg1 / 1000000);
            }
            else if (arg1 > 99999)
            {
                int reminder = arg1 % 100000;

                if (reminder == 0)
                    return string.Format("{0:0}L", (float)arg1 / 100000);
                else
                    return string.Format("{0:0.0}L", (float)arg1 / 100000);
            }
            else if (arg1 > 999)
            {
                int reminder = arg1 % 1000;
                if (reminder == 0)
                    return string.Format("{0:0}K", (float)arg1 / 1000);
                else
                    return string.Format("{0:0.0}K", (float)arg1 / 1000);
            }

            return string.Format("{0:n0}", arg1);
        }

        //Returns unclamped value
        public static Vector3 Lerp(Vector3 from, Vector3 to, float time)
        {
            return from + ((to - from) * time);
        }

        //guest users will have alphanumeric values, while fb user will only have numeric
        public static bool IsGuestUser(string id)
        {
            double n; return !double.TryParse(id, out n);
        }
        //Returns name of the variable, ex : int count = 0;
        // When you pass count, it will return count
        public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }

        public static T Cast<T>(object o)
        {
            return (T)o;
        }

        public static T FindObjectOfType<T>() where T : MonoBehaviour
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();
            if (objects != null && objects.Length > 0)
            {
                return objects[0];
            }
            return null;
        }

        public static AudioType GetAudioType(string fileName)
        {
            string extn = Path.GetExtension(fileName).ToUpper();
            switch (extn)
            {
                case ".WAV":
                    return AudioType.WAV;
                case ".MP3":
                    return AudioType.MPEG;
                case ".OGG":
                    return AudioType.OGGVORBIS;
                default:
                    return AudioType.UNKNOWN;
            }
        }

        public static string GetMimeType(string fileName)
        {
            string extn = System.IO.Path.GetExtension(fileName);
            if (".png,.jpg,.jpeg".Contains(extn))
                return MimeType.IMAGES;
            if (".pdf".Contains(extn))
                return MimeType.PDF;
            if (".mp4".Contains(extn))
                return MimeType.VIDEO;
            if (".json".Contains(extn))
                return MimeType.JSON;
            if (".ogg".Contains(extn))
                return MimeType.AUDIO;
            XDebug.LogException($"Failed to get MimeType for {fileName}");
            return null;
        }

        public static MimeTypeId GetMimeTypeId(string fileName)
        {
            string mimeType = GetMimeType(fileName);
            switch (mimeType)
            {
                case MimeType.IMAGES:
                    return MimeTypeId.Images;
                case MimeType.PDF:
                    return MimeTypeId.Pdf;
                case MimeType.VIDEO:
                    return MimeTypeId.Video;
                case MimeType.AUDIO:
                    return MimeTypeId.Audio;
            }
            return MimeTypeId.None;
        }

        public static void AddCanvas(GameObject gameObj, bool clickable, int sortingOrder)
        {
            Canvas canvas = gameObj.GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObj.AddComponent<Canvas>();
            if (canvas)
            {
                if (clickable)
                {
                    if (canvas.gameObject.GetComponent<GraphicRaycaster>() == null)
                        canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;
            }
        }

        public static void RemoveCanvas(GameObject gameObj)
        {
            GraphicRaycaster raycaster = gameObj.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                UnityEngine.GameObject.DestroyImmediate(raycaster, true);

            Canvas canvas = gameObj.GetComponent<Canvas>();
            if (canvas != null)
                UnityEngine.GameObject.DestroyImmediate(canvas, true);
        }

        /// <summary>
        /// Used for truncating the floating point values at per given pricision & return the string
        /// </summary>
        /// <param name="value">value which needs to truncate</param>
        /// <param name="precision">truncate the value on this precision, by default it's value is 2</param>
        /// <returns></returns>
        public static string Truncate(float value, int precision = 2)
        {
            string result = value.ToString();
            if (precision < 0)
                return result;
            int dot = result.IndexOf('.');
            if (dot < 0)
                return result;
            int newLength = dot;
            if (precision > 0)
                newLength += precision + 1;
            if (newLength > result.Length)
                newLength = result.Length;
            return result.Substring(0, newLength);
        }

        /// <summary>
        /// Returns sleep timeout for the given enum value
        /// </summary>
        /// <param name="sleepTimeout"></param>
        /// <returns></returns>
        public static int GetSleepTimeoutValue(SleepTimeout sleepTimeout)
        {
            if (sleepTimeout == SleepTimeout.NeverSleep)
                return UnityEngine.SleepTimeout.NeverSleep;
            return UnityEngine.SleepTimeout.SystemSetting;
        }

        /// <summary>
        /// Checks if the receiver of the given Action is alive.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="action">Action to check for</param>
        /// <returns>true is receiver alive else false</returns>
        public static bool IsCallerAlive<T>(Action<T> action)
        {
            Delegate[] delegates = action.GetInvocationList();
            if (delegates != null && delegates.Length > 0)
            {
                if (delegates[0].Target != null && delegates[0].Target.ToString() != "null")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return the default value of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Returns trimmed version of app version. If we pass 1.2.3, it returns 1.2
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GetAppVersionTrimmed(string version)
        {
            if (version.IsNullOrEmpty())
                return "0.0";
            return version.Substring(0, version.LastIndexOf('.'));
        }

        public const string UpperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXUZ";
        public const string LowerCaseLetters = "abcdefghijklmnopqrstuvwxuz";
        public const string Numbers = "0123456789";

        /// <summary>
        /// Returns a random string 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomTextUpperCase(int length)
        {
            return GetRandomText(UpperCaseLetters, length);
        }

        public static string GetRandomTextLowererCase(int length)
        {
            return GetRandomText(LowerCaseLetters, length);
        }

        public static string GetRandomTextMixedCase(int length)
        {
            return GetRandomText(UpperCaseLetters + LowerCaseLetters, length);
        }

        public static string GetRandomTextAlphaNumeric(int length)
        {
            return GetRandomText(Numbers + UpperCaseLetters + LowerCaseLetters, length);
        }

        public static string GetRandomText(string text, int length)
        {
            string randomText = null;

            for (int i = 0; i < length; ++i)
            {
                randomText += text[UnityEngine.Random.Range(0, text.Length)];
            }
            return randomText;
        }
    }
}