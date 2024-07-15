using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Editor.Inspectors;

namespace XcelerateGames.Editor.Build
{
    /// <summary>
    /// Editor tool to enable/disable features of framework
    /// </summary>
    public class FrameworkFeatures : EditorWindow
    {
        private const string WARNING_AS_ERROR = "warnaserror";

        private bool mWarningAsError = true;

        protected Dictionary<string, bool> mDefines = null;

        //[MenuItem(Utilities.MenuName + "Framework Features", false, 3)]
        public static void OpenFrameworkFeaturesWindow()
        {
            FrameworkFeatures instance = GetWindow<FrameworkFeatures>(true, "Framework Features", true);
            instance.GetCurrentSettings();
        }

        protected virtual void OnEnable()
        {
            CSCFileHandler.Read();

            mDefines = new Dictionary<string, bool>();
            mDefines.Add("FB_ENABLED", false);
            mDefines.Add("USE_NATIVE_WEBSOCKET", false);
            mDefines.Add("USE_WEBSOCKET_SHARP", false);
            mDefines.Add("IN_MEMORY_LOGS", false);
            mDefines.Add("CLICKSTREAM_EVENTS_FROM_UNITY", false);
            mDefines.Add("USE_IAP", false);

            GetCurrentSettings();
        }

        protected virtual void GetCurrentSettings()
        {
            mWarningAsError = CSCFileHandler.Contains(WARNING_AS_ERROR);

            List<string> keys = new List<string>(mDefines.Keys);
            foreach(string key in keys)
            {
                mDefines[key] = CSCFileHandler.Contains(key);
            }
        }

        protected virtual void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
            EditorGUILayout.Space();
            //Increase the width of label, as our labels are big
            EditorGUIUtility.labelWidth = 200;

            mWarningAsError = EditorGUITools.DrawToggle("Treat Warning as error : ", mWarningAsError);
            EditorGUILayout.Space();

            List<string> keys = new List<string>(mDefines.Keys);
            foreach (string key in keys)
            {
                mDefines[key] = EditorGUITools.DrawToggle($"{key.ToTitleCase().Replace("_", " ")}:", mDefines[key]);
                EditorGUILayout.Space();
            }
            //Reset the label width to default.
            EditorGUIUtility.labelWidth = 0;

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
            if (GUILayout.Button("Apply"))
            {
                OnUpdateClicked();
            }
        }

        protected virtual void OnUpdateClicked()
        {
            CSCFileHandler.AddOrRemove(WARNING_AS_ERROR, mWarningAsError, false);
            foreach(KeyValuePair<string, bool> keyValue in mDefines)
            {
                CSCFileHandler.AddOrRemove(keyValue.Key, keyValue.Value, true);
            }

            CSCFileHandler.Commit();
        }
    }
}