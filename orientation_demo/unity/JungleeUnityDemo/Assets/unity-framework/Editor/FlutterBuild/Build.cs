#if USING_FLUTTER
using System;
using System.IO;
#if AUTOMATION_ENABLED
using AltTester.AltTesterUnitySDK.Editor;
#endif
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Editor;
using XcelerateGames.Editor.AssetBundles;
using XcelerateGames.Editor.Build;
using Application = UnityEngine.Application;

namespace JungleeGames.FlutterBuild.Editor
{
    public class Build : EditorWindow
    {
        public static readonly string ProjectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        public void Copy(string source, string destinationPath)
        {
            if (Directory.Exists(destinationPath))
                Directory.Delete(destinationPath, true);

            Directory.CreateDirectory(destinationPath);

            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destinationPath));

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destinationPath), true);
        }

        public BuildReport DoBuild(BuildOptions buildOptions, string outputPath, bool noPrompt)
        {
            OnPreProcess(noPrompt);
            if (!noPrompt)
            {
                bool dlgResult = EditorUtility.DisplayDialog("Info", "Click continue if you saw Asset Import UI, else click Cancel", "Continue", "Cancel");
                if (!dlgResult)
                {
                    Debug.Log("Build cancelled");
                    Close();
                    return null;
                }
            }

            buildOptions |= BuildOptions.CompressWithLz4;
#if AUTOMATION_ENABLED
            buildOptions |= BuildOptions.IncludeTestAssemblies;
#endif
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = BuildAppSettings.BuildScenes();
            buildPlayerOptions.locationPathName = outputPath;
            buildPlayerOptions.assetBundleManifestPath = EditorUtilities.mAssetsDir + PlatformUtilities.GetAssetFolderPath();
            buildPlayerOptions.options = buildOptions;
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;

#if AUTOMATION_ENABLED
            var buildTargetGroup = BuildTargetGroup.Android;
            AltBuilder.AddAltTesterInScriptingDefineSymbolsGroup(buildTargetGroup);
#endif
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("BuildPlayer result : " + report.summary.result);
#if AUTOMATION_ENABLED
            AltBuilder.RemoveAltTesterFromScriptingDefineSymbols(BuildTargetGroup.Android);
#endif
            if (report.summary.result == BuildResult.Succeeded)
                OnBuildCreated();

            if (!noPrompt)
            {
                bool okayClicked = EditorUtility.DisplayDialog("Exported", "Build exported successfully", "Okay", "Open");
                if (!okayClicked)
                    EditorUtility.RevealInFinder(outputPath);
            }

            Close();
            return report;
        }

        protected virtual void OnPreProcess(bool noPrompt)
        {
            PlayerSettings.stripUnusedMeshComponents = true;

            ApplyProjectSettings();

            //Generate bundles
            BuildAssetBundle.CreateAssetBundleFromAssetDatabase();

            //Create a list of assets that are shipped with the app
            ShipAssetWithApp.AddAssetsToShipWithApp();
            //Create version list.
            CreateAssetVersionList.DoCreateAssetVersionList();
            //Generate the prefetch xml.
            GeneratePrefetchXml.GenerateXML();
            //Copy all bundles
            AssetDatabase.ImportAsset(EditorUtilities.mAssetsDir, ImportAssetOptions.ImportRecursive);

            if (PlatformUtilities.IsLocalBuild())
                CopyBundles.CopyLocalBundles_CloudBuild();
            else
                CopyBundles.CopyCDNBundles_CloudBuild();

            if (!noPrompt)
                EditorUtility.DisplayDialog("Info", "Assets will import now, click continue", "Continue");

            AssetDatabase.ImportAsset("Assets/StreamingAssets", ImportAssetOptions.ImportRecursive);
        }

        public virtual void OnBuildCreated()
        {

        }

        protected virtual void ApplyProjectSettings()
        {

        }
    }
}
#endif //USING_FLUTTER