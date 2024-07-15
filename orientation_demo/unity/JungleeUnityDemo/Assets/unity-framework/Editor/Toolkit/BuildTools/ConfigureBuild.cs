using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Editor.Inspectors;

namespace XcelerateGames.Editor.Build
{
    public class ConfigureBuild : ScriptableWizard
    {
        private const string OTA_BUILD = "OTA_BUILD";
        private const string DBG_CFG = "DBG_CFG";
        private const string AUTOMATION_ENABLED = "AUTOMATION_ENABLED";
        private const string SIMULATE_MOBILE = "SIMULATE_MOBILE";
        private const string UNITY_FACEBOOK = "UNITY_FACEBOOK";

        protected int mSelectedBuildServerType = 0;
        private bool mIsLocalBuild = true;
        private bool mIsDebugConfig = false;
        private bool mIsAutomationEnabled = true;

        private bool mSimulateMobile = false;
        private bool mFBArcadeBuild = false;

        private string[] mBuildServerTypes;

        [MenuItem(Utilities.MenuName + "Configure Build")]
        public static void OpenConfigureWindow()
        {
            OpenConfigureWindow(false);
        }

        public static void OpenConfigureWindow(bool CloseWindowOnUpdate)
        {
            ConfigureBuild cb = EditorWindow.GetWindow<ConfigureBuild>(true, "Configure Build", true);
            cb.GetCurrentSettings();
        }

        protected virtual void OnEnable()
        {
            GetCurrentSettings();
        }

        protected virtual void GetCurrentSettings()
        {
            mBuildServerTypes = PlatformUtilities.GetAllEnvironementType();
            mSelectedBuildServerType = 0;
            ScriptingDefinedSymbols.Read();
            foreach (string serverType in mBuildServerTypes)
            {
                if (ScriptingDefinedSymbols.Contains(serverType + "_BUILD"))
                    break;
                else
                    mSelectedBuildServerType++;
            }

            mSimulateMobile = ScriptingDefinedSymbols.Contains(SIMULATE_MOBILE);
            mFBArcadeBuild = ScriptingDefinedSymbols.Contains(UNITY_FACEBOOK);

            mIsLocalBuild = !ScriptingDefinedSymbols.Contains(OTA_BUILD);
            mIsDebugConfig = ScriptingDefinedSymbols.Contains(DBG_CFG);
            mIsAutomationEnabled = ScriptingDefinedSymbols.Contains(AUTOMATION_ENABLED);
        }

        protected virtual void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Server Type : ");
            mSelectedBuildServerType = EditorGUILayout.Popup(mSelectedBuildServerType, mBuildServerTypes);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            mIsLocalBuild = EditorGUITools.DrawToggle("Is Local Build? : ", mIsLocalBuild);
            EditorGUILayout.Space();

            mIsDebugConfig = EditorGUITools.DrawToggle("Is Debug Config : ", mIsDebugConfig);
            EditorGUILayout.Space();

            mIsAutomationEnabled = EditorGUITools.DrawToggle("Is Automation Enabled? : ", mIsAutomationEnabled);
            EditorGUILayout.Space();

            mSimulateMobile = EditorGUITools.DrawToggle("Simulate Mobile : ", mSimulateMobile);
            EditorGUILayout.Space();

            mFBArcadeBuild = EditorGUITools.DrawToggle("Unity Facebook :", mFBArcadeBuild);
            EditorGUILayout.Space();

            DrawUpdateButton();

            EndDisableGroup();
        }

