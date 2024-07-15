using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;
using XcelerateGames.UI;
using XcelerateGames.AssetLoading;
using TMPro;
using XcelerateGames.Editor.Build;
#pragma warning disable 618

namespace XcelerateGames.Editor
{

    public class EditorUtilities : ScriptableWizard
    /// <summary>
    /// This class contains utility methods and constants used in the editor scripts.
    /// </summary>
    {
#if UNITY_IOS
	public const string mAssetsDir = "Assets/Data_iOS/";
#elif UNITY_ANDROID
        public const string mAssetsDir = "Assets/Data_Android/";
#elif UNITY_FACEBOOK
    public const string mAssetsDir = "Assets/Data_FBA/";
#elif UNITY_WEBGL
    public const string mAssetsDir = "Assets/Data_WebGL/";
#elif UNITY_STANDALONE_OSX
        public const string mAssetsDir = "Assets/Data_OSX/";
#elif UNITY_STANDALONE_WIN
        public const string mAssetsDir = "Assets/Data_Win/";
#else
        public const string mAssetsDir = "Assets/Data/";
#endif

#if UNITY_ANDROID
        public const BuildTargetGroup CurrentBuildTargetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
            public const BuildTargetGroup CurrentBuildTargetGroup = BuildTargetGroup.iOS;
#elif UNITY_STANDALONE
            public const BuildTargetGroup CurrentBuildTargetGroup = BuildTargetGroup.Standalone;
#elif UNITY_WEBGL
            public const BuildTargetGroup CurrentBuildTargetGroup = BuildTargetGroup.WebGL;
#else
        public const BuildTargetGroup BuildTargetGroup = BuildTargetGroup.Unknown;
#endif


        //https://docs.unity3d.com/ScriptReference/AudioImporter.GetOverrideSampleSettings.html
        public static string GetCurrentPlatform()
        {
#if UNITY_IOS
        return "iOS";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_WEBGL
        return "WebGL";
#else
            return "Standalone";
#endif
        }

        public static string RunInBackgroundKey = "RunInBackground";
        public const string NameSpace = "XcelerateGames";

        //Returns a list of files specified by pattern, You can pass CSV for multiple patterns
        public static List<string> GetFiles(string path, string pattern)
        {
            List<string> files = new List<string>();
            foreach (string searchPattern in pattern.Split(','))
                files.AddRange(System.IO.Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories));

            return files;
        }

