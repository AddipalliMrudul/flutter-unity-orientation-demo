using System.Collections.Generic;
using System.IO;
using XcelerateGames.Editor.Build;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class S3UploaderMenu
    {
        private const string BaseCommand = "Assets/" + Utilities.MenuName;
        
        [MenuItem(BaseCommand + "S3/Upload Selected")]
        private static void UploadSelectedFiles()
        {
            List<string> files = new List<string>();
            foreach (Object o in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                //if (IsValid(path))
                    files.Add(path);
            }
            LaunchUploader(files, false,false, false);
        }

        //[MenuItem(BaseCommand + "S3/Upload Selected", true)]
        //private static bool UploadSelectedFilesValidate()
        //{
        //    foreach (Object obj in Selection.objects)
        //    {
        //        if (IsValid(AssetDatabase.GetAssetPath(obj)))
        //            return true;
        //    }
        //    return false;
        //}

        [MenuItem(BaseCommand + "S3/Upload Preset Assets")]
        private static void UploadPresetAssets()
        {
            List<string> files = new List<string>(GetFilesToUpload.FindAllFilesToUpload());
            for (int i = 0; i < files.Count; ++i)
                files[i] = PlatformUtilities.GetAssetDirectoryForPlatform(PlatformUtilities.GetCurrentPlatform()) + files[i];
            LaunchUploader(files, false, false, false);
        }

        public static void UploadPresetAssetsSilent()
        {
            List<string> files = new List<string>(GetFilesToUpload.FindAllFilesToUpload());
            for (int i = 0; i < files.Count; ++i)
                files[i] = PlatformUtilities.GetAssetDirectoryForPlatform(PlatformUtilities.GetCurrentPlatform()) + files[i];
            LaunchUploader(files, false, false, true);
        }

        [MenuItem(BaseCommand + "S3/Upload All Assets")]
        private static void UploadAllAssets()
        {
            List<string> files = new List<string>(System.IO.Directory.GetFiles(EditorUtilities.mAssetsDir));
            files.RemoveAll(e => e.EndsWith(".manifest") || e.EndsWith(".meta"));
            LaunchUploader(files, false, false, false);
        }

        //Uploaded files that are modified in last 10 mins from platform specific data folder ex : Data_Android, Data_iOS etc 
        [MenuItem(BaseCommand + "S3/Upload Modified/10 mins")]
        private static void UploadRecentlyUpdatedFiles10Mins()
        {
            List<string> allFiles = EditorUtilities.GetFilesModifiedInLastMinutes(10);
            LaunchUploader(allFiles, false, false, false);
        }

        //Uploaded files that are modified in last 30 mins from platform specific data folder ex : Data_Android, Data_iOS etc 
        [MenuItem(BaseCommand + "S3/Upload Modified/30 mins")]
        private static void UploadRecentlyUpdatedFiles30Mins()
        {
            List<string> allFiles = EditorUtilities.GetFilesModifiedInLastMinutes(30);
            LaunchUploader(allFiles, false, false, false);
        }

        //Uploaded files that are modified in last 60 mins from platform specific data folder ex : Data_Android, Data_iOS etc 
        [MenuItem(BaseCommand + "S3/Upload Modified/60 mins")]
        private static void UploadRecentlyUpdatedFiles60Mins()
        {
            List<string> allFiles = EditorUtilities.GetFilesModifiedInLastMinutes(60);
            LaunchUploader(allFiles, false, false, false);
        }

        //private static bool IsValid(string fileName)
        //{
        //    return !fileName.EndsWith(".unity3d") && !fileName.EndsWith(".manifest");
        //}

        private static bool LaunchUploader(List<string> files, bool requireVersionCheck, bool takeBackup, bool silent)
        {
            S3UploaderWindow.AddToUploadList(files, requireVersionCheck, takeBackup, silent);
            return true;
        }
    }
}