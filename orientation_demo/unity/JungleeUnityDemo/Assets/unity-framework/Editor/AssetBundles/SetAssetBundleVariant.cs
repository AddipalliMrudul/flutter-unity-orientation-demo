using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.AssetBundles
{
    /// <summary>
    /// Set variant name for the asset bundle
    /// </summary>
    public class SetAssetBundleVariant : EditorWindow
    {
        /// <summary>
        /// Variant name to be set
        /// </summary>
        private string mVariant = "";

        private List<string> mVariants = new List<string>();
        private Vector2 mScroll = Vector2.zero;

        /// <summary>
        /// Launch the editor window to set variant name
        /// </summary>
        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Set Variant", false, 37)]
        private static void DoSetAssetBundleVariant()
        {
            EditorWindow.GetWindow<SetAssetBundleVariant>().titleContent.text = "Set Bundle Variant";
        }

        /// <summary>
        /// Validate the menu option. Menu option will be enabled only if one or mor objects are selected in Project window
        /// </summary>
        /// <returns></returns>
        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Set Variant", true, 37)]
        private static bool DoSetAssetBundleVariantValidate()
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
        /// List all variants currently being used in the game.
        /// @note If the variant is not listed, try building asset bundles
        /// </summary>
        private void OnEnable()
        {
            AssetBundleManifest assetBundleManifest = EditorUtilities.LoadManifest();
            if (assetBundleManifest != null)
            {
                string[] variants = assetBundleManifest.GetAllAssetBundlesWithVariant();
                foreach(string variant in variants)
                {
                    mVariants.AddItemOnce(FileUtilities.GetExtension(variant));
                }
            }
        }

        /// <summary>
        /// Draw UI & Controls
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Variant Name : ");
            mVariant = EditorGUILayout.TextField(mVariant);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GUIColor.Push(Color.yellow);
            if (GUILayout.Button("Set"))
                SetVariant(mVariant);
            GUIColor.Pop();

            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach (string variant in mVariants)
            {
                if (variant.Contains(mVariant))
                {
                    if (GUILayout.Button(variant))
                        SetVariant(variant);
                }
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Set the variant name for all the selected objects
        /// @note Variant can be set only if the asset bundle name is set.
        /// </summary>
        /// <param name="variant">variant name</param>
        private void SetVariant(string variant)
        {
            foreach (Object obj in Selection.objects)
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
                importer.assetBundleVariant = variant;
                importer.SaveAndReimport();
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(obj);
            }
        }
    }
}