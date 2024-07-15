using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Editor.Inspectors;

namespace XcelerateGames.Editor.UI
{
    public class CreateTextFile : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/Text File")]
        static void DoSetAssetBundleName()
        {
            GetWindow<CreateTextFile>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "TextFileTemplate.txt";
        }

        protected override void OnGUI()
        {
            DrawFileName();
            EditorGUILayout.Space();
            DrawCreateButton();
        }

        void DrawFileName()
        {
            mClassName = EditorGUITools.DrawTextField("File Name with extension", mClassName);
        }

        protected override void DrawCreateButton()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(mClassName));
            if (GUILayout.Button("Create"))
            {
                string path = Path.GetDirectoryName(mClassName);

                path = CreateDirectories(path);
                CreateFile(path, mClassName);
                Close();
            }
            EditorGUI.EndDisabledGroup();
        }

        void CreateFile(string path, string className)
        {
            string filePath = path + className;
            if (File.Exists(filePath) == false)
            {
                File.Create(filePath);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}