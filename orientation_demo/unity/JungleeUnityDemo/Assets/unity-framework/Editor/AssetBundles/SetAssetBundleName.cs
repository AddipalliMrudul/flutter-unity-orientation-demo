using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.AssetBundles
{
    /// <summary>
    /// Helper script to set AssetBundle name of all selected objects in project window. Shows the list of all available bundle names.
    /// </summary>
    public class SetAssetBundleName : EditorWindow
    {
        /// <summary>
        /// Asset bundle name to set
        /// </summary>
        public string _BundleName = "";

        /// <summary>
        /// List of all asset bundles in the game
        /// </summary>
        private List<string> mBundleNames = new List<string>();
        private Vector2 mScroll = Vector2.zero;

        /// <summary>
        /// Launch editor window to set bundle name
        /// </summary>
        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Set Name", false, 36)]
        private static void DoSetAssetBundleName()
        {
            EditorWindow.GetWindow<SetAssetBundleName>().titleContent.text = "Set Bundle Name";
        }

        /// <summary>
        /// Validate the menu option. Menu option will be enabled only if one or mor objects are selected in Project window
        /// </summary>
        /// <returns></returns>
        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Set Name", true, 36)]
        private static bool DoSetAssetBundleNameValidate()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
                return false;

            foreach (Object obj in Selection.objects)
            {
                if (obj == null)
                    continue;
                if (obj.GetType().ToString().EndsWith(".MonoScript"))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// List all asset bundle names being used in the game
        /// </summary>
        private void OnEnable()
        {
            string[] assetBundles = AssetDatabase.GetAllAssetBundleNames();
            foreach(string assetBundle in assetBundles)
            {
                mBundleNames.AddItemOnce(System.IO.Path.GetFileNameWithoutExtension(assetBundle));
            }
        }

        /// <summary>
        /// Draw UI
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bundle Name : ");
            _BundleName = EditorGUILayout.TextField(_BundleName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GUIColor.Push(Color.yellow);
            if (GUILayout.Button("Set"))
                SetName(_BundleName);
            GUIColor.Pop();

            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach (string bundleName in mBundleNames)
            {
                if (bundleName.Contains(_BundleName))
                {
                    if (GUILayout.Button(bundleName))
                        SetName(bundleName);
                }
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Set the selected asset bundle name to all selected objects in project window
        /// </summary>
        /// <param name="bundleName"></param>
        private void SetName(string bundleName)
        {
            foreach (Object obj in Selection.objects)
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(obj);
            }
        }
    }
}