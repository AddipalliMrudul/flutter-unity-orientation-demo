#if USING_FLUTTER
using UnityEditor;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Editor;
using System.IO;

namespace JungleeGames.FlutterBuild.Editor
{
    /// <summary>
    /// Represents a window for exporting the Android artifact.
    /// </summary>
    public class ExportArtifact : EditorWindow
    {
        private string mDescription = null;

        /// <summary>
        /// Performs the export of the Android artifact.
        /// </summary>
        [MenuItem(Utilities.MenuName + "Build/Export Artifact", false, 1)]
        public static void DoExportAndroidArtifact()
        {
            GetWindow<ExportArtifact>();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            mDescription = EditorGUILayout.TextField("Description", mDescription);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export"))
            {
                //Export the library as unityLibrary
                string exportPath = null;
                //This is because, Android & iOS have different directory name, android is lowercase & iOS is in Uppercase
                string unityLibrary = null;
#if UNITY_ANDROID
                BuildAndroid.DoBuildAndroidLibrarySilent();
                exportPath = BuildAndroid.pExportPath;
                unityLibrary = "unityLibrary";
#elif UNITY_IOS
                BuildiOS.DoBuildIOS();
                exportPath = BuildiOS.pExportPath;
                unityLibrary = "UnityLibrary";
#endif

                string cwd = exportPath.Replace($"/{unityLibrary}", string.Empty);
                string compressedFileName = Path.GetFileName(exportPath);
                //Compress the unityLibrary folder
                EditorUtilities.Compress(compressedFileName, compressedFileName, cwd);

                string description = mDescription.IsNullOrEmpty() ? "" : mDescription + "-";
                string fileName = $"{description}{GetProcessorType()}-{unityLibrary}.zip";
                string destinationPath = exportPath.Replace($"{unityLibrary}", fileName);
                File.Move(exportPath + ".zip", destinationPath);
                Debug.Log($"Unity artifact exported to: {destinationPath}");
                EditorUtility.DisplayDialog("Exported", $"Unity artifact exported to: {destinationPath}", "OK");
                EditorUtility.RevealInFinder(destinationPath);
                Close();
            }
        }

        /// <summary>
        /// Gets the processor type of the current system.
        /// </summary>
        /// <returns>The processor type as a string. Possible values are "Intel" or "Apple".</returns>
        private static string GetProcessorType()
        {
            if (UnityEngine.SystemInfo.processorType.ToLower().Contains("intel"))
                return "Intel";
            return "Apple";
        }
    }
}
#endif // USING_FLUTTER