        public static T[] GetAtPath<T>(string path)
        {
            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                if (fileName.EndsWith(".meta") || fileName.EndsWith(".unity") || fileName.EndsWith(".unity3d"))
                    continue;
                UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(fileName, typeof(T));

                if (t != null)
                    al.Add(t);
            }
            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }

        public static SortedDictionary<string, AssetData> GetVersionList()
        {
            SortedDictionary<string, AssetData> versionList = File.ReadAllText(mAssetsDir + ResourceManager.mAssetVersionListFileName).FromJson<SortedDictionary<string, AssetData>>();
            if (!versionList.ContainsKey(ResourceManager.mAssetVersionListFileName))
                versionList.Add(ResourceManager.mAssetVersionListFileName, AssetData.Create(mAssetsDir + ResourceManager.mAssetVersionListFileName, null));
            if (!versionList.ContainsKey(ResourceManager.SecondaryPrefetchList))
            {
                if (File.Exists(mAssetsDir + ResourceManager.SecondaryPrefetchList))
                    versionList.Add(ResourceManager.SecondaryPrefetchList, AssetData.Create(mAssetsDir + ResourceManager.SecondaryPrefetchList, null));
            }
            return versionList;
        }

        public static string GetAssetDirByPlatform(PlatformUtilities.Platform platform)
        {
            if (platform == PlatformUtilities.Platform.Android || platform == PlatformUtilities.Platform.Amazon)
                return "Assets/Data_Android/";
            else if (platform == PlatformUtilities.Platform.iOS)
                return "Assets/Data_iOS/";
            Debug.LogError("Unknown platform");
            return null;
        }

        [MenuItem(Utilities.MenuName + "Get World Corners")]
        static void DisplayWorldCorners()
        {
            RectTransform rectTransform = Selection.activeTransform.GetComponent<RectTransform>();
            Vector3[] v = new Vector3[4];
            rectTransform.GetWorldCorners(v);

            Debug.Log("World Position : " + rectTransform.GetWorldPosition());
            for (var i = 0; i < 4; i++)
            {
                Debug.Log("World Corner " + i + " : " + v[i]);
            }
        }

        [MenuItem(Utilities.MenuName + "Get Asset Path")]
        static void GetAssetPath()
        {
            List<string> paths = new List<string>();
            foreach (Object obj in Selection.objects)
                paths.Add(AssetDatabase.GetAssetPath(obj));
            GUIUtility.systemCopyBuffer = paths.Printable();
            Debug.Log(paths.Printable());
        }

        [MenuItem(Utilities.MenuName + "Get Asset Path", true)]
        static bool GetAssetPathValidate()
        {
            return (Selection.objects != null && Selection.objects.Length > 0);
        }

        [MenuItem(Utilities.MenuName + "Get Object Path")]
        static void GetObjectPath()
        {
            string data = null;
            foreach (Transform obj in Selection.transforms)
                data += obj.gameObject.GetObjectPath() + "\n";
            GUIUtility.systemCopyBuffer = Selection.transforms[0].gameObject.GetObjectPath();
            Debug.Log(data);
        }

        [MenuItem(Utilities.MenuName + "Get Object Path", true)]
        static bool GetObjectPathValidate()
        {
            return (Selection.transforms != null && Selection.transforms.Length > 0);
        }

        [MenuItem(Utilities.MenuName + "Debug/Dump Selected Object Info")]
        static void DumpSelectedObejctInfo()
        {
            StreamWriter writer = new StreamWriter(File.Create("Dependencies.txt"));

            foreach (Object obj in Selection.objects)
            {
                writer.WriteLine("{0,-15}{1,25}", EditorUtility.FormatBytes(GetAssetSize(obj)), AssetDatabase.GetAssetPath(obj));
            }

            writer.Close();
        }

        //Returns path to our project folder.
        public static string ProjectRoot()
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }

        public static AssetBundleManifest LoadManifest()
        {
            //Get path to our AssetBundleManifest asset bundle file.
            string assetBundleManifestPath = "file://" + Application.dataPath + "/" + PlatformUtilities.GetAssetFolderPath() + "/" + PlatformUtilities.GetAssetFolderPath();

            //Load the bundle now.
            AssetBundleManifest assetBundleManifest = null;
            WWW manifestObj = new WWW(assetBundleManifestPath);
            if (string.IsNullOrEmpty(manifestObj.error))
            {
                AssetBundle manifestBundle = manifestObj.assetBundle;
                if (manifestBundle != null)
                {
                    //Load AssetBundleManifest object.
                    assetBundleManifest = manifestBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                    //Unload the bundle, but keep the Manifest in memory.
                    manifestBundle.Unload(false);
                }
                else
                    Debug.LogError("Failed to load AssetBundleManifest");
            }
            else
                Debug.LogError("Error! Failed to load AssetBundleManifest : " + manifestObj.url + ", Error : " + manifestObj.error);
            return assetBundleManifest;
        }

        [MenuItem(Utilities.MenuName + "Level Designing/Toggle Active %#g")]
        static void ToggleGameObjectActive()
        {
            foreach (Transform t in Selection.transforms)
                t.gameObject.SetActive(!t.gameObject.activeSelf);
        }

        public static List<string> GetLabels(Object obj)
        {
            if (obj != null)
                return new List<string>(AssetDatabase.GetLabels(obj));
            return new List<string>();
        }

        public static List<string> GetLabels(string assetPath)
        {
            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            return GetLabels(obj);
        }

        public static bool HasLabel(string assetPath, string label)
        {
            return GetLabels(assetPath).Contains(label);
        }

        public static long GetAssetSize(string assetpath, System.Type type)
        {
            return Profiler.GetRuntimeMemorySizeLong(AssetDatabase.LoadAssetAtPath(assetpath, type));
        }

        public static long GetAssetSize(Object asset)
        {
            return GetAssetSize(AssetDatabase.GetAssetPath(asset), asset.GetType());
        }

        public static string FormatSize(long size)
        {
            return EditorUtility.FormatBytes(size);
        }

        public static string GetFormattedSize(Object obj)
        {
            return FormatSize(GetAssetSize(obj));
        }

        [MenuItem(Utilities.MenuName + "UI/UI State")]
        static void ShowUIState()
        {
            string data = "Exlusive UI : " + (UiBase.pIsAnyExclusiveUiActive ? UiBase.pExclusiveItem.name : "null");
            EditorUtility.DisplayDialog("Ui State", data, "Ok", "");
        }

        [MenuItem(Utilities.MenuName + "UI/Remove RayCast on TMP")]
        static void RemoveRayCastFromTMP()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                TextMeshProUGUI[] textItems = go.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI item in textItems)
                {
                    item.raycastTarget = false;
                    EditorUtility.SetDirty(item);
                }
            }
        }

        [MenuItem(Utilities.MenuName + "Open Save Dir")]
        static void OpenSavePath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        const string kSimulateAssetBundlesMenu = Utilities.MenuName + "Utils/Run In Background";
        [MenuItem(kSimulateAssetBundlesMenu, false, 51)]
        public static void ToggleRunInBackground()
        {
            Application.runInBackground = !Application.runInBackground;
            XGEditorPrefs.SetBool(RunInBackgroundKey, Application.runInBackground);
        }

        [MenuItem(kSimulateAssetBundlesMenu, true, 51)]
        public static bool ToggleRunInBackgroundValidate()
        {
            Application.runInBackground = XGEditorPrefs.GetBool(RunInBackgroundKey);
            Menu.SetChecked(kSimulateAssetBundlesMenu, Application.runInBackground);
            return true;
        }

        //Returns name of assetbundle that the asset is under
        public static bool IsInAssetBundle(string assetPath, out string bundleName)
        {
            bundleName = AssetDatabase.GetImplicitAssetBundleName(assetPath);
            return !string.IsNullOrEmpty(bundleName);
        }

        public static bool IsInAssetBundle(Object asset, out string bundleName)
        {
            return IsInAssetBundle(AssetDatabase.GetAssetPath(asset), out bundleName);
        }

        //Returns a lits of all files that were modified in last X mintes,
        public static List<string> GetFilesModifiedInLastMinutes(int minutes)
        {
            List<string> allFiles = new List<string>(Directory.GetFiles(EditorUtilities.mAssetsDir, "*.*"));
            allFiles.RemoveAll(e => e.EndsWith(".meta") || e.EndsWith(".manifest"));
            allFiles.RemoveAll(delegate (string file)
            {
                System.DateTime dt1 = File.GetLastWriteTime(file);
                double diff = (System.DateTime.Now - dt1).TotalMinutes;
                if (diff > minutes)
                    return true;//This file was modified earlier than given time, remove it
                return false;
            });
            return allFiles;
        }

        public static string GetS3FolderPath()
        {
            string folderName = string.Empty;
            if (!ProductSettings.pInstance.ContentFolderInBucket.IsNullOrEmpty())
                folderName = ProductSettings.pInstance.ContentFolderInBucket + "/";
            folderName += PlatformUtilities.GetCurrentPlatform() + "/";
            return folderName;
        }

        public static Dictionary<string, string> GetMeta()
        {
            //Meta needed by our server for API validation
            Dictionary<string, string> meta = new Dictionary<string, string>();
            string ticks = "123";
            meta.Add("uuid", Utilities.GetUniqueID());
            meta.Add("ticks", ticks);
            meta.Add("app_version_major", ProductSettings.GetProductVersion(0).ToString());
            meta.Add("app_version_minor", ProductSettings.GetProductVersion(1).ToString());
            meta.Add("build_number", ProductSettings.pInstance._BuildNumber);
            meta.Add("app", ProductSettings.pInstance._AppName);
            meta.Add("platform", PlatformUtilities.GetCurrentPlatform().ToString());
            meta.Add("env", PlatformUtilities.GetEnvironment().ToString());
            meta.Add("device_os", PlatformUtilities.GetOSVersion());
            meta.Add("app_version", ProductSettings.GetProductVersion());

            return meta;
        }

        public static string GetNdkPath()
        {
#if UNITY_EDITOR_WIN
            return $"\"C:/Program Files/Unity/Hub/Editor/{Application.unityVersion}/Editor/Data/PlaybackEngines/AndroidPlayer/NDK\"";
#elif UNITY_EDITOR_OSX
            return $"\"/Applications/Unity/Hub/Editor/{Application.unityVersion}/PlaybackEngines/AndroidPlayer/NDK\"";
#else
            return string.Empty;
#endif
        }

        public static void SetScriptingDefineSymbol(string compilerFlag)
        {
            string flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup);
            if (!flags.Contains(compilerFlag))
            {
                flags += ";" + compilerFlag;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup, flags);
            }
        }

        public static void RemoveScriptingDefineSymbol(string compilerFlag)
        {
            string flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup);
            flags = flags.Replace(compilerFlag, string.Empty);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup, flags);
        }

        /// <summary>
        /// Clears all server environment compiler flags
        /// </summary>
        public static void ClearEnvFlags()
        {
            string flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup);
            //Remove all server types.
            foreach (string serverType in PlatformUtilities.GetAllEnvironementType())
                flags = flags.Replace(serverType + "_BUILD", "");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUtilities.CurrentBuildTargetGroup, flags);
        }

        /// <summary>
        /// Helps to get all the assets of the types in the project
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> FindAssetsByType<T>(string[] searchInFolders) where T : Object
        {
            foreach (string folderPath in searchInFolders)
            {
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    Debug.LogError("Found invalid path " + folderPath);
                    yield break;
                }
            }

            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}", searchInFolders);
            foreach (var t in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(t);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }

        /// <summary>
        /// Compresses a file or directory using the zip command.
        /// </summary>
        /// <param name="fileName">The name of the file or directory to compress.</param>
        /// <param name="compressedFileName">The name of the compressed file.</param>
        /// <param name="currentWorkingDir">The current working directory (optional).</param>
        public static void Compress(string fileName, string compressedFileName, string currentWorkingDir = null)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            // Configure the process using the StartInfo properties.
            if (!currentWorkingDir.IsNullOrEmpty())
                process.StartInfo.WorkingDirectory = currentWorkingDir;
            process.StartInfo.FileName = "zip";
            process.StartInfo.Arguments = $"-r {compressedFileName} {fileName}/ -x \"*.DS_Store\"";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
            process.Start();
            process.WaitForExit();
        }
    }
}