using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    [InitializeOnLoad]
    public class InitFramework : EditorWindow
    {
        #region Member Variables
        private const string Key = "FrameworkInited";
        private const string GameDir = "Assets/Game/";
        private const string ResourcesDir = "Assets/Resources/";

        #endregion Member Variables

        static InitFramework()
        {
            //bool inited = XGEditorPrefs.GetBool(Key, false);
            //if(!inited)
            //{
            //    Debug.Log("Framework not inited yet. Prompting user");
            //    bool startInit = EditorUtility.DisplayDialog("Warning!", "Framework not initialized yet, Would you like to do it now?", "Yes", "No");
            //    if(startInit)
            //    {
            //        Debug.Log("Starting init process");
            //        OpenFrameworkInitWindow();
            //    }
            //    else
            //        Debug.Log("Skipping framework init process");
            //}
        }

        //[MenuItem(Utilities.MenuName + "Init Framework")]
        public static void OpenFrameworkInitWindow()
        {
            CreateWindow<InitFramework>().Init();
        }

        void Init()
        {
            titleContent.text = "Initialize Framework";
        }

        private void OnDisable()
        {
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if(GUILayout.Button("Start"))
            {
                CreateDirStructure();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateDirStructure()
        {
            FileUtilities.CreateDirectoryRecursively(GameDir + "Scripts");
            string gameResources = GameDir + "Resources";
            FileUtilities.CreateDirectoryRecursively(gameResources);
            AssetImporter importer = AssetImporter.GetAtPath(gameResources);
            importer.assetBundleName = "resources";
            importer.SaveAndReimport();
            AssetDatabase.Refresh();

            FileUtilities.CreateDirectoryRecursively(ResourcesDir);

            AssetDatabase.ImportAsset(GameDir, ImportAssetOptions.ImportRecursive);
        }

        private void OnInitComplete()
        {
        }
    }
}