        protected virtual void EndDisableGroup()
        {
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isCompiling)
            {
                GUIStyle s = new GUIStyle(EditorStyles.textField);
                s.normal.textColor = Color.red;
                GUILayout.Label("DO NOT CLOSE THIS WINDOW till compilation is complete", s);
            }
        }

        protected virtual void DrawUpdateButton()
        {
            if (GUILayout.Button("Update"))
            {
                OnUpdateClicked();
                SetScriptingDefineSymbol();
            }
        }

        protected virtual void OnUpdateClicked()
        {
            //Remove all server types.
            foreach (string serverType in mBuildServerTypes)
                ScriptingDefinedSymbols.Remove(serverType + "_BUILD");

            //Remove all build types.
            ScriptingDefinedSymbols.Remove(OTA_BUILD);

            //Now add the selected server type.
            ScriptingDefinedSymbols.Add(mBuildServerTypes[mSelectedBuildServerType] + "_BUILD");
            if (!mIsLocalBuild)
                ScriptingDefinedSymbols.Add(OTA_BUILD);

            ScriptingDefinedSymbols.AddOrRemove(DBG_CFG, mIsDebugConfig);

            ScriptingDefinedSymbols.AddOrRemove(SIMULATE_MOBILE, mSimulateMobile);

            ScriptingDefinedSymbols.AddOrRemove(UNITY_FACEBOOK, mFBArcadeBuild);

            ScriptingDefinedSymbols.AddOrRemove(AUTOMATION_ENABLED, mIsAutomationEnabled);
        }

        /// <summary>
        /// Set all changed Scripting Define symbol to project setting
        /// </summary>
        protected virtual void SetScriptingDefineSymbol()
        {
            Debug.Log($">>>>>>Symbols : {ScriptingDefinedSymbols.Symbols.Printable(';')}");
            ScriptingDefinedSymbols.Commit();
        }

        /// <summary>
        /// Sets compiler flags for server type, CDN or local and if automation is enabled or not via
        /// command line
        /// </summary>
        public static void SetEnvVariables()
        {
            EditorUtilities.ClearEnvFlags();
            ScriptingDefinedSymbols.Read();
            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-servertype")
                {
                    Debug.Log(">>>argument for -servertype is: " + args[i + 1]);
                    ScriptingDefinedSymbols.Add(args[i + 1] + "_BUILD");
                }
                else if (args[i] == "-islocalbuild")
                {
                    Debug.Log(">>>argument for -islocalbuild is: " + args[i + 1]);
                    ScriptingDefinedSymbols.Remove(OTA_BUILD);
                    if (args[i + 1] == "false")
                        ScriptingDefinedSymbols.Add(OTA_BUILD);
                }
                else if (args[i] == "-isAutomationEnabled")
                {
                    Debug.Log(">>>argument for -isAutomationEnabled is: " + args[i + 1]);
                    ScriptingDefinedSymbols.Remove(AUTOMATION_ENABLED);
                    if (args[i + 1] == "true")
                        ScriptingDefinedSymbols.Add(AUTOMATION_ENABLED);
                }
                else if (args[i] == "-standalonegametype")
                {
                    Debug.Log(">>>argument for -standalonegametype is: " + args[i + 1]);

                    ScriptingDefinedSymbols.Remove("NONE");
                    ScriptingDefinedSymbols.Remove("POKER_STANDALONE");
                    ScriptingDefinedSymbols.Remove("SOLITAIRE_GOLD_STANDALONE");
                    ScriptingDefinedSymbols.Remove("LUDO_STANDALONE");
                    ScriptingDefinedSymbols.Remove("CARROM_STANDALONE");

                    ScriptingDefinedSymbols.Add(args[i + 1]);
                }
            }
            ScriptingDefinedSymbols.Commit();
        }

        /// <summary>
        /// Writes hash of asset bundle into a text file which
        /// we are further sending to all developers as a post build action
        /// when build is triggered via jenkins
        /// </summary>
        public static void WriteHashOfAssetBundleToTextFile()
        {
            string assetBundleHash = CreateAssetVersionList.GetVersionListHash();

            //path of file
            string path = Application.dataPath + "/AssetBundleHashFile.txt";

            //create file and add hash code, if file already exists then it is overriden
            File.WriteAllText(path, assetBundleHash);
        }
    }
}