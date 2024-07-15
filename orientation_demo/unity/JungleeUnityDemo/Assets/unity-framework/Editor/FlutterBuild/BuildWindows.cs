#if UNITY_STANDALONE_WIN
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Editor;
using Application = UnityEngine.Application;

namespace JungleeGames.FlutterBuild.Editor
{
    public class BuildWindows : Build
    {
        static readonly string ExportPath = Path.GetFullPath(Path.Combine(ProjectPath,
            "../../../poker/poker.exe"));

        [MenuItem(Utilities.MenuName + "Build/Export Windows %&n", false, 1)]
        public static void DoBuildWindows()
        {
            BuildWindows build = GetWindow<BuildWindows>(true, "Export Windows", true);

            bool buildSucceeded = build.DoBuildWindows(true);

            // Copy over resources from the launcher module that are used by the library
          
        }

        protected override void OnPreProcess(bool noPrompt)
        {
            base.OnPreProcess(noPrompt);

            List<string> excludedVariants = new List<string>() { ".desktop", ".psdesktop", ".mobile", ".psmobile" };
#if POKER_STARS_DESKTOP
            excludedVariants.Remove(".psdesktop");
#elif JUNGLEE_POKER_DESKTOP
            excludedVariants.Remove(".desktop");
#elif JUNGLEE_POKER_MOBILE
            excludedVariants.Remove(".mobile");
#elif POKER_STARS_MOBILE
            excludedVariants.Remove(".psmobile");
#else
            excludedVariants.Clear();
#endif
            string mDataFolder = "Assets/StreamingAssets/Data";
            string[] files = Directory.GetFiles(mDataFolder);

            foreach (string s in files)
            {
                string fileExt = Path.GetExtension(s);

                if (!string.IsNullOrEmpty(fileExt) && excludedVariants.Contains(fileExt))
                    AssetDatabase.DeleteAsset(s);
            }
        }

        public bool DoBuildWindows(bool noPrompt)
        {
            bool buildSucceeded = false;

            if (Directory.Exists(ExportPath))
                Directory.Delete(ExportPath, true);

            BuildOptions options = BuildOptions.None;
            BuildReport buildReport = DoBuild(options, ExportPath, noPrompt);

            buildSucceeded = buildReport?.summary.result == BuildResult.Succeeded;
            if (buildSucceeded && !PlatformUtilities.IsLocalBuild())
            {
                S3UploaderMenu.UploadPresetAssetsSilent();
            }
            return buildSucceeded;
        }
    }
}
#endif
