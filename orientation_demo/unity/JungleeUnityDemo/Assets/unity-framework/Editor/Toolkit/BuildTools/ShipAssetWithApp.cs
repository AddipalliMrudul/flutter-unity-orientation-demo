using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using System;
using XcelerateGames.AssetLoading;
using XcelerateGames.Editor.AssetBundles;

/// <summary>
/// Author : Altaf
/// Date : Nov 16, 2015
/// Purpose : This script creates a list of assets that are shipped with the app.
/// </summary>

#if !UNITY_WEBPLAYER

namespace XcelerateGames.Editor.Build
{
    internal class ShipAssetWithApp : ScriptableWizard
    {
        private static string FileName { get { return ResourceManager.BundledAssetsAssetName + ".json"; } }

        private const string mBaseCommand = "Assets/" + Utilities.MenuName;
        private static AssetBundleManifest mAssetBundleManifest = null;
        private static List<string> mProcessedFiles = null;

        private static bool Validate()
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                string fName = AssetDatabase.GetAssetPath(obj);
                if (!fName.Contains(EditorUtilities.mAssetsDir))
                    return false;
            }
            return true;
        }

        private static bool CanAdd(string fName)
        {
            if (fName.Contains(".DS_Store") || fName.EndsWith("meta") || fName.EndsWith("manifest") || fName.Contains(ResourceManager.mAssetVersionListFileName) ||
                fName.Contains(ResourceManager.PrimaryPrefetchList) || fName.Contains(ResourceManager.SecondaryPrefetchList) /*|| fName.Contains(ResourceManager.mJsonAssetVersionListFileName)*/)
                return false;
            return true;
        }

        private static void GetAssetList()
        {
#if !UNITY_WEBGL
            BuildAssetBundle.DeleteUnusedBundles();

            mAssetBundleManifest = EditorUtilities.LoadManifest();
#endif
            mProcessedFiles = new List<string>();


            foreach (UnityEngine.Object obj in Selection.objects)
            {
                string fName = AssetDatabase.GetAssetPath(obj);
                if (!CanAdd(fName))
                    continue;

                if (FileUtilities.IsDirectory(fName))
                {
                    string[] filesInDir = Directory.GetFiles(fName, "*.*");
                    foreach (string f in filesInDir)
                    {
                        if (!CanAdd(fName))
                            continue;
                        AddToList(f);
                    }
                    continue;
                }
                AddToList(fName);
            }
        }

        private static void GetFileList()
        {
#if !UNITY_STANDALONE
            BuildAssetBundle.DeleteUnusedBundles();
#endif
            //#if !UNITY_WEBGL
            mAssetBundleManifest = EditorUtilities.LoadManifest();
            //#endif
            mProcessedFiles = new List<string>();

            string[] files = System.IO.Directory.GetFiles(EditorUtilities.mAssetsDir);
            foreach (string fName in files)
            {
                if (!CanAdd(fName))
                    continue;
                AddToList(fName);
            }
        }

        private static void AddToList(string fName)
        {
            string fileName = fName.Replace("\\", "/");
            fileName = fileName.Replace(EditorUtilities.mAssetsDir, "");
            if (!mProcessedFiles.Contains(fileName))
                mProcessedFiles.Add(fileName);
        }

        #region Ship Assets with app

        public static void AddAssetsToShipWithApp()
        {
            //Clear the existing list
            ClearAssetsToShipWithApp();
            //Get the list of all assets uner the respective data folder.
            GetFileList();

            //Remove all assets that are marked as secondary prefetch.
            List<string> secondaryPrefetch = GetSecondaryPrefetchList();
            mProcessedFiles.RemoveAll(e => secondaryPrefetch.Contains(e));

            //We want to add the xml file in the app as well.
            mProcessedFiles.Add(FileName);
            mProcessedFiles.Remove(PlatformUtilities.GetCurrentPlatform().ToString());

            Dictionary<string, string> shippedAssets = new Dictionary<string, string>();
            if (shippedAssets != null)
            {
                foreach (string path in mProcessedFiles)
                {
                    if (!shippedAssets.ContainsKey(path))
                        shippedAssets.Add(path, string.Empty);
                    string hash = null;
                    if (ResourceManager.IsTextAsset(path))
                        hash = FileUtilities.GetMD5OfFile(EditorUtilities.mAssetsDir + path);
                    else
                        hash = mAssetBundleManifest.GetAssetBundleHash(path).ToString();
                    shippedAssets[path] = hash;
                }
            }
            SaveShipWithApp(shippedAssets);
        }

        private static void ClearAssetsToShipWithApp()
        {
            Dictionary<string, string> shippedAssets = ResourceManager.GetDefaultVersionList();
            if (shippedAssets != null)
            {
                shippedAssets.Clear();
                SaveShipWithApp(shippedAssets);
            }
        }

        private static void SaveShipWithApp(Dictionary<string, string> shippedAssets)
        {
            string folderPath = "Assets/Resources/";
            string filePath = folderPath + FileName;
            FileUtilities.CreateDirectoryRecursively(folderPath);
            System.IO.File.WriteAllText(filePath, shippedAssets.ToJson());
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);

            Debug.Log("Total " + shippedAssets.Count + " assets will be shipped with app.");
        }

        #endregion Ship Assets with app

        #region Secondary Prefetch list

        [MenuItem(mBaseCommand + "Add/Secondary Prefetch List", true)]
        private static bool AddAssetsToSecondaryPrefetchValidate()
        {
            return Validate();
        }

        [MenuItem(mBaseCommand + "Add/Secondary Prefetch List")]
        private static void AddAssetsToSecondaryPrefetch()
        {
            GetAssetList();

            List<string> prefetchList = GetSecondaryPrefetchList();

            foreach (string path in mProcessedFiles)
            {
                if (path == PlatformUtilities.GetAssetFolderPath())
                    continue;
                string asset = prefetchList.Find(e => e == path);
                if (asset == null)
                    prefetchList.Add(path);
            }

            SaveSecondaryPrefetch(prefetchList);
        }

        public static List<string> GetSecondaryPrefetchList()
        {
            List<string> prefetchList = null;
            string fileName = EditorUtilities.mAssetsDir + ResourceManager.SecondaryPrefetchList;
            if (File.Exists(fileName))
                prefetchList = File.ReadAllText(fileName).FromJson<List<string>>();
            if (prefetchList == null)
            {
                prefetchList = new List<string>();
            }
            return prefetchList;
        }

        [MenuItem(mBaseCommand + "Remove/Secondary Prefetch List", true)]
        private static bool RemoveAssetsToSecondaryPrefetchValidate()
        {
            return Validate();
        }

        [MenuItem(mBaseCommand + "Remove/Secondary Prefetch List")]
        private static void RemoveAssetsToSecondaryPrefetch()
        {
            GetAssetList();
            //We want to add the xml file in the app as well.
            mProcessedFiles.Add(FileName);

            List<string> prefetchList = GetSecondaryPrefetchList();

            foreach (string path in mProcessedFiles)
                prefetchList.Remove(path);

            SaveSecondaryPrefetch(prefetchList);
        }

        [MenuItem(mBaseCommand + "Clear/Secondary Prefetch List")]
        private static void ClearAssetsToSecondaryPreftch()
        {
            List<string> prefetchList = GetSecondaryPrefetchList();
            prefetchList.Clear();
            SaveSecondaryPrefetch(prefetchList);
        }

        public static void SaveSecondaryPrefetch(List<string> prefetchList)
        {
            string fileName = EditorUtilities.mAssetsDir + ResourceManager.SecondaryPrefetchList;
            FileUtilities.WriteToFile(fileName, prefetchList.ToJson());
        }
        #endregion Secondary Prefetch list
    }
}
#endif //UNITY_WEBPLAYER
