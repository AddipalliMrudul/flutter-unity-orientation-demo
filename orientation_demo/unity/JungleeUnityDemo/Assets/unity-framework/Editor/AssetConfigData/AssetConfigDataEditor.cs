using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// Custom inspector for AssetConfigData
    /// </summary>
    [InitializeOnLoad]
    [CustomEditor(typeof(AssetConfigData))]
    public class AssetConfigDataEditor : UnityEditor.Editor
    {
        private AssetConfigData mInstance = null;      /**<Instance of AssetConfigData */

        private string mSearchItem = null;
        private float mSearchItemControlId;
        private float mSelectedField;

        /// <summary>
        /// Draw the inspector UI
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (GUIUtility.hotControl != 0)
                mSelectedField = GUIUtility.hotControl - 1;

            mSearchItemControlId = GUIUtility.GetControlID(new GUIContent("SearchItem"), FocusType.Passive);
            mSearchItem = EditorGUILayout.TextField("Search_Item", mSearchItem);
            base.OnInspectorGUI();

            if (mSearchItemControlId == mSelectedField)
            {
                mInstance.Searching(mSearchItem);
            }
            if (GUILayout.Button("Populate", GUILayout.Height(40)))
            {
                Populate();
            }
            if (GUILayout.Button("Export", GUILayout.Height(40)))
            {
                ExportList();
            }
        }


        /// <summary>
        /// Cache the reference of the object
        /// </summary>
        private void OnEnable()
        {
            mInstance = (AssetConfigData)target;
            mInstance.BackUp();

            if (Selection.activeObject != null)
                AssetConfigData.Init(Selection.activeObject.name);
        }

        /// <summary>
        /// Do necessary operations on disable
        /// </summary>
        private void OnDisable()
        {
        }

        private void Populate()
        {
            mInstance.pConfigs = new List<AssetConfig>();
            AssetBundleManifest assetBundleManifest = EditorUtilities.LoadManifest();
            string[] assetBundles = assetBundleManifest.GetAllAssetBundles();
            foreach (string assetBundle in assetBundles)
            {
                mInstance.pConfigs.Add(new AssetConfig() { AssetName = assetBundle, Name = assetBundle });
            }
            Debug.Log("Num of asset bundles added to mapper: " + mInstance.pConfigs.Count);
            mInstance.PopulateList();
            //AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            //importer.SaveAndReimport();
            //AssetDatabase.Refresh();
            //EditorUtility.SetDirty(mInstance);
        }

        /// <summary>
        /// Export the list
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ExportList()
        {
            Populate();
            System.IO.File.WriteAllText("AssetMappings.txt", mInstance.pConfigs.Printable(converter: PrintConverter));
        }

        private string PrintConverter(AssetConfig item)
        {
            return item.ToString();
        }
    }
}
