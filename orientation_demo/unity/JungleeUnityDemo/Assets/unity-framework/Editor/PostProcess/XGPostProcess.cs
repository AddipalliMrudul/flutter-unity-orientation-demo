using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.Diagnostics;
using System.Collections;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

namespace XcelerateGames.Editor.Build
{
//    public class XGPostProcess
//    {
//        // Set PostProcess priority to a high number to ensure that the this is started last.
//        [PostProcessBuild(900)]
//        public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
//        {
//#if UNITY_IOS

//            if (target == BuildTarget.iOS)
//            {
//                const string postProcessPlayerPath = "Assets/Framework/Files/XCode";
//                string infoPlistPath = Application.dataPath + "/Framework/Files/XCode";
//                // Current path while executing the script is
//                // the project root folder.

//                Process myCustomProcess = new Process();
//                myCustomProcess.StartInfo.FileName = "python";
//                myCustomProcess.StartInfo.Arguments = string.Format(postProcessPlayerPath + "/CGPostProcessBuildPlayer " + "\"" + pathToBuildProject + "\"" + " " + target + " " + infoPlistPath);
//                myCustomProcess.StartInfo.UseShellExecute = false;
//                myCustomProcess.StartInfo.RedirectStandardOutput = false;
//                myCustomProcess.Start();
//                myCustomProcess.WaitForExit();

//                UpdateXcodePlist(pathToBuildProject);
//                UpdateBuildSettings(pathToBuildProject);
//            }
//#endif
//        }

//        public static void UpdateXcodePlist(string pathToBuiltProject)
//        {
//#if UNITY_IOS

//            // Get plist
//            string plistPath = pathToBuiltProject + "/Info.plist";
//            PlistDocument plist = new PlistDocument();
//            plist.ReadFromString(File.ReadAllText(plistPath));

//            // Get root
//            PlistElementDict rootDict = plist.root;

//            rootDict.SetString("ITSAppUsesNonExemptEncryption", "NO");

//            // To select iPhone launch screen
//            rootDict.SetString("UILaunchStoryboardName", "LaunchScreen-iPhone");
//            // Write to file
//            File.WriteAllText(plistPath, plist.WriteToString());
//#endif
//        }

//        public static void UpdateBuildSettings(string pathToBuiltProject)
//        {
//#if UNITY_IOS

//            string projPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");
//            PBXProject project = new PBXProject();
//            project.ReadFromString(File.ReadAllText(projPath));
//            string target = project.TargetGuidByName("Unity-iPhone");
//            project.SetBuildProperty(target, "ENABLE_BITCODE", "false");
//            project.WriteToFile(projPath);
//#endif
//        }

//    }
}
