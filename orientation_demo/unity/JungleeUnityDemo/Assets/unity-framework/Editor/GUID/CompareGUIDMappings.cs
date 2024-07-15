using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class CompareGUIDMappings : GenerateGUIDMapping
    {
        TextAsset mOtherMappings = null;
        Dictionary<string, UnityEngine.Object> mCommonGUIDs = new Dictionary<string, UnityEngine.Object>();
        private Vector2 mScroll = Vector2.zero;
        private List<string> mFoldersToIgnore = new List<string>() { "/Framework/", "/TestingAutomationFramework/", "/Editor Default Resources/", "/TextMesh Pro/" };

        [MenuItem(Utilities.MenuName + mGroupName + "Compare GUID Mappings")]
        static void DoGenerateGUIDMapping()
        {
            CompareGUIDMappings window = GetWindow<CompareGUIDMappings>();
            window.titleContent.text = "Compare GUID Mappings";
        }

        /// <summary>
        /// Draw the UI
        /// </summary>
        private void OnGUI()
        {
            mOtherMappings = (TextAsset)EditorGUILayout.ObjectField("Other Mappings", mOtherMappings, typeof(TextAsset), false);

            EditorGUI.BeginDisabledGroup(mOtherMappings == null);
            if (GUILayout.Button("Compare"))
            {
                Compare();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField($"Total Conflicts found: {mCommonGUIDs.Count}");

            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach(KeyValuePair<string, UnityEngine.Object> keyValuePair in mCommonGUIDs)
            {
                if(GUILayout.Button(AssetDatabase.GetAssetPath(keyValuePair.Value)))
                    EditorGUIUtility.PingObject(keyValuePair.Value);
            }
            GUILayout.EndScrollView();
        }

        private void Compare()
        {
            //Enable all options as we dont know what the other mapping file was generated with. May be they had enabled all.
            mAssets = mPackages = mProjectSettings = true;
            //Get mappings for the current project
            Dictionary<string, string> mappings = GenerateMapping();
            Dictionary<string, string> otherMappings = mOtherMappings.text.FromJson<Dictionary<string,string>>();
            foreach(KeyValuePair<string,string> keyValuePair in mappings)
            {
                if(mFoldersToIgnore.Find(e => keyValuePair.Value.Contains(e)) != null)
                    continue;
                //Check if there is a conflict in GUID
                if(otherMappings.ContainsKey(keyValuePair.Key))
                {
                    mCommonGUIDs[keyValuePair.Key] = AssetDatabase.LoadMainAssetAtPath(keyValuePair.Value);
                }
            }
        }
    }
}