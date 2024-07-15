using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// Class to generate a mapping of GUID to asset path. GUID is the key & asset path is the value
    /// </summary>
    public class GenerateGUIDMapping : EditorWindow
    {
        protected bool mAssets = true;
        protected bool mPackages = false;
        protected bool mProjectSettings = false;
        protected bool mWebSocketSharp = false;

        protected string mFileName = "mappings.json";         /**< Filename to save to*/

        protected const string mGroupName = "GUID/";

        /// <summary>
        /// Generate mapping of GUID to asset path.
        /// </summary>
        [MenuItem(Utilities.MenuName + mGroupName + "Generate GUID Mapping")]
        static void DoGenerateGUIDMapping()
        {
            GenerateGUIDMapping window = GetWindow<GenerateGUIDMapping>();
            window.titleContent.text = "Generate GUID Mapping";
        }

        /// <summary>
        /// Draw the UI
        /// </summary>
        private void OnGUI()
        {
            mAssets = EditorGUILayout.Toggle("Assets", mAssets);
            mPackages = EditorGUILayout.Toggle("Packages", mPackages);
            mProjectSettings = EditorGUILayout.Toggle("Project Settings", mProjectSettings);
            mWebSocketSharp = EditorGUILayout.Toggle("websocket-sharp", mWebSocketSharp);
            mFileName = EditorGUILayout.TextField("Enter File Name: ", mFileName);

            EditorGUI.BeginDisabledGroup(mFileName.IsNullOrEmpty());
            if (GUILayout.Button("Generate"))
            {
                Dictionary<string, string> mappings = GenerateMapping();
                FileUtilities.WriteToFile(mFileName, mappings.ToJson());
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Generate mappings. Key being the GUID & value being the asset path
        /// </summary>
        protected Dictionary<string, string> GenerateMapping()
        {
            string[] assets = AssetDatabase.GetAllAssetPaths();
            Dictionary<string, string> mappings = new Dictionary<string, string>();
            foreach (string assetPath in assets)
            {
                if (!CanAdd(assetPath))
                    continue;
                GUID guid = AssetDatabase.GUIDFromAssetPath(assetPath);
                mappings.Add(guid.ToString(), assetPath);
            }

            Debug.Log($"Total assets: {assets.Length}, Total added to file: {mappings.Count}");
            return mappings;
        }

        /// <summary>
        /// Returns true if the asset can be added to the list after filtering by settings
        /// </summary>
        /// <param name="assetPath">path of the asset</param>
        /// <returns>true if the asset can be added else returns false</returns>
        private bool CanAdd(string assetPath)
        {
            if (!mWebSocketSharp && assetPath.Contains("websocket-sharp"))
                return false;
            if (mAssets && assetPath.StartsWith("Assets"))
                return true;
            if (mPackages && assetPath.StartsWith("Packages"))
                return true;
            if (mProjectSettings && assetPath.StartsWith("ProjectSettings"))
                return true;
            return false;
        }
    }
}