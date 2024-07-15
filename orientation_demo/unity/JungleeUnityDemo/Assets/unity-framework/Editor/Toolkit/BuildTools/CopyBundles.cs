using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor.Build
{
    public class CopyBundles : ScriptableWizard
    {
        public bool _IgnoreTimeStamp = false;
        public bool _Data = true;

        private static bool mIsLocalBuild = true;

        static string mDataDst = Application.dataPath + "/StreamingAssets/Data";
        static string mDataFolder = "Assets/StreamingAssets/Data";

        [MenuItem(Utilities.MenuName + "Build/Copy Bundles/For Local Build")]
        public static void CopyBundlesForLocalBuild()
        {
            CopyBundles cb = ScriptableWizard.DisplayWizard("Copy Bundles", typeof(CopyBundles), "Copy") as CopyBundles;
            cb.SetState();
            mIsLocalBuild = true;
        }

        [MenuItem(Utilities.MenuName + "Build/Copy Bundles/For CDN Build")]
        public static void CopyBundlesForCDNBuild()
        {
            CopyBundles cb = ScriptableWizard.DisplayWizard("Copy Bundles", typeof(CopyBundles), "Copy") as CopyBundles;
            cb.SetState();
            mIsLocalBuild = false;
        }

        void SetState()
        {
            _Data = Directory.GetLastWriteTime(EditorUtilities.mAssetsDir) > Directory.GetLastWriteTime(mDataDst);
        }

        void OnWizardCreate()
        {
            //Create directories if they dont exist.
            FileUtilities.CreateDirectoryRecursively(mDataDst);

            if (mIsLocalBuild)
            {
                if (_Data)
                    CopyData(EditorUtilities.mAssetsDir, mDataDst, _IgnoreTimeStamp);
            }
            else
            {
                CopyCDNBundles();
                mIsLocalBuild = true;
            }

            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }

        void OnWizardUpdate()
        {
        }

        static void Copy(string inFrom, string inTo)
        {
            Debug.Log($"inFrom: {inFrom} and inTo : {inTo}");
            File.SetAttributes(inFrom, FileAttributes.Normal);
            if (File.Exists(inTo))
            {
                Debug.Log(">>>>>File.Exists");
                File.SetAttributes(inTo, FileAttributes.Normal);
            }
            File.Copy(inFrom, inTo, true);
        }

        public static void CopyCDNBundles_CloudBuild()
        {
            Debug.Log("CopyCDNBundles_CloudBuild");
            //clear current streaming assets folder
            FileUtilities.CleanDirectory(mDataFolder);
            CopyCDNBundles();
        }

        public static void CopyLocalBundles_CloudBuild()
        {
            Debug.Log("CopyLocalBundles_CloudBuild");
            //clear current streaming assets folder
            FileUtilities.CleanDirectory(mDataFolder);
            StaticCopyBundlesForLocalBuild();
        }

        static void CopyCDNBundles()
        {
            if (!Directory.Exists(mDataFolder))
                FileUtilities.CreateDirectoryRecursively(mDataFolder);
            else
                FileUtilities.CleanDirectory(mDataFolder);
            int count = 0;

            Dictionary<string, string> shippedAssets = ResourceManager.GetDefaultVersionList();

            if (shippedAssets != null)
            {
                foreach (string bundleName in shippedAssets.Keys)
                {
                    string srcFile = EditorUtilities.mAssetsDir + "/" + bundleName;
                    if (File.Exists(srcFile))
                    {
                        Copy(srcFile, mDataDst + "/" + bundleName);
                        count++;
                    }
                }
            }

            Copy(EditorUtilities.mAssetsDir + "/" + ResourceManager.mAssetVersionListFileName, mDataDst + "/" + ResourceManager.mAssetVersionListFileName);

            Debug.Log("Copied " + count + " files.");
        }

        static bool IsChanged(string newFile, string oldFile)
        {
            DateTime dt1 = File.GetLastWriteTimeUtc(newFile);
            DateTime dt2 = File.GetLastWriteTimeUtc(oldFile);
            if (dt1 > dt2)
                return true;
            return false;
        }

        static bool CanCopyFile(string fileExt, string fileName)
        {
#if LIVE_BUILD
            if (BuildAppSettings.pInstance.AssetBundleToIgnore.Contains(fileName))
                return false;
#endif
            if (fileExt != ".meta" && !fileName.Contains("DS_Store") && fileExt != ".manifest")
                return true;
            return false;
        }

        static void CopyData(string from, string targetPath, bool ignoreTimeStamp)
        {
            //Debug.Log("Copying : " + from  + " to " + targetPath);
            if (!Directory.Exists(from))
            {
                Debug.LogError("Directory not found : " + from);
                return;
            }

            if (!Directory.Exists(targetPath))
                FileUtilities.CreateDirectoryRecursively(targetPath);
            //Set the directory write time.
            Directory.SetLastWriteTime(targetPath, DateTime.Now);
            string[] files = Directory.GetFiles(from);

            // Copy the files and overwrite destination files if they already exist.
            int count = 0;

            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                string fileName = Path.GetFileName(s);
                string destFile = Path.Combine(targetPath, fileName);
                string fileExt = Path.GetExtension(s);
                if (CanCopyFile(fileExt, fileName))
                {
                    if (ignoreTimeStamp || IsChanged(s, destFile))
                    {
                        //Debug.Log(System.IO.Path.GetFileName(s));
                        Copy(s, destFile);
                        //mBundles.Add(destFile);
                        count++;
                    }
                }
            }

            if (count > 0)
                Debug.Log("Copied : " + count + " files out of " + files.Length);
        }

        public static void StaticCopyBundlesForLocalBuild()
        {
            CopyData(EditorUtilities.mAssetsDir, mDataDst, true);
        }
    }
}