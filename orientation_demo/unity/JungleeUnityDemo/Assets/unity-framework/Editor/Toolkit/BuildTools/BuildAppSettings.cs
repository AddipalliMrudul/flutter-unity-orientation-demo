using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace XcelerateGames.Editor.Build
{
    public enum AndroidBuildType
    {
        APK = 0,
        AAB,
        AndroidStudioProject,
        End
    }

    /// <summary>
    /// To maintain build information.
    /// </summary>
	[CreateAssetMenu(fileName = "BuildAppSettings", menuName = Utilities.MenuName + "BuildAppSettings")]
    public class BuildAppSettings : ScriptableObject
    {
        public string KeyStorePassword;
        public string KeyStoreAliasPassword;
        public string KeyStoreAliasName;
        public string KeyStoreName;
        //This is the name of the jar file
        public string GradleWrapperFileName = null;

        public SceneAsset[] Scenes;
        public string[] ResourcesFolders;
        public string[] AssetBundleToIgnore;
        public Object _BuildStepsDoc = null;

        [SerializeField] private EnvironmentPath _UpdateVersionHashAPI = null;
        public static string UpdateVersionHashAPI => pInstance._UpdateVersionHashAPI.Path;

        [SerializeField] private EnvironmentPath _DownloadLocalizationAPI = null;
        public static string DownloadLocalizationAPI => pInstance._DownloadLocalizationAPI.Path;

        [SerializeField] private EnvironmentPath _S3UploadAPI = null;
        public static string S3UploadAPI => pInstance._S3UploadAPI.Path;

        [SerializeField] private EnvironmentPath _S3UploadAPIV2 = null;
        public static string S3UploadAPIV2 => pInstance._S3UploadAPIV2.Path;

        [SerializeField] private EnvironmentPath _S3CheckFileStatusV1 = null;
        public static string S3CheckFileStatusV1 => pInstance._S3CheckFileStatusV1.Path;

        [SerializeField] private EnvironmentPath _S3ReadFile = null;
        public static string S3ReadFile => pInstance._S3ReadFile.Path;

        [SerializeField] private EnvironmentPath _BucketName = null;
        public static string BucketName => pInstance._BucketName.Path;

        [Header("Used while debugging")]
        public string LocalHostUrl = "http://127.0.0.1:5000/";

        private static BuildAppSettings mInstance;

        public static BuildAppSettings pInstance
        {
            get
            {
                if (mInstance == null)
                    mInstance = EditorGUIUtility.Load("BuildAppSettings.asset") as BuildAppSettings;

                return mInstance;
            }
        }

        public static bool Validate(out string error)
        {
            if (pInstance.Scenes == null || pInstance.Scenes.Length == 0)
            {
                error = "No scene added to Scenes";
                return false;
            }
            foreach (SceneAsset scene in pInstance.Scenes)
            {
                if (scene == null)
                {
                    error = "scene is null in Scenes";
                    return false;
                }
            }
            error = null;
            return true;
        }

        //Returns the path of scenes that need to be built for the build
        public static string[] BuildScenes()
        {
            List<string> scenes = new List<string>();
            foreach (SceneAsset scene in pInstance.Scenes)
            {
                scenes.Add(AssetDatabase.GetAssetPath(scene));
            }
            return scenes.ToArray();
        }


        [MenuItem(Utilities.MenuName + "Build/Build Steps")]
        static void OpenBuildProcessDoc()
        {
            if (pInstance._BuildStepsDoc != null)
            {
                string filePath = AssetDatabase.GetAssetPath(pInstance._BuildStepsDoc);
                Application.OpenURL($"file:///{System.IO.Directory.GetCurrentDirectory()}/{filePath}");
            }
            else
            {
                EditorUtility.DisplayDialog("No Document", $"Build steps doc not linked. Create & link it under BuildAppSettings/{nameof(pInstance._BuildStepsDoc)}", "Ok");
            }
        }
    }
}
