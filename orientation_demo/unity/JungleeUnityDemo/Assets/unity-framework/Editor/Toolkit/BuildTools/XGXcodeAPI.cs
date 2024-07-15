#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace XcelerateGames.Editor.Build
{
    public class XGXcodeAPI
    {
    	//[PostProcessBuild(999999)]
        public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget == BuildTarget.iOS)
            {
    			UpdateInfoList(pathToBuiltProject);
    			AddCapabilities(pathToBuiltProject);
                AddFrameworks(pathToBuiltProject);
                UpdateBuildProperties(pathToBuiltProject);
            }
        }

    	private static void UpdateInfoList(string pathToBuiltProject)
    	{
    		Debug.Log ("Updating Info.plist with our custom settings.");
    		// Get plist created by Unity
    		string plistPath = pathToBuiltProject + "/Info.plist";
    		PlistDocument plist = new PlistDocument();
    		plist.ReadFromString(File.ReadAllText(plistPath));

    		// Get root
    		PlistElementDict rootDict = plist.root;

#region File Sharing

    		bool isFileSharingEnabled = true;
#if LIVE_BUILD
    		isFileSharingEnabled = false;
#endif
    		rootDict.SetBoolean("UIFileSharingEnabled", isFileSharingEnabled);

            #endregion File Sharing

            #region Facebook Settings
#if USE_FB_SDK

    		rootDict.SetString("FacebookAppID", Facebook.Unity.Settings.FacebookSettings.AppId);
#endif //USE_FB_SDK
            #endregion Facebook Settings

            #region Permissions & Descriptions

            rootDict.SetString("NSCameraUsageDescription", "To Check CameraUsage");
    		rootDict.SetString("NSLocationWhenInUseUsageDescription", "To Check Location");
    		rootDict.SetString("NSMicrophoneUsageDescription", "To Check Voice");
    		rootDict.SetString("NSCalendarsUsageDescription", "To Check Date");
    		rootDict.SetString("NSPhotoLibraryUsageDescription", "To save and read user profile pic locally");

#endregion Permissions & Descriptions

#region Bundle URL Types

    		PlistElementArray bundleUrlsArray = rootDict.CreateArray("CFBundleURLTypes");
    		PlistElementDict bundleUrlsDict = bundleUrlsArray.AddDict();
    		bundleUrlsDict.SetString("CFBundleURLName", "facebook-unity-sdk");
    		PlistElementArray array = bundleUrlsDict.CreateArray("CFBundleURLSchemes");
#if USE_FB_SDK
            array.AddString("fb" + Facebook.Unity.Settings.FacebookSettings.AppId);
#endif //USE_FB_SDK
            array.AddString(PlayerSettings.applicationIdentifier);

#endregion Bundle URL Types

#region LSApplicationQueriesSchemes

    		PlistElementArray applicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
    		applicationQueriesSchemes.AddString("sms");
    		applicationQueriesSchemes.AddString("mailto");
    		applicationQueriesSchemes.AddString("fb");
    		applicationQueriesSchemes.AddString("fbapi");
    		applicationQueriesSchemes.AddString("whatsapp");
    		applicationQueriesSchemes.AddString("twitter");
    		applicationQueriesSchemes.AddString("AppBundleId");
    		applicationQueriesSchemes.AddString("fb-messenger-api");
    		applicationQueriesSchemes.AddString("fbauth2");
    		applicationQueriesSchemes.AddString("fbshareextension");

#endregion LSApplicationQueriesSchemes

    		// Write to file
    		File.WriteAllText(plistPath, plist.WriteToString());
    	}

    	private static void AddCapabilities(string pathToBuiltProject)
    	{
    		string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

    		PBXProject proj = new PBXProject();
    		proj.ReadFromString(File.ReadAllText(projPath));

    		string target = proj.TargetGuidByName("Unity-iPhone");

            proj.AddCapability(target, PBXCapabilityType.PushNotifications);
            proj.AddCapability(target, PBXCapabilityType.GameCenter);
    		proj.AddCapability(target, PBXCapabilityType.InAppPurchase);

    		File.WriteAllText(projPath, proj.WriteToString());
    	}

        private static void AddFrameworks(string pathToBuiltProject)
        {
            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            proj.AddFrameworkToProject(target, "UserNotifications.framework", false);
            File.WriteAllText(projPath, proj.WriteToString());
        }

        private static void UpdateBuildProperties(string pathToBuiltProject)
        {
            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
#endif