using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Editor.Inspectors;

namespace XcelerateGames.Editor.UI
{
    public class CreateTemplateBase : EditorWindow
    {
        protected const string TemplatesFolder = "Assets/Framework/ScriptTemplates/";
        public ReadOnlyCollection<string> DefaultNamespaces { get; } = new ReadOnlyCollection<string>(new string[] { "None", "XcelerateGames" });

        protected string mTemplateName = "";
        protected string mClassName = "";
        protected List<string> mNamespaces = null;
        protected string mEnteredNamespace = null;
        protected int mSelectedNameSpace = 0;
        protected const string NamespaceKey = "PreviousNamespace1";

        private void OnEnable()
        {
            SetDefaultValues();
        }

        protected virtual void SetDefaultValues()
        {
            if(mNamespaces == null)
                mNamespaces = new List<string>(DefaultNamespaces);
        }

        protected virtual void OnGUI()
        {
            DrawClassName();
            EditorGUILayout.Space();

            DrawNameSpace();
            EditorGUILayout.Space();

            DrawCreateButton();
        }

        protected virtual void DrawCreateButton()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(mClassName) || (mSelectedNameSpace == 0 && mEnteredNamespace.IsNullOrEmpty()));
            if (GUILayout.Button("Create"))
            {
                string path = Path.GetDirectoryName(mClassName);
                string clasName = Path.GetFileNameWithoutExtension(mClassName);

                path = CreateDirectories(path);
                if (mSelectedNameSpace > 0)
                    mEnteredNamespace = mNamespaces[mSelectedNameSpace];
                CreateClass(path, clasName, mEnteredNamespace);
                Close();
            }
            EditorGUI.EndDisabledGroup();
        }

        protected string CreateDirectories(string path)
        {
            path = GetSelectedDir() + path + "/";
            FileUtilities.CreateDirectoryRecursively(path);
            return path;
        }

        protected virtual void DrawClassName()
        {
            mClassName = EditorGUITools.DrawTextField("Class Name", mClassName);
        }

        protected virtual void DrawNameSpace()
        {
            mSelectedNameSpace = EditorTools.DrawList("Select Namespace", mNamespaces.ToArray(), mNamespaces[mSelectedNameSpace], null);
            if (mNamespaces[mSelectedNameSpace].Equals("None"))
                mEnteredNamespace = EditorGUITools.DrawTextField("Enter New Namespace", mEnteredNamespace);
            else
                mEnteredNamespace = null;
        }

        protected virtual void CreateClass(string path, string className, string nameSpace)
        {
            string filePath = path + className + ".cs";
            if (File.Exists(filePath) == false)
            {
                string[] data = File.ReadAllLines(TemplatesFolder + mTemplateName);
                for (int i = 0; i < data.Length; ++i)
                {
                    data[i] = data[i].Replace("{NAMESPACE}", nameSpace);
                    data[i] = data[i].Replace("{CLASS_NAME}", className);
                }
                File.WriteAllLines(filePath, data);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        protected string GetSelectedDir()
        {
            string dstPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string fileExt = Path.GetExtension(dstPath);
            if (!string.IsNullOrEmpty(fileExt))
                dstPath = Path.GetDirectoryName(dstPath);
            return dstPath + "/";
        }

        protected virtual void Awake()
        {
            if (XGEditorPrefs.HasKey(NamespaceKey))
                mNamespaces = new List<string>(XGEditorPrefs.GetString(NamespaceKey).Split(','));
            else
                SetDefaultValues();
        }

        private void OnDestroy()
        {
            if (!mEnteredNamespace.IsNullOrEmpty())
                mNamespaces.Add(mEnteredNamespace);
            XGEditorPrefs.SetString(NamespaceKey, mNamespaces.Printable(','));
        }
    }
}