using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
using XcelerateGames.Editor.AssetBundles;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace XcelerateGames.Editor.Build
{
    public class BuildApp : EditorWindow
    {
        private string mBuildName = "";

        //other settings
        private bool mDevelopment = false;
        private bool mBuildScriptsOnly = false;

        private bool mAllowDebugging = false;

        private bool mAutoConnectProfiler = false;
        private bool mIgnoreResources = true;

        private bool mCloseWindowOnPostBuild = true;

        //If this flag is enabled, no build will be made but all steps till the actual build will be executed.
        private bool mMockRun = false;

        //pre build
        private bool mFetchLocalisation = false;

        private AndroidBuildType mAndroidBuildType = AndroidBuildType.APK;
        //post build
#if UNITY_IOS
        private bool mEnableBitCode = false;
#elif UNITY_ANDROID
        private List<string> mAndroidBuildTypes = null;
#endif

        private BuildOptions mBuildOptions = BuildOptions.None;

        private string pDefaultBuildName
        {
            get
            {
                if (EditorApplication.isCompiling)
                    return null;

                var date = DateTime.Now.Date.ToString("ddMMMyy");
                var path = "Binary/" + PlatformUtilities.GetCurrentPlatform().ToString() + "/";
                string bundleVersionCode = string.Empty;
#if UNITY_ANDROID
                bundleVersionCode = $"({PlayerSettings.Android.bundleVersionCode})";
#endif
                path = $"{path}{ProductSettings.pInstance._AppName}-v{ProductSettings.GetCurrentProductInfo().Version}{bundleVersionCode}-b{ProductSettings.pInstance._BuildNumber}-{PlatformUtilities.GetEnvironment()}-{(PlatformUtilities.IsLocalBuild() ? "LOCAL" : "OTA")}-{date}";

#if UNITY_ANDROID
                if (mAndroidBuildType == AndroidBuildType.APK)
                    path += ".apk";
                else if (mAndroidBuildType == AndroidBuildType.AAB)
                    path += ".aab";
#endif
                return path;
            }
        }

        private string ApkName
        {
            get
            {
                if (EditorApplication.isCompiling)
                    return null;
                string extn = "apk";
                if (mAndroidBuildType == AndroidBuildType.AAB)
                    extn = ".aab";
                return $"{ProductSettings.pInstance._AppName}-v{ProductSettings.GetProductVersion()}({pCurrentPlatformBundleCode})b{ProductSettings.pInstance._BuildNumber}-{PlatformUtilities.GetEnvironment()}-{(PlatformUtilities.IsLocalBuild() ? "LOCAL" : "OTA")}.{extn}";
            }
        }

        private string BuildDir
        {
            get
            {
                if (EditorApplication.isCompiling)
                    return null;
                return $"Binary/{PlatformUtilities.GetCurrentPlatform()}";
            }
        }
        #region Tools Resources

        private static Texture2D mTexRegenBuildName;

        #endregion Tools Resources

        [MenuItem(Utilities.MenuName + "Build/Build App %&b", false, 1)]
        private static void BuildApplication()
        {
            BuildApp buildApp = EditorWindow.GetWindow<BuildApp>(true, "Build App: " + PlatformUtilities.GetCurrentPlatform().ToString(), true);
            buildApp.minSize = new Vector2(425, 320);

            if (XGEditorPrefs.HasKey("BuildName"))
            {
                var build_name = XGEditorPrefs.GetString("BuildName");
                if (build_name != null)
                    buildApp.mBuildName = build_name;
            }

            buildApp.mAllowDebugging = XGEditorPrefs.GetBool("Debug", false);

            buildApp.mAutoConnectProfiler = XGEditorPrefs.GetBool("AutoConnectProfiler", false);

            buildApp.mCloseWindowOnPostBuild = XGEditorPrefs.GetBool("CloseWindowOnBuild", true);

            buildApp.mDevelopment = XGEditorPrefs.GetBool("Development", false);

            buildApp.mAndroidBuildType = (AndroidBuildType)XGEditorPrefs.GetInt("AndroidBuildType", (int)AndroidBuildType.AndroidStudioProject);

            buildApp.mBuildScriptsOnly = XGEditorPrefs.GetBool("BuildScriptsOnly", false);

            //pre-build
            //localisation helper
            buildApp.mFetchLocalisation = false;

            buildApp.mFetchLocalisation = XGEditorPrefs.GetBool("FetchLocalisation", false);

            #region Tool Resources

            mTexRegenBuildName = Resources.Load("processing-small", typeof(Texture2D)) as Texture2D;

            #endregion Tool Resources

#if UNITY_ANDROID
            buildApp.mAndroidBuildTypes = new List<string>();
            for (AndroidBuildType buildType = AndroidBuildType.APK; buildType < AndroidBuildType.End; ++buildType)
            {
                buildApp.mAndroidBuildTypes.Add(buildType.ToString());
            }
#endif
        }

        private bool mPrevIsCompiling = false;

        private void Update()
        {   //update only if there is a change in compiling
            if (mPrevIsCompiling != EditorApplication.isCompiling)
            {
                mPrevIsCompiling = EditorApplication.isCompiling;

                string title = mPrevIsCompiling ? "*Please wait till scripts are compiled" : "Build App: " + PlatformUtilities.GetCurrentPlatform().ToString();
                Texture icon = mPrevIsCompiling ? Resources.Load("processing-small") as Texture : null;
                GUIContent content = mPrevIsCompiling ? new GUIContent(title, icon) : new GUIContent(title);

                mTexRegenBuildName = Resources.Load("processing-small") as Texture2D;

                titleContent = content;

                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

            #region build info

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Build Type : " + (PlatformUtilities.IsLocalBuild() ? "Local" : "OTA"), EditorStyles.boldLabel);
            GUILayout.Label("Server Type : " + PlatformUtilities.GetEnvironment(), EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Configure"))
            {
                ConfigureBuild.OpenConfigureWindow(true);
            }

            if (GUILayout.Button("Refresh"))
                UnityEngine.Debug.Log("Window Refreshed");

            EditorGUILayout.EndHorizontal();

            #endregion build info

            #region build settings

            EditorGUILayout.BeginHorizontal();

            //name generator
            if (GUILayout.Button(mTexRegenBuildName, GUILayout.Width(20f), GUILayout.Height(20f)))
                mBuildName = pDefaultBuildName;

            //build name
            EditorGUIUtility.labelWidth = 80f;
            GUI.backgroundColor = Color.white;
            var build_name = EditorGUILayout.TextField("Build Name", mBuildName);
            if (string.IsNullOrEmpty(build_name))
            {
                //if blank insert last saved path
                if (XGEditorPrefs.HasKey("BuildName"))
                {
                    var saved_name = XGEditorPrefs.GetString("BuildName");
                    mBuildName = saved_name;
                }
                else
                {
                    //generate a new one
                    mBuildName = pDefaultBuildName;
                }
            }
            // new data is inputted
            else if (build_name != mBuildName)
            {
                //save the new data
                mBuildName = build_name;
                XGEditorPrefs.SetString("BuildName", mBuildName);
            }

            //file chooser
            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    mBuildName = EditorUtility.SaveFilePanel("Choose Save Path", BuildDir, ApkName, "apk");
                else
                    mBuildName = EditorUtility.SaveFilePanel("Choose Save Path", BuildDir, "", "");
            }

            if (GUILayout.Button("Set Name", GUILayout.ExpandWidth(false)))
            {
                mBuildName = pDefaultBuildName;
            }

            EditorGUILayout.EndHorizontal();

            #region product settings

            //product settings
            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 80f;
            var build_number = EditorGUILayout.TextField("Internal Ver.", ProductSettings.pInstance._BuildNumber, GUILayout.ExpandWidth(false));
            ProductSettings.pInstance._BuildNumber = string.IsNullOrEmpty(build_number) ? "0" : build_number;

            EditorGUIUtility.labelWidth = 110f;
            var product_version = EditorGUILayout.TextField("Bundle/Bucket No.", pCurrentPlatformVersion, GUILayout.ExpandWidth(true));
            pCurrentPlatformVersion = string.IsNullOrEmpty(product_version) ? "0.0.0" : product_version;

            EditorGUIUtility.labelWidth = 80f;
            var build_code = EditorGUILayout.TextField("Bundle Code", pCurrentPlatformBundleCode, GUILayout.Width(110));
            pCurrentPlatformBundleCode = string.IsNullOrEmpty(build_code) ? "0" : build_code;

            EditorGUILayout.EndHorizontal();

            #endregion product settings

            #endregion build settings

            EditorGUILayout.BeginHorizontal();

            #region pre build

            //pre build
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 140f;

            EditorGUILayout.LabelField("Pre Build", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            mFetchLocalisation = EditorGUILayout.Toggle("Fetch Latest Localisation", mFetchLocalisation, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndVertical();

            //post build
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 140f;

            EditorGUILayout.LabelField("Post Build", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndVertical();

            #endregion pre build

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            #region other

            //other
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel);
#if UNITY_ANDROID
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Build Type : ");
            AndroidBuildType androidBuildType = (AndroidBuildType)EditorGUILayout.Popup((int)mAndroidBuildType, mAndroidBuildTypes.ToArray());
            bool changed = mAndroidBuildType != androidBuildType;
            mAndroidBuildType = androidBuildType;
            if (changed)
                mBuildName = pDefaultBuildName;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
#endif

            mDevelopment = EditorGUILayout.Toggle("Development", mDevelopment, GUILayout.ExpandWidth(false));
            EditorGUI.BeginDisabledGroup(!mDevelopment);
            mAllowDebugging = EditorGUILayout.Toggle("Debugging", mAllowDebugging, GUILayout.ExpandWidth(false)) && mDevelopment;
            mBuildScriptsOnly = EditorGUILayout.Toggle("Build Scripts Only", mBuildScriptsOnly, GUILayout.ExpandWidth(false)) && mDevelopment;
            mAutoConnectProfiler = EditorGUILayout.Toggle("Auto Connect Profiler", mAutoConnectProfiler, GUILayout.ExpandWidth(false)) && mDevelopment;
            EditorGUI.EndDisabledGroup();

            mMockRun = EditorGUILayout.Toggle("Mock Run", mMockRun, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndVertical();

            #endregion other

            #region Platform Specific

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Platform Settings", EditorStyles.boldLabel);
            GUILayout.Label("Bundle Version: " + PlayerSettings.bundleVersion + " Bundle Code: " + pCurrentPlatformBundleCode, EditorStyles.label);

#if UNITY_IOS
            // ios specific
            mEnableBitCode = EditorGUILayout.Toggle("Enable Bitcode", mEnableBitCode);
#elif UNITY_ANDROID

            // android specific
#if !DISABLE_ANDROID_TYPE
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 35f;

            EditorGUILayout.EndHorizontal();
#endif

#endif

            EditorGUILayout.EndVertical();

            #endregion Platform Specific

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // build
            EditorGUILayout.BeginHorizontal();

            #region build button

            int build_action = 0; // 0:no build, 1:build, 2:build and run
            if (GUILayout.Button("Build"))
                build_action = 1;
            else if (GUILayout.Button("Build & Run"))
                build_action = 2;

            if (build_action != 0)
            {
                string error = null;
                if (!BuildAppSettings.Validate(out error))
                {
                    EditorUtility.DisplayDialog("Error in Build settings", error, "Okay");
                    return;
                }
                mBuildOptions |= build_action == 1 ? BuildOptions.ShowBuiltPlayer : BuildOptions.AutoRunPlayer;
                EditorApplication.delayCall = () =>
                {
                    //license check
                    bool isPro = UnityEditorInternal.InternalEditorUtility.HasPro();
                    bool cancelBuild = false;

                    if (!isPro && PlatformUtilities.GetEnvironment() == PlatformUtilities.Environment.live)
                    {
                        UnityEngine.Debug.LogWarning("Pro License Missing");
                        cancelBuild = !EditorUtility.DisplayDialog("Pro License missing", "No pro license was found on this machine", "Continue", "Cancel");
                    }

                    if (cancelBuild)
                    {
                        UnityEngine.Debug.Log("Build Cancelled by user");
                        return;
                    }
                    else
                        UnityEngine.Debug.Log("Proceeding with build");

                    MakeBuild();
                };
            }

            #endregion build button

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void MakeBuild()
        {
            Close();
            //build path check
            var build_dir = Path.GetDirectoryName(mBuildName);
            if (!Directory.Exists(build_dir))
            {
                UnityEngine.Debug.Log("Build directory missing, creating a new one....");
                Directory.CreateDirectory(build_dir);
            }

            #region pre build

            //pre build
            if (mFetchLocalisation)
            {
                //var result = UILocalizationHelper.FetchCSV();

                //if (result == "FAILED")
                //{
                //UnityEngine.Debug.LogError("Build Cancelled");
                //return;
                //}
            }

            #endregion pre build

            #region build

            //build
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                if (!mBuildName.EndsWith(".exe"))
                    mBuildName += ".exe";
                mBuildName = mBuildName.Replace(".All files", "");
                if (!Path.IsPathRooted(mBuildName))
                    mBuildName = EditorUtilities.ProjectRoot() + "/" + mBuildName;
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX)
            {
                if (!mBuildName.EndsWith(".app"))
                    mBuildName += ".app";
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                if (!File.Exists(PlayerSettings.Android.keystoreName))
                {
                    UnityEngine.Debug.LogWarning("Keystore file not found : " + PlayerSettings.Android.keystoreName + ", defaulting to " + BuildAppSettings.pInstance.KeyStoreName);

                    if (string.IsNullOrEmpty(BuildAppSettings.pInstance.KeyStoreName))
                    {
                        UnityEngine.Debug.LogError("Default keystore name missing, aborting build");
                        EditorUtility.DisplayDialog("Error", "Default keystore name missing, aborting build", "Ok");
                        return;
                    }

                    PlayerSettings.Android.keystoreName = BuildAppSettings.pInstance.KeyStoreName;
                }

                PlayerSettings.Android.keystorePass = BuildAppSettings.pInstance.KeyStorePassword;
                PlayerSettings.Android.keyaliasName = BuildAppSettings.pInstance.KeyStoreAliasName;
                PlayerSettings.Android.keyaliasPass = BuildAppSettings.pInstance.KeyStoreAliasPassword;
                if (mAndroidBuildType == AndroidBuildType.APK)
                {
                    EditorUserBuildSettings.buildAppBundle = false;
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

                    if (!mBuildName.EndsWith(".apk"))
                    {
                        if (mBuildName.StartsWith("Binary/"))
                        {
                            if (!Directory.Exists(mBuildName) || !FileUtilities.IsDirectory(mBuildName))
                            {
                                mBuildName = EditorUtility.SaveFilePanel("Choose Save Path", BuildDir, ApkName, "apk");
                            }
                        }
                    }
                }
                else if (mAndroidBuildType == AndroidBuildType.AndroidStudioProject)
                {
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                }
                else if (mAndroidBuildType == AndroidBuildType.AAB)
                {
                    EditorUserBuildSettings.buildAppBundle = true;
                }
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
#if UNITY_IOS
                mBuildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                //0 - None, 1 - ARM64, 2 - Universal.
                PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2);
#endif
            }

            Application.runInBackground = false;
            if (mAllowDebugging)
                mBuildOptions |= BuildOptions.AllowDebugging;
            if (mDevelopment)
                mBuildOptions |= BuildOptions.Development;
            if (mBuildScriptsOnly)
                mBuildOptions |= BuildOptions.BuildScriptsOnly;

            if (mAutoConnectProfiler)
                mBuildOptions |= BuildOptions.ConnectWithProfiler;

            PlayerSettings.stripUnusedMeshComponents = true;

            //Generate bundles
            BuildAssetBundle.CreateAssetBundleFromAssetDatabase();

#if UNITY_WEBPLAYER
        	DirectoryCopy("Assets/Data", mBuildName + "/Data", true);
#else
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

            EditorUtility.DisplayDialog("Info", "Assets will import now, click continue", "Continue");

            AssetDatabase.ImportAsset("Assets/StreamingAssets", ImportAssetOptions.ImportRecursive);

            bool dlgResult = EditorUtility.DisplayDialog("Info", "Click continue if you saw Asset Import UI, else click Cancel", "Continue", "Cancel");
            if (!dlgResult)
                return;

#endif //UNITY_WEBPLAYER
            string postFix = ".IGNORE";
            if (mIgnoreResources)
            {
                //Rename the resources,
                foreach (string res in BuildAppSettings.pInstance.ResourcesFolders)
                {
                    string folderName = Path.GetFileName(res);
                    string moveResult = AssetDatabase.RenameAsset(res, folderName + postFix);
                    if (!string.IsNullOrEmpty(moveResult))
                        UnityEngine.Debug.LogError("Failed to rename asset : " + res + " :-> " + moveResult);
                }
            }
#if UNITY_ANDROID
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
#elif UNITY_IOS
            mBuildOptions |= BuildOptions.SymlinkSources;
#endif
            Build(mBuildName);

            if (mIgnoreResources)
            {
                foreach (string res in BuildAppSettings.pInstance.ResourcesFolders)
                {
                    string folderName = Path.GetFileName(res);
                    string moveResult = AssetDatabase.RenameAsset(res + postFix, folderName);
                    if (!string.IsNullOrEmpty(moveResult))
                        UnityEngine.Debug.LogError("Failed to rename asset : " + res + " :-> " + moveResult);
                }
            }

            #endregion build
        }

        private void OnDestroy()
        {
            XGEditorPrefs.SetString("BuildName", mBuildName);
            XGEditorPrefs.SetBool("Debug", mAllowDebugging);
            XGEditorPrefs.SetBool("Development", mDevelopment);
            XGEditorPrefs.SetBool("BuildScriptsOnly", mBuildScriptsOnly);
            XGEditorPrefs.SetBool("AutoConnectProfiler", mAutoConnectProfiler);

            XGEditorPrefs.SetBool("FetchLocalisation", mFetchLocalisation);
            XGEditorPrefs.SetBool("CloseWindowOnPostBuild", mCloseWindowOnPostBuild);
            XGEditorPrefs.SetInt("AndroidBuildType", (int)mAndroidBuildType);

#if UNITY_IOS
            XGEditorPrefs.SetBool("XCode_Bitcode", mEnableBitCode);
#endif
        }

        private void Build(string buildName)
        {
            if (Application.HasProLicense())
            {
#if UNITY_IOS
                PlayerSettings.SplashScreen.show = false;
#else
                PlayerSettings.SplashScreen.show = true;
#endif
                PlayerSettings.SplashScreen.showUnityLogo = false;
            }

            if (mMockRun)
                UnityEngine.Debug.Log("This is mock run");
            else
            {
#if UNITY_IOS
                if(Directory.Exists(mBuildName))
                {
                    Debug.Log("Build exists, ask user if he wants to replce or append");
                    bool replace = EditorUtility.DisplayDialog("Append?", "A build already exists", "Append", "Replace");
                    if (replace)
                        mBuildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                    else
						Directory.Delete(mBuildName, true);
                }
#endif
                mBuildOptions |= BuildOptions.CompressWithLz4;
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = BuildAppSettings.BuildScenes();
                buildPlayerOptions.locationPathName = buildName;
                buildPlayerOptions.assetBundleManifestPath = EditorUtilities.mAssetsDir + PlatformUtilities.GetAssetFolderPath();
                buildPlayerOptions.options = mBuildOptions;
                buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;

                BuildReport result = BuildPipeline.BuildPlayer(buildPlayerOptions);
                UnityEngine.Debug.Log("BuildPlayer result : " + result.summary.result);
            }
            UpdateVersionListHashHistory.Update("Build " + ProductSettings.pInstance._BuildNumber, "NA", null);
        }

        public string pCurrentPlatformVersion
        {
            get
            {
                var product_info = ProductSettings.GetCurrentProductInfo();
                if (product_info != null && !product_info.Version.Equals(PlayerSettings.bundleVersion))
                    PlayerSettings.bundleVersion = ProductSettings.GetCurrentProductInfo().Version;

                return PlayerSettings.bundleVersion;
            }
            set
            {
                var product_info = ProductSettings.GetCurrentProductInfo();
                if (product_info != null)
                {
                    product_info.Version = value;
                    PlayerSettings.bundleVersion = value;
                }
            }
        }

        public string pCurrentPlatformBundleCode
        {
            get
            {
#if UNITY_ANDROID
                return PlayerSettings.Android.bundleVersionCode.ToString();
#elif UNITY_IOS
                return PlayerSettings.iOS.buildNumber;
#elif UNITY_STANDALONE_OSX
                return PlayerSettings.macOS.buildNumber;
#elif UNITY_STANDALONE_WIN
                return "1";
#elif UNITY_WEBGL
                return "1";
#else
                EditorUtility.DisplayDialog ("Bundle Code Platform Unknown", "Unknown Platform", "Ok");
	        	return string.Empty;
#endif
            }

            set
            {
#if UNITY_ANDROID
                PlayerSettings.Android.bundleVersionCode = int.Parse(value);
#elif UNITY_IOS
                PlayerSettings.iOS.buildNumber = value;
#elif UNITY_STANDALONE_OSX
                PlayerSettings.macOS.buildNumber = value;
#endif
            }
        }

        [PostProcessBuild(1000)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if UNITY_ANDROID
            Directory.CreateDirectory(pathToBuiltProject + "/" + Application.productName + "/gradle/wrapper");
            string filePath = Application.dataPath + $"/Plugins/Android/gradle/wrapper/{BuildAppSettings.pInstance.GradleWrapperFileName}";
            if (File.Exists(filePath))
                File.Copy(filePath, pathToBuiltProject + "/" + Application.productName + $"/gradle/wrapper/{BuildAppSettings.pInstance.GradleWrapperFileName}", true);
            else
                Debug.LogWarning($"Could not find jar file {filePath}");

            filePath = Application.dataPath + "/Plugins/Android/gradle/wrapper/gradle-wrapper.properties";
            if (File.Exists(filePath))
                File.Copy(filePath, pathToBuiltProject + "/" + Application.productName + "/gradle/wrapper/gradle-wrapper.properties", true);
            else
                Debug.LogWarning($"Could not find properties file {filePath}");
#endif
        }

    }
}