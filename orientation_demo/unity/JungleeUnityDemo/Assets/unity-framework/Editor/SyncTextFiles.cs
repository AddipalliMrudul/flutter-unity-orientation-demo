using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class SyncTextFiles : ScriptableWizard
    {
        [MenuItem("Assets/Sync file")]
        private static void DoSyncTextFiles()
        {
            SyncFiles();
        }

        [MenuItem("Assets/Sync file", true)]
        private static bool DoSyncTextFilesValidate()
        {
            foreach (Object obj in Selection.objects)
            {
                string sourceFilePath = AssetDatabase.GetAssetPath(obj);
                if (!sourceFilePath.EndsWith(".xml") && !sourceFilePath.EndsWith(".csv") && !sourceFilePath.EndsWith(".json"))
                    return false;
            }
            return true;
        }

        private static void SyncFiles()
        {
            foreach (Object obj in Selection.objects)
            {
                string sourceFileName = obj.name;
                string sourceFilePath = AssetDatabase.GetAssetPath(obj);
                //Remove Assets,
                sourceFilePath = sourceFilePath.Replace("Assets/", "");
                //Remove the file name
                sourceFilePath = sourceFilePath.Replace(sourceFileName, "");
                string extn = Path.GetExtension(sourceFilePath);
                //Remove extension
                sourceFilePath = sourceFilePath.Replace(extn, "");
                foreach (PlatformUtilities.Platform platform in ProductSettings.GetSupportedPlatforms())
                {
                    if (platform != PlatformUtilities.GetCurrentPlatform())
                        SyncFile(AssetDatabase.GetAssetPath(obj), "Assets/" + PlatformUtilities.GetAssetFolderByPlatform(platform)+ "/" + sourceFileName + extn);
                }
            }
        }

        private static void SyncFile(string inSource, string inTarget)
        {
            if (File.Exists(inTarget))
            {
                if (File.GetLastWriteTime(inTarget) > File.GetLastWriteTime(inSource))
                {
                    string data = $"{inTarget} \nis newer than \n{inSource} file.";
                    bool cancelCopy = !EditorUtility.DisplayDialog("Data loss warning", data, "Overwrite", "Skip");
                    if (cancelCopy)
                        return;
                }
            }
            FileUtilities.CreateDirectoryRecursively(Path.GetDirectoryName(inTarget));

            File.Copy(inSource, inTarget, true);
        }
    }
}
