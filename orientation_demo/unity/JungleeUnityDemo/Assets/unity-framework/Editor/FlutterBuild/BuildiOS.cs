#if UNITY_IOS && USING_FLUTTER
using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames;

namespace JungleeGames.FlutterBuild.Editor
{
    public class BuildiOS : Build
    {
        static readonly string iosExportPath = Path.GetFullPath(Path.Combine(ProjectPath, "../../ios/UnityLibrary"));
        public static string pExportPath { get {return iosExportPath;}}

        [MenuItem(Utilities.MenuName + "Build/Export IOS %&i", false, 3)]
        public static void DoBuildIOS()
        {
            BuildiOS build = GetWindow<BuildiOS>(true, "Export iOS", true);

            if (Directory.Exists(iosExportPath))
                Directory.Delete(iosExportPath, true);

            //EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;
            EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;

            var options =  BuildOptions.None;
            build.DoBuild(options, iosExportPath, false);
        }

        [MenuItem(Utilities.MenuName + "Build/Export IOS without Prompt %&j", false, 3)]
        public static void DoBuildIOSPipeline()
        {
            BuildiOS build =  GetWindow<BuildiOS>(true, "Export iOS", true);

            if (Directory.Exists(iosExportPath))
                Directory.Delete(iosExportPath, true);

            //EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;
            EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;

            var options =  BuildOptions.None;
            build.DoBuild(options, iosExportPath, true);
        }

        protected override void ApplyProjectSettings()
        {
            Debug.Log("Applying iOS specific project settings");
            base.ApplyProjectSettings();
            PlayerSettings.iOS.requiresFullScreen = false;
        }

        public override void OnBuildCreated()
        {
   //         string iosRunnerPath = Path.GetFullPath(Path.Combine(ProjectPath, "../../ios/Runner"));
   //         var info_file = Path.Combine(iosRunnerPath, "info.plist");
   //         var info_text = File.ReadAllText(info_file);

   //         if (!Regex.IsMatch(info_text, @"<key>io.flutter.embedded_views_preview</key>"))
   //         {
   //             Regex regex = new Regex(@"</dict>", RegexOptions.Multiline);
   //             info_text = regex.Replace(info_text, @"
			//	<key>io.flutter.embedded_views_preview</key>
			//	<true/>
			//</dict>
			//");
   //             File.WriteAllText(info_file, info_text);
   //         }
        }
    }
}
#endif //UNITY_IOS && USING_FLUTTER