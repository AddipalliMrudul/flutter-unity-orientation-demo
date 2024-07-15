using System;
using System.Collections.Generic;
using System.IO;
using XcelerateGames.AssetLoading;
using XcelerateGames.Editor.Inspectors;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using XcelerateGames.Editor.Build;
using XcelerateGames.Cryptography;
using System.Net;

#pragma warning disable 618

namespace XcelerateGames.Editor
{
    public class S3UploadUpdatedAssets : EditorWindow
    {
        public enum AssetUploadState
        {
            Waiting,
            Verifying,
            Uploading,
            Failed,
            Successful,
            UptoDate,
            End
        }

        public enum UploadState
        {
            Waiting,
            GetCurrentVersionListHash,
            SetCurrentVersionListHash,
            DownloadingVersionList,
            Validating,
            Uploading,
            UploadingVersionList,
            Complete,
        }

        public class S3UploadItem
        {
            public WWW _WWW = null;
            public string _File = null;
            public string _Hash = null;
            public string _Size = null;
            public bool _Active = true;
            public AssetUploadState _State = AssetUploadState.Waiting;

            public void StartUploading(AssetUploadState state, string filePath)
            {
                _State = state;
                if (_State == AssetUploadState.UptoDate)
                    return;

                // Create a Web Form
                WWWForm form = new WWWForm();
                form.AddField("Content-Type", "application/x-www-form-urlencoded");

                Dictionary<string, string> dataToSend = new Dictionary<string, string>();
                dataToSend.Add("file_name", filePath + Path.GetFileName(_File));
                dataToSend.Add("hash", _Hash);
                dataToSend.Add("data", Convert.ToBase64String(File.ReadAllBytes(_File)));
                //dataToSend.Add("bucket", mWindow.mBucketPath.Replace("{platform}", mWindow.mPlatform.ToString()));
                string data = CryptoUtilities.EncryptOrDecrypt(dataToSend.ToJson());
                form.AddField("data", data);

                //Meta needed by our server for API validation
                string ticks = "123";
                form.AddField("meta", EditorUtilities.GetMeta().ToJson());

                Dictionary<string, string> headers = form.headers;
                headers.Add("secure", CryptoUtilities.GetMd5Hash(Utilities.GetUniqueID(), ticks, data));
                headers.Add("ticks", ticks);

                _WWW = new WWW(pUploadURL, form.data, headers);
            }
        }

        public class S3CheckStatus
        {
            [JsonIgnore] public bool _IsDone = false;
            public string folderPath = null;
            public List<string> _Files = null;
        }


        private Vector2 mScroll = Vector2.zero;
        private List<S3UploadItem> mObjects = new List<S3UploadItem>();
        //private List<string> mUploadList = new List<string>();
        private SortedDictionary<string, AssetData> mVersionList = null;
        private int mCompleted = 0;
        private int mFailed = 0;
        private string mBucketPath = null;
        private UploadState mUploadState = UploadState.Waiting;
        private static bool mDebug = false;
        Dictionary<string, bool> mFileStatus;
        private PlatformUtilities.Platform mPlatform = PlatformUtilities.Platform.NONE;

        private const float SlNoWidth = 30;
        private const float FileNameWidth = 300;
        private const float HashWidth = 256;
        private const float SizeWidth = 128;

        private static string pUploadURL
        {
            get
            {
                if (mDebug)
                    return BuildAppSettings.pInstance.LocalHostUrl + "uploadFile";
                else
                    return BuildAppSettings.S3UploadAPI;
            }
        }

        private static string pUploadURL1
        {
            get
            {
                if (mDebug)
                    return BuildAppSettings.pInstance.LocalHostUrl + "uploadFileV2";
                else
                    return BuildAppSettings.S3UploadAPIV2;
            }
        }

        private static string pDownloadURL
        {
            get
            {
                if (mDebug)
                    return BuildAppSettings.pInstance.LocalHostUrl + "readFileV2";
                else
                    return BuildAppSettings.S3ReadFile;
            }
        }

