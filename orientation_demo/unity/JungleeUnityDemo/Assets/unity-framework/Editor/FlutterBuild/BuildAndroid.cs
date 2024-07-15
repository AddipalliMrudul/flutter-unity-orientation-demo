#if UNITY_ANDROID && USING_FLUTTER
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
    public class BuildAndroid : Build
    {
        static readonly string androidExportPath = Path.GetFullPath(Path.Combine(ProjectPath, "../../android/unityLibrary"));

        public static string pExportPath { get {return androidExportPath;}}

        [MenuItem(Utilities.MenuName + "Build/Export Android %&n", false, 1)]
        public static void DoBuildAndroidLibrary()
        {
            string apkPath = Path.Combine(ProjectPath, "Builds/" + Application.productName + ".apk");
            BuildAndroid build = GetWindow<BuildAndroid>(true, "Export Android", true);

            bool buildSucceeded = build.DoBuildAndroid(false);

            // Copy over resources from the launcher module that are used by the library
            if (buildSucceeded)
                build.Copy(Path.Combine(apkPath + "/launcher/src/main/res"), Path.Combine(androidExportPath, "src/main/res"));
        }

        public static void DoBuildAndroidLibrarySilent()
        {
            string apkPath = Path.Combine(ProjectPath, "Builds/" + Application.productName + ".apk");
            BuildAndroid build = GetWindow<BuildAndroid>(true, "Export Android", true);

            bool buildSucceeded = build.DoBuildAndroid(true);

            // Copy over resources from the launcher module that are used by the library
            if (buildSucceeded)
                build.Copy(Path.Combine(apkPath + "/launcher/src/main/res"), Path.Combine(androidExportPath, "src/main/res"));
        }

        protected override void ApplyProjectSettings()
        {
            Debug.Log("Applying Android specific project settings");
            base.ApplyProjectSettings();

            PlayerSettings.use32BitDisplayBuffer = false;
        }

        public bool DoBuildAndroid(bool noPrompt)
        {
            string apkPath = Path.Combine(ProjectPath, "Builds/" + Application.productName + ".apk");
            bool buildSucceeded = false;
            if (Directory.Exists(apkPath))
                Directory.Delete(apkPath, true);

            if (Directory.Exists(androidExportPath))
                Directory.Delete(androidExportPath, true);

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
#if UNITY_2021_1_OR_NEWER
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
#else
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
#endif
            BuildOptions options = BuildOptions.None;
            BuildReport buildReport = DoBuild(options, apkPath, noPrompt);

            buildSucceeded = buildReport?.summary.result == BuildResult.Succeeded;
            if (buildSucceeded && !PlatformUtilities.IsLocalBuild())
            {
                S3UploaderMenu.UploadPresetAssetsSilent();
            }
            return buildSucceeded;
        }

        public override void OnBuildCreated()
        {
            string apkPath = Path.Combine(ProjectPath, "Builds/" + Application.productName + ".apk");
            Copy(Path.Combine(apkPath, "unityLibrary"), androidExportPath);
            // Modify build.gradle
            var build_file = Path.Combine(androidExportPath, "build.gradle");
            var build_text = File.ReadAllText(build_file);
            build_text = build_text.Replace("com.android.application", "com.android.library");
            build_text = build_text.Replace("bundle {", "splits {");
            build_text = build_text.Replace("enableSplit = false", "enable false");
            build_text = build_text.Replace("enableSplit = true", "enable true");
            build_text = build_text.Replace("implementation fileTree(dir: 'libs', include: ['*.jar'])", "implementation(name: 'unity-classes', ext:'jar')");
#if UNITY_2020_2_OR_NEWER
            build_text = build_text.Replace("android.ndkDirectory", EditorUtilities.GetNdkPath());
#endif
            build_text = Regex.Replace(build_text, @"\n.*applicationId '.+'.*\n", "\n");
            File.WriteAllText(build_file, build_text);

            // Modify AndroidManifest.xml
            var manifest_file = Path.Combine(androidExportPath, "src/main/AndroidManifest.xml");
            var manifest_text = File.ReadAllText(manifest_file);
            manifest_text = Regex.Replace(manifest_text, @"<application .*>", "<application>");
            Regex regex = new Regex(@"<activity.*>(\s|\S)+?</activity>", RegexOptions.Multiline);
            manifest_text = regex.Replace(manifest_text, "");
            File.WriteAllText(manifest_file, manifest_text);


            string androidPath = Path.GetFullPath(Path.Combine(ProjectPath, "../../android"));
            string androidAppPath = Path.GetFullPath(Path.Combine(ProjectPath, "../../android/app"));
            var proj_build_path = Path.Combine(androidPath, "build.gradle");
            var app_build_path = Path.Combine(androidAppPath, "build.gradle");
            var settings_path = Path.Combine(androidPath, "settings.gradle");

            var proj_build_script = File.ReadAllText(proj_build_path);
            var settings_script = File.ReadAllText(settings_path);
            var app_build_script = File.ReadAllText(app_build_path);

            // Sets up the project build.gradle files correctly
            if (!Regex.IsMatch(proj_build_script, @"flatDir[^/]*[^}]*}"))
            {
                regex = new Regex(@"allprojects \{[^\{]*\{", RegexOptions.Multiline);
                proj_build_script = regex.Replace(proj_build_script, @"
allprojects {
    repositories {
        flatDir {
            dirs ""${project(':unityLibrary').projectDir}/libs""
        }
");
                File.WriteAllText(proj_build_path, proj_build_script);
            }

            // Sets up the project settings.gradle files correctly
            if (!Regex.IsMatch(settings_script, @"include "":unityLibrary"""))
            {
                settings_script += @"

include "":unityLibrary""
project("":unityLibrary"").projectDir = file(""./unityLibrary"")
";
                File.WriteAllText(settings_path, settings_script);
            }

            // Sets up the project app build.gradle files correctly
            if (!Regex.IsMatch(app_build_script, @"dependencies \{"))
            {
                app_build_script += @"

dependencies {
    implementation project(':unityLibrary')
}
";
                File.WriteAllText(app_build_path, app_build_script);
            }
            else
            {
                if (!app_build_script.Contains(@"implementation project(':unityLibrary')"))
                {
                    regex = new Regex(@"dependencies \{", RegexOptions.Multiline);
                    app_build_script = regex.Replace(app_build_script, @"
dependencies {
    implementation project(':unityLibrary')
");
                    File.WriteAllText(app_build_path, app_build_script);
                }
            }
        }
    }
}
#endif //UNITY_ANDROID && USING_FLUTTER