using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.AssetLoading;

/// <summary>
/// Author : Altaf
/// Date : Dec 05, 2015
/// Purpose : This script creates a list of assets along with version.
/// </summary>

namespace XcelerateGames.Editor.Build
{
#if !UNITY_WEBPLAYER
    class CreateAssetVersionList : ScriptableWizard
    {
        private static List<string> mProcessedFiles = null;
        private static AssetBundleManifest mAssetBundleManifest = null;
        [MenuItem(Utilities.MenuName + "Get Asset hash")]
        public static void GetAssetHash()
        {
            foreach (Object asset in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string hash = null;
                //if (path.EndsWith(ResourceManager.mCompressedFileExtension))
                //    hash = FileUtilities.GetMD5OfText(StringCompression.Decompress(File.ReadAllBytes(path)));
                //else
                hash = FileUtilities.GetMD5OfFile(path);
                Debug.LogError(path + " : " + hash);
                GUIUtility.systemCopyBuffer = hash;
            }
        }

        [MenuItem(Utilities.MenuName + "Get VersionList hash")]
        public static string GetVersionListHash()
        {
            string filePath = EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName;
            string hash = FileUtilities.GetMD5OfFile(filePath);
            Debug.Log(filePath + " : " + hash);
            hash = $"\"b-{ProductSettings.pInstance._BuildNumber}\":\"{hash}\"";
            GUIUtility.systemCopyBuffer = hash;
            return hash;
        }

        [MenuItem(Utilities.MenuName + "Build/Create Asset Version List")]
        public static void DoCreateAssetVersionList()
        {
            mAssetBundleManifest = EditorUtilities.LoadManifest();

            mProcessedFiles = new List<string>();

            string[] filesInDir = Directory.GetFiles(EditorUtilities.mAssetsDir, "*.*", SearchOption.AllDirectories);
            foreach (string fName in filesInDir)
            {
                if (fName.Contains("PrefetchList") || fName.Contains(ResourceManager.mAssetVersionListFileName) || fName.EndsWith("manifest") || fName.EndsWith("meta") || fName.EndsWith(PlatformUtilities.GetAssetFolderPath()))
                    continue;
                AddToList(fName);
            }

            //Sort the files by extension
            mProcessedFiles.Sort(delegate (string s1, string s2)
            {
                return Path.GetExtension(s1).CompareTo(Path.GetExtension(s2));
            });

            SortedDictionary<string, AssetData> assetsList = new SortedDictionary<string, AssetData>();
            string hash = null;
            foreach (string path in mProcessedFiles)
            {
                if (ResourceManager.IsTextAsset(path))
                    hash = FileUtilities.GetMD5OfFile(EditorUtilities.mAssetsDir + path);
                else
                    hash = mAssetBundleManifest.GetAssetBundleHash(path).ToString();
                assetsList.Add(path, new AssetData(hash, FileUtilities.GetFileSize(EditorUtilities.mAssetsDir + path), AssetConfigDataHelper.GetModuleNameByName(Path.GetFileNameWithoutExtension(path))));
            }

            string filePath = EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName;
            File.WriteAllText(filePath, assetsList.ToJson());
            hash = FileUtilities.GetMD5OfFile(filePath);
            Debug.Log(filePath + " : " + hash);
            GUIUtility.systemCopyBuffer = hash;
        }

        private static void AddToList(string fName)
        {
            string fileName = fName.Replace("\\", "/");
            fileName = fileName.Replace(EditorUtilities.mAssetsDir, "");
            if (!mProcessedFiles.Contains(fileName))
                mProcessedFiles.Add(fileName);
        }
    }
#endif //UNITY_WEBPLAYER
}