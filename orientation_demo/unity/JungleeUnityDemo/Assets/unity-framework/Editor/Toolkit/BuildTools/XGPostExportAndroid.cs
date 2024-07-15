#if UNITY_ANDROID
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace XcelerateGames.Editor.Build
{
    public class XGPostExportAndroid
    {
        private static string BuildGradleFilePath = null;

    	[PostProcessBuild(999999)]
        public static void PostExportAndroid(BuildTarget buildTarget, string pathToBuiltProject)
        {
            Debug.Log($">> {buildTarget}   {pathToBuiltProject}");
            if (buildTarget == BuildTarget.Android)
            {
                #region Update gradle file
                BuildGradleFilePath = $"/{pathToBuiltProject}/{PlayerSettings.productName}/build.gradle";
                if (File.Exists(BuildGradleFilePath))
                {
                    UpdateBuildGradleFile();
                }
                else
                    Debug.LogWarning($"Not updating build.gradle file we could not find {BuildGradleFilePath}");
                #endregion Update gradle file

#region Export symbols
#if UNITY_2021_1_OR_NEWER
#else
                ExportSymbols(pathToBuiltProject);
#endif
#endregion Export symbols
            }
        }

        private static void UpdateBuildGradleFile()
        {
            string[] lines = File.ReadAllLines(BuildGradleFilePath);
            for(int i = 0; i < lines.Length; ++i)
            {
                if(lines[i].Contains("{BUILD_NUMBER}"))
                {
                    lines[i] = lines[i].Replace("{BUILD_NUMBER}", ProductSettings.pInstance._BuildNumber);
                    lines[i] = lines[i].Replace("{APP_NAME}", ProductSettings.pInstance._AppName);
                    lines[i] = lines[i].Replace("{BUNDLE_ID}", PlayerSettings.Android.bundleVersionCode.ToString());
                    lines[i] = lines[i].Replace("{APP_VERSION}", ProductSettings.GetProductVersion());
                }
            }
            File.WriteAllLines(BuildGradleFilePath, lines);
        }

        private static void ExportSymbols(string pathToBuiltProject)
        {
            string outputDir = $"{Directory.GetParent(pathToBuiltProject)}/Symbols-v{ProductSettings.GetProductVersion()}-b{ProductSettings.pInstance._BuildNumber}";
            FileUtilities.DeleteDirectory(outputDir);
            FileUtil.CopyFileOrDirectory(EditorUtilities.ProjectRoot() + "/Temp/StagingArea/libs", outputDir);
        }
    }
}
#endif