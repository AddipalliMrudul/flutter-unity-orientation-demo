using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.Cryptography;
using XcelerateGames.Editor.Build;
#pragma warning disable 618

namespace XcelerateGames.Editor
{
    public class S3UploaderWindow : EditorWindow
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
            Validating,
            Uploading,
            Complete,
        }

        public class S3UploadItem
        {
            public WWW _WWW = null;
            public string _File = null;
            public string _Hash = null;
            public string _Size = null;
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

                dataToSend.Add("bucket", BuildAppSettings.BucketName);
                dataToSend.Add("file_name", filePath + Path.GetFileName(_File));
                dataToSend.Add("hash", _Hash);
                dataToSend.Add("data", Convert.ToBase64String(File.ReadAllBytes(_File)));
                if (!string.IsNullOrEmpty(mBackupFolderName))
                    dataToSend.Add("backup", mBackupFolderName);
                string data = CryptoUtilities.EncryptOrDecrypt(dataToSend.ToJson());
                form.AddField("data", data);

                //Meta needed by our server for API validation
                Dictionary<string, string> meta = new Dictionary<string, string>();
                string ticks = "123";
                form.AddField("meta", EditorUtilities.GetMeta().ToJson());

                Dictionary<string, string> headers = form.headers;
                headers.Add("secure", CryptoUtilities.GetMd5Hash(Utilities.GetUniqueID(), ticks, data));
                headers.Add("ticks", ticks);

                _WWW = new WWW(pUploadURL, form.data, headers);

                //WebRequestHandler webRequest = new WebRequestHandler(pUploadURL, OnUploadSuccess, OnUploadFail, null);
                //webRequest.Run(dataToSend.ToJson());
            }

            private void OnUploadFail(string obj)
            {
                throw new NotImplementedException();
            }

            private void OnUploadSuccess(string obj)
            {
                Debug.LogError(obj);
            }
        }

        public class S3CheckStatus
        {
            [JsonIgnore] public bool _IsDone = false;
            public string folderPath = null;
            public string bucket = null;
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
        private bool mRequireVersionCheck = false;
        private bool mAutoClose = false;
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
                    return BuildAppSettings.pInstance.LocalHostUrl + "uploadAssetV2";
                else
                    return BuildAppSettings.S3UploadAPIV2;
            }
        }

        private static string pCheckFileStatusURL
        {
            get
            {
                if (mDebug)
                    return BuildAppSettings.pInstance.LocalHostUrl + "checkFileStatus";
                else
                    return BuildAppSettings.S3CheckFileStatusV1;
            }
        }

        private int mBatchCount = 2500;
        private int mIndex = 0;
        private S3CheckStatus mCheckStatus;
        private static string mBackupFolderName = null;

        private string mVersion = "";

        private static S3UploaderWindow mWindow = null;

        private void OnEnable()
        {
            mVersionList = EditorUtilities.GetVersionList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Path : " + mBucketPath.Replace("{platform}", mPlatform.ToString()), EditorStyles.boldLabel, new GUILayoutOption[0] { });
            EditorGUI.BeginDisabledGroup(mCheckStatus != null);
            if (mUploadState == UploadState.Waiting)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Debug : ", GUILayout.Width(50));
                mDebug = GUILayout.Toggle(mDebug, string.Empty);
                GUILayout.EndHorizontal();

                if (mRequireVersionCheck)
                {
                    mPlatform = (PlatformUtilities.Platform)EditorGUILayout.EnumPopup("Pick Platform : ", mPlatform);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Version : ", GUILayout.Width(50));
                    mVersion = GUILayout.TextField(mVersion, GUILayout.Width(75));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.BeginVertical();

            DrawHeader();

            mScroll = GUILayout.BeginScrollView(mScroll);

            for (int i = 0; i < mObjects.Count; ++i)
            {
                S3UploadItem obj = mObjects[i];
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                GUILayout.Label((i + 1) + ".", GUILayout.Width(SlNoWidth));
                GUILayout.Label(obj._File, GUILayout.Width(FileNameWidth));
                GUILayout.Label(obj._Hash, GUILayout.Width(HashWidth));
                GUILayout.Label(obj._Size, GUILayout.Width(SizeWidth));
                GUILayout.Label(obj._State.ToString());
                if (mUploadState == UploadState.Waiting)
                {
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        Debug.Log("Removed : " + obj);
                        mObjects.Remove(obj);
                    }
                }
                int progress = 0;
                if (obj._State == AssetUploadState.Uploading || obj._State == AssetUploadState.Successful || obj._State == AssetUploadState.Failed)
                {
                    progress = 0;// (int)(obj._WWW.uploadProgress * 100);
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

                        //Dictionary<string, string> response = obj._WWW.text.FromJson<Dictionary<string, string>>();
                        Dictionary<string, string> response = new Dictionary<string, string>() { { "code", "200" } };
                        OnUploadComplete(response, obj._File);
                    }
                }
                GUILayout.EndHorizontal();
            }
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
            mCheckStatus.bucket = BuildAppSettings.BucketName;
            mCheckStatus.folderPath = mWindow.mBucketPath.Replace("{platform}", mWindow.mPlatform.ToString());
            mCheckStatus._Files = new List<string>();
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
            mCheckStatus._IsDone = true;
            WebRequestResponse responseData = obj.FromJson<WebRequestResponse>();
            if (responseData != null)
                mFileStatus = CryptoUtilities.EncryptOrDecrypt(responseData.data).FromJson<Dictionary<string, bool>>();
            else
                mFileStatus = new();

            mCheckStatus = null;
            OnQuestionPopupClosed(true);
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
                if (mRequireVersionCheck && mVersion != ProductSettings.GetProductVersion())
                    EditorUtility.DisplayDialog("Validation Failed", "Version numbers do not match", "Ok");
                else
                {
                    StartUploading();
                }
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
                Debug.LogError("IMPLEMENT UPDATING VERSIONLIST HASH");
                //UpdateVersionListHash.DoUpdateVersionListHash();
                mUploadState = UploadState.Complete;
                if (mAutoClose)
                    Close();
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

        public static void AddToUploadList(List<string> files, bool requireVersionCheck, bool takeBackup = true, bool silent = true)
        {
            if (takeBackup)
                mBackupFolderName = DateTime.Now.ToString("MMM-d-yyyy-HH-mm-tt");
            else
                mBackupFolderName = null;

            mWindow = EditorWindow.GetWindow<S3UploaderWindow>(true, "S3 Uploader", true);
            mWindow.mBucketPath += EditorUtilities.GetS3FolderPath();

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

            mWindow.mPlatform = PlatformUtilities.GetCurrentPlatform();
            mWindow.mRequireVersionCheck = requireVersionCheck;
            mWindow.Focus();

            mWindow.Show();
            mWindow.mAutoClose = silent;
            if (silent)
                mWindow.CheckFileStatus();
        }

        private void StartUploading()
        {
            mUploadState = UploadState.Uploading;
            mCompleted = mFailed = 0;
            for (int i = 0; i < mBatchCount && i < mObjects.Count; ++i)
            {
                AssetUploadState state = GetState(mObjects[i]._File);
                if (state == AssetUploadState.UptoDate)
                {
                    mObjects[i]._State = AssetUploadState.UptoDate;
                    mCompleted++;
                }
                else
                    mObjects[i].StartUploading(state, mWindow.mBucketPath);
            }
            mIndex = mBatchCount;
            if (mCompleted == mObjects.Count)
            {
                Debug.Log("All assets up to date");
                mUploadState = UploadState.Complete;
            }
            if (mAutoClose && mUploadState == UploadState.Complete)
                Close();
        }

        //// Resolve CDN path by URL that client passes
        //private static string ResolveBucketPath(string inBucket)
        //{
        //    inBucket =inBucket.Replace(ProductSettings.GetCDNBasePath(), ProductSettings.GetS3BucketName()) + "/wish/" + PlatformUtilities.GetCurrentPlatform() + "/";
        //    return inBucket;
        //}
    }
}