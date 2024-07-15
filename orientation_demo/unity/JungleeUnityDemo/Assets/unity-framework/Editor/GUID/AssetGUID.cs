using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// Helper class to get GUID of selected assets & asset path from given GUID.
    /// Will be useful while merging projects
    /// </summary>
    public class AssetGUID : UnityEditor.EditorWindow
    {
        #region Get selected assets GUID
        /// <summary>
        /// Get the GUID of all selected assets & dump it to console
        /// </summary>
        [MenuItem(Utilities.MenuName + mGroupName + "Get Asset GUID")]
        static void GetAssetGUID()
        {
            string guids = null;
            foreach (Object obj in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                guids += $"{assetPath} : {AssetDatabase.GUIDFromAssetPath(assetPath)}\n";
            }
            Debug.Log($"GUIDs. Total {Selection.objects.Length} assets selected. \n {guids}");
        }

        /// <summary>
        /// Validate if any object is selected in project window. Menu is enabled only if one or more assets are selected.
        /// </summary>
        /// <returns></returns>
        [MenuItem(Utilities.MenuName + mGroupName + "Get Asset GUID", true)]
        static bool GetAssetGUIDValidate()
        {
            return (Selection.objects != null && Selection.objects.Length > 0);
        }
        #endregion Get selected assets GUID

        #region Get AssetPath by GUID
        public string _GUID = null;         /**< GUID to search*/
        public string _AssetPath = null;    /**< Path of the asset coreesponding to the GUID given. Null if GUID is not found*/
        private bool? mIsAssetFound;
        private const string mGroupName = "GUID/";

        /// <summary>
        /// Get asset path by given GUID.
        /// </summary>
        [MenuItem(Utilities.MenuName + mGroupName + "Get AssetPath By GUID")]
        static void GetAssetPathByGUID()
        {
            GetWindow<AssetGUID>();
        }

        /// <summary>
        /// Draw the UI
        /// </summary>
        private void OnGUI()
        {
            _GUID = EditorGUILayout.TextField("Enter GUID: ", _GUID);

            if (mIsAssetFound.HasValue)
            {
                if (mIsAssetFound.Value)
                    EditorGUILayout.LabelField(_AssetPath);
                else
                    EditorGUILayout.LabelField("Could not find any asset with the given GUID");
            }

            EditorGUI.BeginDisabledGroup(_GUID.IsNullOrEmpty());
            if (GUILayout.Button("Get"))
            {
                _AssetPath = AssetDatabase.GUIDToAssetPath(_GUID);
                mIsAssetFound = !_AssetPath.IsNullOrEmpty();
                if (mIsAssetFound.HasValue && mIsAssetFound.Value)
                    EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(_AssetPath));
            }
            EditorGUI.EndDisabledGroup();
        }
        #endregion Get AssetPath by GUID
    }
}