        private static string pCheckFileStatusURL
        {
            get
            {
                if (mDebug)
                    return BuildAppSettings.pInstance.LocalHostUrl + "checkFileStatusV1";
                else
                    return BuildAppSettings.S3CheckFileStatusV1;
            }
        }

        private int mBatchCount = 25;
        private int mIndex = 0;
        private S3CheckStatus mCheckStatus;

        private string mBuildNumber = null;
        private string mMessage = "";
        private string mHashTemp = "";
        //Last modified time (in mins)
        private int mModifiedTime = 10;

        private static S3UploadUpdatedAssets mWindow = null;

        private void OnEnable()
        {
            mVersionList = EditorUtilities.GetVersionList();
        }

        private void Awake()
        {
            mBuildNumber = ProductSettings.pInstance._BuildNumber;
        }

        private void OnGUI()
        {
            GUILayout.Label("Path : " + mBucketPath.Replace("{platform}", mPlatform.ToString()), EditorStyles.boldLabel, new GUILayoutOption[0] { });
            EditorGUI.BeginDisabledGroup(mCheckStatus != null);
            if (mUploadState == UploadState.Waiting)
            {
                mDebug = EditorGUITools.DrawToggle("Debug", mDebug);

                int lastTime = mModifiedTime;
                mModifiedTime = EditorGUITools.DrawIntDelayed("Last modified (in mins)", mModifiedTime);
                if (lastTime != mModifiedTime)
                    RefreshList(mModifiedTime);

                mBuildNumber = EditorGUITools.DrawTextField("Build Number", mBuildNumber);
                mHashTemp = EditorGUITools.DrawTextField("Hash", mHashTemp);
                mMessage = EditorGUITools.DrawTextField("Message", mMessage);
            }
            GUILayout.BeginVertical();

            DrawHeader();

            mScroll = GUILayout.BeginScrollView(mScroll);
            Color defaultColor = GUI.color;
            for (int i = 0; i < mObjects.Count; ++i)
            {
                S3UploadItem obj = mObjects[i];
                GUI.color = obj._Active ? defaultColor : Color.red;
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                GUILayout.Label((i + 1) + ".", GUILayout.Width(SlNoWidth));
                GUILayout.Label(obj._File, GUILayout.Width(FileNameWidth));
                GUILayout.Label(obj._Hash, GUILayout.Width(HashWidth));
                GUILayout.Label(obj._Size, GUILayout.Width(SizeWidth));
                GUILayout.Label(obj._State.ToString());
                if (mUploadState == UploadState.Waiting)
                {
                    if (GUILayout.Button((obj._Active ? "-" : "+"), GUILayout.Width(30)))
                    {
                        obj._Active = !obj._Active;
                    }
                }
                int progress = 0;
                if (obj._State == AssetUploadState.Uploading || obj._State == AssetUploadState.Successful || obj._State == AssetUploadState.Failed)
                {
                    progress = (int)(obj._WWW.uploadProgress * 100);
                    GUILayout.Label(progress + "%", GUILayout.Width(40));
                }
                else if (obj._State == AssetUploadState.UptoDate)
                {
                    GUILayout.Label("--", GUILayout.Width(40));
                }
                if (obj._State == AssetUploadState.Uploading && obj._WWW.isDone)
                {
                    if (!string.IsNullOrEmpty(obj._WWW.error))
                    {
                        obj._State = AssetUploadState.Failed;

                        Dictionary<string, string> response = new Dictionary<string, string>();
                        response.Add("code", "500");
                        response.Add("data", obj._WWW.error);
                        OnUploadComplete(response, obj._File);
                    }
                    else
                    {
                        obj._State = AssetUploadState.Successful;

                        Debug.Log(obj._WWW.text);
                        //Dictionary<string, string> response = obj._WWW.text.FromJson<Dictionary<string, string>>();
                        Dictionary<string, string> response = new Dictionary<string, string>() { { "code", "200" } };
                        OnUploadComplete(response, obj._File);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUI.color = defaultColor;

            GUILayout.EndScrollView();
            if (mUploadState == UploadState.Validating)
                GUILayout.Label("Checking file status... Please wait");

            GUILayout.BeginHorizontal();
            //This has to be float, otherwise progress calculation will not work
            GUILayout.Label("Total : " + mObjects.Count);
            GUILayout.Label("Completed : " + mCompleted);
            GUILayout.Label("Failed : " + mFailed);
            float overallProgress = 0;
            if (mObjects.Count > 0)
                overallProgress = ((mFailed + mCompleted) / mObjects.Count);
            if (mObjects.Count > 0)
                GUILayout.Label("Progress : " + (int)(overallProgress * 100) + "%");
            else
                GUILayout.Label("Progress : 0%");
            if (mUploadState == UploadState.Validating)
            {
            }

            if (mUploadState == UploadState.Waiting)
            {
                if (GUILayout.Button("Upload"))
                {
                    if (string.IsNullOrEmpty(mHashTemp))
                        EditorUtility.DisplayDialog("Error", "Hash is empty", "Ok");
                    else if (string.IsNullOrEmpty(mMessage))
                        EditorUtility.DisplayDialog("Error", "You must enter a message", "Ok");
                    else
                        CheckFileStatus();
                }
            }
            if (mUploadState == UploadState.Complete)
            {
                if (mFailed > 0 && GUILayout.Button("Remove Completed"))
                {
                    foreach (S3UploadItem item in mObjects)
                    {
                        mObjects.RemoveAll((obj) => obj._State == AssetUploadState.Successful || obj._State == AssetUploadState.UptoDate);
                    }
                }
                else
                {
                    if (GUILayout.Button("Close"))
                        Close();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("");

            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawHeader()
        {
            GUIColor.Push(Color.cyan);
            GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
            GUILayout.Label("Sl No", GUILayout.Width(SlNoWidth));
            GUILayout.Label("File Name", GUILayout.Width(FileNameWidth));
            GUILayout.Label("Hash", GUILayout.Width(HashWidth));
            GUILayout.Label("Size", GUILayout.Width(SizeWidth));
            GUILayout.Label("Status");
            GUILayout.EndHorizontal();
            GUIColor.Pop();
        }

        private void CheckFileStatus()
        {
            mCheckStatus = new S3CheckStatus();
            mCheckStatus.folderPath = mWindow.mBucketPath.Replace("{platform}", mWindow.mPlatform.ToString());
            mCheckStatus._Files = new List<string>();

            //Now remove all files that were Non-active
            mObjects.RemoveAll((obj) => !obj._Active);

            for (int i = 0; i < mObjects.Count; ++i)
            {
                string fileName = Path.GetFileName(mObjects[i]._File);
                if (mVersionList.ContainsKey(fileName))
                    mCheckStatus._Files.Add(fileName + "/" + mVersionList[fileName].hash);
                else
                    XDebug.LogError(fileName + "  not found in Version List");
            }

            WebRequestHandler webRequest = new WebRequestHandler(pCheckFileStatusURL, CheckFileStatusOnSuccess, CheckFileStatusOnFail, CheckFileStatusOnProgress);
            webRequest.Run(mCheckStatus.ToJson());
            mUploadState = UploadState.Validating;
        }

        private void CheckFileStatusOnProgress(float obj)
        {
            throw new NotImplementedException();
        }

        private void CheckFileStatusOnFail(string error)
        {
            Debug.LogError("File status verification failed : " + error);
        }

        private void CheckFileStatusOnSuccess(string obj, WebHeaderCollection responseHeaders)
        {
            WebRequestResponse requestResponse = obj.FromJson<WebRequestResponse>();
            mCheckStatus._IsDone = true;
            mFileStatus = CryptoUtilities.EncryptOrDecrypt(requestResponse.data).FromJson<Dictionary<string, bool>>();

            mCheckStatus = null;
            GetVersionListHash(mBuildNumber);
            //OnQuestionPopupClosed(true);
        }

        private void GetVersionListHash(string buildNumber)
        {
            mUploadState = UploadState.GetCurrentVersionListHash;
            //TODO:We need to have a API call to get the current version hash
            DownloadVersionList(mHashTemp);
        }

        private void DownloadVersionList(string hash)
        {
            Debug.Log("Downloading version list with hash : " + hash);
            mUploadState = UploadState.DownloadingVersionList;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("file_path", EditorUtilities.GetS3FolderPath() + ResourceManager.mAssetVersionListFileName + "/" + hash);
            WebRequestHandler webRequest = new WebRequestHandler(pDownloadURL, DownloadVersionListOnSuccess, DownloadVersionListOnFail, null);
            webRequest.Run(data.ToJson());
        }

        private void DownloadVersionListOnFail(string error)
        {
            Debug.LogError("Version list downloading failed : " + error);
            EditorUtility.DisplayDialog("Version List Failed", "Version list downloading failed: " + error, "Ok");
        }

        private void DownloadVersionListOnSuccess(string obj, WebHeaderCollection responseHeaders)
        {
            Debug.Log("Version list downloaded : " + obj);
            //Deugging purpose only
            //File.WriteAllText("version_list_downloaded.json", obj);

            WebRequestResponse responseData = obj.FromJson<WebRequestResponse>();

            SortedDictionary<string, AssetData> versionListFromServer = CryptoUtilities.EncryptOrDecrypt(responseData.data).FromJson<SortedDictionary<string, AssetData>>();
            InsertHashOfUpdatedAssets(versionListFromServer);
        }

        private void InsertHashOfUpdatedAssets(SortedDictionary<string, AssetData> versionListFromServer)
        {
            for (int i = 0; i < mObjects.Count; ++i)
            {
                AssetData assetData = AssetData.Create(mObjects[i]._File, null);
                versionListFromServer.AddOrUpdate<string, AssetData>(Path.GetFileName(mObjects[i]._File), assetData);
            }
            UploadNewVersionList(versionListFromServer);
        }

        private void UploadNewVersionList(SortedDictionary<string, AssetData> versionListFromServer)
        {
            //File.WriteAllText("version_list_updated.json", versionListFromServer.ToJson());
            File.WriteAllText(EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName, versionListFromServer.ToJson());

            mUploadState = UploadState.UploadingVersionList;
            string newHash = FileUtilities.GetMD5OfText(versionListFromServer.ToJson());
            Debug.Log("New version hash : " + newHash);

            Dictionary<string, string> dataToSend = new Dictionary<string, string>();
            string folderName = string.Empty;
            dataToSend.Add("file_name", EditorUtilities.GetS3FolderPath() + ResourceManager.mAssetVersionListFileName);
            dataToSend.Add("hash", newHash);
            dataToSend.Add("data", Convert.ToBase64String(File.ReadAllBytes(EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName)));
            //dataToSend.Add("bucket", mWindow.mBucketPath.Replace("{platform}", mWindow.mPlatform.ToString()));

            WebRequestHandler webRequest = new WebRequestHandler(pUploadURL1, UploadVersionListOnSuccess, UploadVersionListOnFail, null);
            webRequest.Run(dataToSend.ToJson());
        }

        private void UploadVersionListOnFail(string error)
        {
            Debug.LogError("Failed to upload the updated version list : " + error);
        }

        private void UploadVersionListOnSuccess(string obj, WebHeaderCollection responseHeaders)
        {
            Debug.Log("Successfully uploaded the updated version list : " + obj);
            SortedDictionary<string, string> uploadedAssets = new SortedDictionary<string, string>();
            foreach (S3UploadItem item in mObjects)
            {
                uploadedAssets.Add(Path.GetFileName(item._File), item._Hash);
            }
            UpdateVersionListHashHistory.Update(mMessage, mHashTemp, uploadedAssets);
            StartUploading();
        }

        private AssetUploadState GetState(string fileName)
        {
            foreach (string file in mFileStatus.Keys)
            {
                bool index = fileName.Replace(EditorUtilities.mAssetsDir, string.Empty).Equals(file.Split('/')[0]);
                if (index)
                {
                    if (mFileStatus[file])
                    {
                        return AssetUploadState.UptoDate;
                    }
                    else
                        return AssetUploadState.Uploading;
                }
            }
            return AssetUploadState.Uploading;
        }

        private void OnQuestionPopupClosed(bool validated)
        {
            if (validated)
            {
                StartUploading();
            }
            else
                EditorUtility.DisplayDialog("Validation Failed", "Validation failed. Please try again", "Ok");
        }

        private void Update()
        {
            mWindow.Repaint();
        }

        private S3UploadItem Upload(string filePath)
        {
            S3UploadItem item = new S3UploadItem();
            item._File = filePath;
            FileInfo fInfo = new FileInfo(filePath);
            item._Size = Utilities.FormatBytes(fInfo.Length);
            string fileName = Path.GetFileName(filePath);
            if (mVersionList.ContainsKey(fileName))
                item._Hash = mVersionList[fileName].hash;
            else
                XDebug.LogError(fileName + "  not found in Version List");
            return item;
        }

        private void OnUploadComplete(Dictionary<string, string> response, string fileName)
        {
            if (response["code"] == "200")
            {
                Debug.Log("Uploaded successfully : " + fileName);
                mCompleted++;
            }
            else
            {
                mFailed++;
                EditorApplication.Beep();
                Debug.LogError("Failed to upload : " + fileName + "\nError : " + response["data"]);
            }

            if ((mCompleted + mFailed) == mObjects.Count)
            {
                UpdateVersionListHash.DoUpdateVersionListHash();
                mUploadState = UploadState.Complete;
            }
            else
            {
                if (mIndex < mObjects.Count)
                {
                    AssetUploadState state = GetState(mObjects[mIndex]._File);
                    if (state == AssetUploadState.UptoDate)
                        mCompleted++;
                    mObjects[mIndex].StartUploading(state, mWindow.mBucketPath);
                    mIndex++;
                }
            }
        }

        [MenuItem(Utilities.MenuName + "Build/Upload Updated Assets", false, 3)]
        public static void CreateS3UploadUpdatedAssets()
        {
            mWindow = EditorWindow.GetWindow<S3UploadUpdatedAssets>(true, "S3 Uploader", true);
            mWindow.Init();
        }

        internal void Init()
        {
            mBucketPath += EditorUtilities.GetS3FolderPath();

            RefreshList(mModifiedTime);
            mPlatform = PlatformUtilities.GetCurrentPlatform();
            Focus();
            Show();
        }

        private void RefreshList(int time)
        {
            mWindow.mObjects.Clear();

            List<string> files = EditorUtilities.GetFilesModifiedInLastMinutes(time);
            if (files.Count == 0)
            {
                Debug.LogErrorFormat($"No files modified in last {time} minutes");
                return;
            }

            //Now remove Version_list.json , Data_iOS & Data_Android files
            files.Remove(EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName);
            files.Remove(EditorUtilities.mAssetsDir + PlatformUtilities.GetAssetFolderPath());

            //Sort files
            files.Sort(delegate (string file1, string file2)
            {
                return file1.CompareTo(file2);
            });

            foreach (string file in files)
            {
                if (File.Exists(file))
                    mWindow.mObjects.Add(mWindow.Upload(file));
                else
                    Debug.LogWarning("File not found : " + file);
            }
        }

        private void StartUploading()
        {
            mUploadState = UploadState.Uploading;
            mCompleted = mFailed = 0;
            for (int i = 0; i < mBatchCount && i < mObjects.Count; ++i)
            {
                AssetUploadState state = GetState(mObjects[i]._File);
                if (state == AssetUploadState.UptoDate)
                    mCompleted++;
                mObjects[i].StartUploading(state, mBucketPath);
            }
            mIndex = mBatchCount;
        }
    }
}