using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor.Build
{
    public class GetFilesToUpload
    {
        [MenuItem(Utilities.MenuName + "Build/GetFilesToUploadToCDN")]
        public static void GetAllFilesToUploadToCDN()
        {
            string outputPath = Application.dataPath.Replace("Assets", "Binary");
            outputPath += "/" + PlatformUtilities.GetAssetFolderPath();

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Directory.CreateDirectory(outputPath);

            string[] filesToUpload = FindAllFilesToUpload();
            foreach (string file in filesToUpload)
            {
                if (File.Exists(EditorUtilities.mAssetsDir + file))
                    File.Copy(EditorUtilities.mAssetsDir + file, outputPath + Path.DirectorySeparatorChar + file, true);
                else
                    Debug.LogError("File does not exist : " + EditorUtilities.mAssetsDir + file);
            }
            EditorUtility.DisplayDialog("Done!", "Your files have been copied to : " + outputPath, "Ok");
        }

        [MenuItem(Utilities.MenuName + "Build/GetFilesToUploadToCDN(Hashed)")]
        public static void GetAllFilesToUploadToCDNWithHash()
        {
            string outputPath = Application.dataPath.Replace("Assets", "Binary");
            outputPath += "/" + PlatformUtilities.GetAssetFolderPath();

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Directory.CreateDirectory(outputPath);

            string[] filesToUpload = FindAllFilesToUpload();
            SortedDictionary<string, AssetData> versionList = EditorUtilities.GetVersionList();
            foreach (string file in filesToUpload)
            {
                if (File.Exists(EditorUtilities.mAssetsDir + file))
                {
                    string dirPath = outputPath + Path.DirectorySeparatorChar + file;
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    if (versionList.ContainsKey(file))
                        File.Copy(EditorUtilities.mAssetsDir + file, dirPath + Path.DirectorySeparatorChar + versionList[file].hash, true);
                    else
                        Debug.LogError("Key not found in version list: " + file);
                }
                else
                    Debug.LogError("File does not exist : " + EditorUtilities.mAssetsDir + file);
            }
            EditorUtility.DisplayDialog("Done!", "Your files have been copied to : " + outputPath, "Ok");
            EditorUtility.RevealInFinder(outputPath);
        }

        public static string[] FindAllFilesToUpload()
        {

            List<string> mFilesToUpload = new List<string>();
            //mFilesToUpload.Add (PlatformUtilities.GetAssetFolderPath());
            mFilesToUpload.Add(ResourceManager.mAssetVersionListFileName);
            //mFilesToUpload.Add (ResourceManager.mJsonAssetVersionListFileName);
            mFilesToUpload.Add(ResourceManager.PrimaryPrefetchList);
            mFilesToUpload.Add(ResourceManager.SecondaryPrefetchList);
            List<string> prefetchList = ShipAssetWithApp.GetSecondaryPrefetchList();
            foreach (string asset in prefetchList)
                mFilesToUpload.Add(asset);

            List<string> allXMLFiles = new List<string>(Directory.GetFiles(EditorUtilities.mAssetsDir, "*.xml"));
            for (int i = 0; i < allXMLFiles.Count; ++i)
            {
                allXMLFiles[i] = Path.GetFileName(allXMLFiles[i]);
                //Removes duplicate entries
                mFilesToUpload.Remove(allXMLFiles[i]);
            }

            mFilesToUpload.AddRange(allXMLFiles);

            return mFilesToUpload.ToArray();
        }
    }
}
