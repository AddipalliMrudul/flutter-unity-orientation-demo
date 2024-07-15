#if PLATFORM_STANDALONE_OSX
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Editor;

namespace JungleeGames.FlutterBuild.Editor
{
    //TODO: Separate the Poker Specific Code in poker repo
    public class BuildMacOS : Build
    {
        private static string ExportPath = Path.GetFullPath(Path.Combine(ProjectPath, "../Builds/macos/AppName"));
        protected static string pExportPath { get { return ExportPath; } set { ExportPath = value; } }

        // private static string DestinationZipFilePath = Path.GetFullPath(Path.Combine(ProjectPath, "../Builds/macos/appName.zip"));
        // protected static string pDestinationZipFilePath { get { return DestinationZipFilePath; } set { DestinationZipFilePath = value; } }

        [MenuItem(Utilities.MenuName + "Build/Export MacOS %&n", false, 1)]
        public static void DoBuildMac()
        {
            BuildMacOS build = GetWindow<BuildMacOS>(true, "Export MacOS", true);

            bool buildSucceeded = build.DoBuildMacOS(false);

            if (buildSucceeded)
            {
                //ZipFolderContents(ExportPath, DestinationZipFilePath);
            }
        }

        public static void DoBuildMacSilently()
        {
            BuildMacOS build = GetWindow<BuildMacOS>(true, "Export MacOS", true);

            bool buildSucceeded = build.DoBuildMacOS(true);

            if (buildSucceeded)
            {
                //ZipFolderContents(ExportPath, DestinationZipFilePath);
            }
        }

        public bool DoBuildMacOS(bool noPrompt)
        {
            bool buildSucceeded = false;

            if (Directory.Exists(ExportPath))
                Directory.Delete(ExportPath, true);

            BuildOptions options = BuildOptions.None | BuildOptions.ShowBuiltPlayer;
            BuildReport buildReport = DoBuild(options, ExportPath, noPrompt);

            buildSucceeded = buildReport?.summary.result == BuildResult.Succeeded;
            if (buildSucceeded && !PlatformUtilities.IsLocalBuild())
            {
                S3UploaderMenu.UploadPresetAssetsSilent();
            }

            return buildSucceeded;
        }

        // public static void ZipFolderContents(string sourceFolderPath, string destinationZipFilePath)
        // {
        //     try
        //     {
        //         if (File.Exists(destinationZipFilePath))
        //         {
        //             File.Delete(destinationZipFilePath);
        //         }
        //         ZipFile.CreateFromDirectory(sourceFolderPath, destinationZipFilePath);
        //         Debug.Log("Folder successfully zipped!");
        //     }
        //     catch (UnauthorizedAccessException uae)
        //     {
        //         Debug.LogError($"Unauthorized access: {uae.Message}");
        //     }
        //     catch (IOException ioe)
        //     {
        //         Debug.LogError($"IO error: {ioe.Message}");
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"Error zipping folder: {e.Message}");
        //     }
        // }
    }
}
#endif