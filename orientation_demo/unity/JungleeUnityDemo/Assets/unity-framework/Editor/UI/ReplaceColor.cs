using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.Editor.Inspectors;

namespace XcelerateGames.Editor
{
    public class ReplaceColor : EditorWindow
    {
        #region SerializeField
        Color _FromImage;
        Color _ToImage;

        Color _FromText;
        Color _ToText;

        bool _Images;
        bool _TextItem;

        string _Path;
        #endregion

        #region Private functions
        [MenuItem(Utilities.MenuName + "UI/Replace Color")]
        static void CreateWizard()
        {
            ReplaceColor replaceColor =  EditorWindow.GetWindow<ReplaceColor>();
            replaceColor.titleContent.text = "Replace Color";
        }

        private void OnGUI()
        {
            EditorGUILayout.PrefixLabel("Image");
            EditorGUITools.BeginContents();

            _FromImage = EditorGUITools.DrawColor("From", _FromImage);
            _ToImage = EditorGUITools.DrawColor("To", _ToImage);
            EditorGUITools.EndContents();

            EditorGUILayout.PrefixLabel("Text");
            EditorGUITools.BeginContents();
            _FromText = EditorGUITools.DrawColor("From", _FromText);
            _ToText = EditorGUITools.DrawColor("To", _ToText);
            EditorGUITools.EndContents();

            EditorGUILayout.LabelField($"Path: {_Path}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Directory"))
            {
                _Path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!_Path.IsNullOrEmpty())
                    _Path = _Path.Replace(EditorUtilities.ProjectRoot()+"/", string.Empty);
            }
            if(GUILayout.Button("Select File"))
            {
                _Path = EditorUtility.OpenFilePanel("Select file", "Assets", "");
                if (!_Path.IsNullOrEmpty())
                    _Path = _Path.Replace(EditorUtilities.ProjectRoot() + "/", string.Empty);
            }
            GUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(_Path.IsNullOrEmpty());
            if (GUILayout.Button("Replace"))
                OnReplace();
            EditorGUI.EndDisabledGroup();

        }

        private void OnReplace()
        {
            string[] assets = AssetDatabase.FindAssets("t:prefab", new[] { _Path });
            for(int i =0;i< assets.Length;++i)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(obj);
                if(_Images)
                {
                    Image[] images = obj.GetComponentsInChildren<Image>(true);
                    foreach (Image image in images)
                    {
                        if (image.color == _FromImage)
                        {
                            Debug.Log($"Replacing color of {assetPath}: {image.name} {image.GetObjectPath()}");
                            image.color = _ToImage;
                        }
                    }
                }
                if(_TextItem)
                {
                    TextMeshProUGUI[] textItems = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (TextMeshProUGUI textItem in textItems)
                    {
                        if (textItem.color == _FromText)
                        {
                            Debug.Log($"Replacing color of {assetPath}: {textItem.name} {textItem.GetObjectPath()}");
                            textItem.color = _ToText;
                        }
                    }
                }
                if (isPrefab)
                {
                    PrefabUtility.SavePrefabAsset(obj);
                }
            }
        }
        #endregion
    }
}
