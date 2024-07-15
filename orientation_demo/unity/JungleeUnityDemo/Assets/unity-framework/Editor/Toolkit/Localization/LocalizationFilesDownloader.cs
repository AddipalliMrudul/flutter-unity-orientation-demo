using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Cryptography;
using XcelerateGames.Editor.Build;
using XcelerateGames.Editor.Inspectors;
using XcelerateGames.Locale;

#pragma warning disable 618

namespace XcelerateGames.Editor
{
    public class LocalizationFilesDownloader : EditorWindow
    {
        public class DownloadItem
        {
            public WWW _WWW = null;
            public string _File = null;
            public int _Column = 0;
            public bool _IsDone = false;
            public bool _IsUploading = false;
            public bool _Succeeded = false;
            public string _SheetName = null;
            public string _WorkSheetName = "Sheet1";

            public void StartUploading()
            {
                _IsUploading = true;
                _IsDone = false;

                // Create a Web Form
                WWWForm form = new WWWForm();

                Dictionary<string, string> mData = new Dictionary<string, string>();
                mData.Add("sheet_name", _SheetName);
                mData.Add("worksheet_name", _WorkSheetName);
                mData.Add("column", _Column.ToString());
                form.AddField("Content-Type", "application/x-www-form-urlencoded");
                string encryptedData = CryptoUtilities.EncryptOrDecrypt(mData.ToJson());
                form.AddField("data", encryptedData);

                //Meta needed by our server for API validation
                string ticks = "123";
                form.AddField("meta", EditorUtilities.GetMeta().ToJson());

                Dictionary<string, string> headers = form.headers;
                headers.Add("ticks", ticks);
                headers.Add("secure", CryptoUtilities.GetMd5Hash(Utilities.GetUniqueID(), ticks, encryptedData));

                _WWW = new WWW(pURL, form.data, headers);
            }
        }

        private static string pURL
        {
            get
            {
                if (mDebug)
                    return "http://127.0.0.1:5000/gs/getLocalizationDoc";
                else
                    return BuildAppSettings.DownloadLocalizationAPI;
            }
        }
        private List<DownloadItem> mDownloadItems = new List<DownloadItem>();

        private bool mDownloading = false;
        private int mCompleted = 0;
        private int mFailed = 0;
        private List<string> _FileNames = null;
        private int[] _Columns = { 2, 3, 4, 5 };
        private Localization mLocalization = null;

        public static System.Action OnDownloadComplete = null;
        private static bool mDebug = false;

        [MenuItem(Utilities.MenuName + "Localization/Fetch Doc")]
        public static void OpenConfigureWindow()
        {
            GetWindow<LocalizationFilesDownloader>(true, "Download Config", true);
        }

        private void OnEnable()
        {
            mDownloadItems.Clear();
            _FileNames = new List<string>();
            mLocalization = Resources.Load<Localization>("Localization");

            foreach (LocaleData localeData in mLocalization._Locales)
            {
                if (!localeData._Enabled)
                    continue;
                DownloadItem downloadItem = new DownloadItem()
                {
                    _File = localeData._AssetName,
                    _Column = localeData._Column,
                    _SheetName = mLocalization._SheetName,
                    _WorkSheetName = mLocalization._WorkSheetName
                };

                mDownloadItems.Add(downloadItem);
            }
        }

        private void Update()
        {
            Repaint();
        }

        private void OnDestroy()
        {
            Localization.RefreshLocalization();
        }

        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(mDownloading);
            int i = 1;
            mDebug = EditorGUITools.DrawToggle("Debug", mDebug);

            foreach (DownloadItem item in mDownloadItems)
            {
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                GUILayout.Label(i++ + ".", GUILayout.Width(30f));
                GUILayout.Button(pURL + "/" + item._File, "TextField", GUILayout.Height(20f));
                if (item._WWW != null)
                {
                    GUILayout.Label(item._WWW.progress * 100 + "%", GUILayout.Width(30));
                    if (item._WWW.isDone && item._IsUploading)
                    {
                        item._IsDone = true;
                        item._IsUploading = false;
                        if (string.IsNullOrEmpty(item._WWW.error))
                        {
                            JObject responseData = JObject.Parse(item._WWW.text);

                            string decrypted = CryptoUtilities.EncryptOrDecrypt(responseData["data"].ToString());
                            FileUtilities.WriteToFile(EditorUtilities.GetAssetDirByPlatform(PlatformUtilities.Platform.iOS) + item._File, decrypted);
                            FileUtilities.WriteToFile(EditorUtilities.GetAssetDirByPlatform(PlatformUtilities.Platform.Android) + item._File, decrypted);
                            mCompleted++;
                            Debug.Log("Downloaded : " + item._WWW.url);
                        }
                        else
                        {
                            mFailed++;
                            Debug.LogError($"Failed to download : {item._WWW.url}, Error: {item._WWW.error}");
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
            if ((mFailed + mCompleted) == mDownloadItems.Count)
            {
                mDownloading = false;
                mFailed = mCompleted = 0;
                if (OnDownloadComplete != null)
                {
                    Close();
                    OnDownloadComplete();
                    OnDownloadComplete = null;
                }
            }
            if (GUILayout.Button("Download"))
            {
                mDownloading = true;
                BeginDownload();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void BeginDownload()
        {
            mCompleted = mFailed = 0;
            mDownloadItems.ForEach((DownloadItem obj) => obj.StartUploading());
        }
    }
}