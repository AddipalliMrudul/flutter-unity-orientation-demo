using UnityEngine;
using UnityEditor;
using System.IO;
using XcelerateGames.Editor.Build;
using System.Collections.Generic;
using XcelerateGames.AssetLoading;
using XcelerateGames.Editor;

namespace XcelerateGames.Editor.AssetBundles
{
    public class BuildAssetBundle
    {
        private static AssetBundleManifest mAssetBundleManifest = null;

        public const string AssetBundleMenu = Utilities.MenuName + "AssetBundle/";
        const string kSimulateAssetBundlesMenu = AssetBundleMenu + "Simulate AssetBundles";
        const string kSimulateLoadingDelayMenu = AssetBundleMenu + "Simulate Loading Delay";
        const string ForceSDvariant = AssetBundleMenu + "Force SD Variant";

        [MenuItem(kSimulateAssetBundlesMenu, false, 17)]
        public static void ToggleSimulateAssetBundle()
        {
            ResourceManager.pSimulateAssetBundles = !ResourceManager.pSimulateAssetBundles;
        }

        [MenuItem(kSimulateAssetBundlesMenu, true, 17)]
        public static bool ToggleSimulateAssetBundleValidate()
        {
            Menu.SetChecked(kSimulateAssetBundlesMenu, ResourceManager.pSimulateAssetBundles);
            return true;
        }

        [MenuItem(kSimulateLoadingDelayMenu, false, 18)]
        public static void ToggleSimulateLoadingDelay()
        {
            AsyncLoader.pSimulateLoadingDelay = !AsyncLoader.pSimulateLoadingDelay;
        }

        [MenuItem(kSimulateLoadingDelayMenu, true, 18)]
        public static bool ToggleSimulateLoadingDelayValidate()
        {
            Menu.SetChecked(kSimulateLoadingDelayMenu, AsyncLoader.pSimulateLoadingDelay);
            return true;
        }
        /// <summary>
        /// Create asset bundles only if they have been modified.
        /// </summary>
        [MenuItem(AssetBundleMenu + "Build %#k", false, 2)]
        public static void CreateAssetBundleFromAssetDatabase()
        {
            CreateAssetBundle(BuildAssetBundleOptions.None);
        }

        /// <summary>
        /// Deletes all AssetBundles & manifest files.
        /// </summary>
        [MenuItem(AssetBundleMenu + "Delete All", false, 3)]
        public static void CleanAssetBundles()
        {
            Debug.Log("Deleting AssetBundles.");

            List<string> files = EditorUtilities.GetFiles(EditorUtilities.mAssetsDir, "*.manifest");

            foreach (string path in files)
            {
                string fileName = PlatformUtilities.GetAssetDirectoryForPlatform(PlatformUtilities.GetCurrentPlatform()) + Path.GetFileName(path).Replace(".manifest", string.Empty);

                AssetDatabase.DeleteAsset(fileName);
                AssetDatabase.DeleteAsset(fileName + ".manifest");
                Debug.Log("Deleting : " + fileName);
            }


            //mAssetBundleManifest = EditorUtilities.LoadManifest();
            //List<string> assetBundles = new List<string>(mAssetBundleManifest.GetAllAssetBundles());

            //int count = 0;
            //foreach (string path in assetBundles)
            //{
            //    string filePath = PlatformUtilities.GetAssetDirectoryForPlatform(PlatformUtilities.GetCurrentPlatform()) + path;
            //    AssetDatabase.DeleteAsset(filePath);
            //    AssetDatabase.DeleteAsset(filePath + ".meta");
            //    AssetDatabase.DeleteAsset(filePath + ".manifest");
            //    AssetDatabase.DeleteAsset(filePath + ".manifest.meta");
            //    count++;
            //}
            //Debug.Log($"Deleted {count} assetBundles.");

            AssetDatabase.ImportAsset(EditorUtilities.mAssetsDir, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Force create all asset bundles with user prompt
        /// </summary>
        [MenuItem(AssetBundleMenu + "Build(Force)", false, 2)]
        public static void ForceCreateAssetBundleFromAssetDatabase()
        {
            if (EditorUtility.DisplayDialog("Warning!", "Rebuilding all AssetBundles will take time. Are you sure you want to force build all?", "Yes", "No"))
                ForceCreateAssetBundles();
        }

        /// <summary>
        /// Force create all asset bundles
        /// </summary>
        public static void ForceCreateAssetBundles()
        {
            CreateAssetBundle(BuildAssetBundleOptions.ForceRebuildAssetBundle);
        }

        /// <summary>
        /// Creates the AssetBundle from AssetDatabase.
        /// </summary>
        /// <param name="inOptions"></param>
        private static void CreateAssetBundle(BuildAssetBundleOptions inOptions)
        {
            //See if the output directory exists, if not, create one
            string outputPath = "Assets/" + PlatformUtilities.GetAssetFolderPath();
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            BuildPipeline.BuildAssetBundles(outputPath, inOptions, EditorUserBuildSettings.activeBuildTarget);

            CreateAssetVersionList.DoCreateAssetVersionList();
        }

        [MenuItem(AssetBundleMenu + "Get AssetBundle Names", false, 5)]
        static void GetAssetBundleNames()
        {
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (var name in names)
                Debug.Log("AssetBundle: " + name);
        }

        [MenuItem(AssetBundleMenu + "Dump AssetBundle Hash", false, 4)]
        static void DumpAssetBundleHash()
        {
            var names = AssetDatabase.GetAllAssetBundleNames();
            DeleteUnusedBundles();
            mAssetBundleManifest = EditorUtilities.LoadManifest();
            StreamWriter writer = null;
            FileStream fStream = File.Create("BundleHashes.txt");
            writer = new StreamWriter(fStream);

            foreach (var name in names)
            {
                string hash = mAssetBundleManifest.GetAssetBundleHash(name).ToString();
                writer.WriteLine(string.Format("{0,-32} : {1,32}", name, hash));
            }
            writer.Close();
        }

        [MenuItem(AssetBundleMenu + "Delete Unused Bundles", false, 3)]
        public static void DeleteUnusedBundles()
        {
            Debug.Log("Deleting unused AssetBundles.");
            mAssetBundleManifest = EditorUtilities.LoadManifest();

            List<string> files = EditorUtilities.GetFiles(EditorUtilities.mAssetsDir, "*.manifest");

            foreach (string path in files)
            {
                string fileName = Path.GetFileName(path).Replace(".manifest", string.Empty);
                if (fileName == PlatformUtilities.GetAssetFolderPath())
                    continue;
                string hash = mAssetBundleManifest.GetAssetBundleHash(fileName).ToString();
                if (hash.Contains("000000000000000000000"))
                {
                    //Hash is invalid, this means the bundle does not exist anymore. Lets delete it.
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.DeleteAsset(path.Replace(".manifest", string.Empty));
                    Debug.Log("Deleting : " + fileName);
                }
            }
        }

        [MenuItem(ForceSDvariant, false, 16)]
        public static void ForceSDVariant()
        {
            if (EditorPrefs.HasKey("ForceSD"))
                EditorPrefs.DeleteKey("ForceSD");
            else
                EditorPrefs.SetBool("ForceSD", true);
        }

        [MenuItem(ForceSDvariant, true, 16)]
        public static bool ToggleForceSDVariant()
        {
            Menu.SetChecked(ForceSDvariant, EditorPrefs.HasKey("ForceSD"));
            return true;
        }
    }
}