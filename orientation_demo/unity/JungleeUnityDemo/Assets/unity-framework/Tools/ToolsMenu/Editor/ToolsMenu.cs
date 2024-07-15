
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace XcelerateGames.EditorTools
{
    public static class ToolsMenu
    {
        #region Menu Item
        [MenuItem("XcelerateGames/EditorTools/Docs/Attributes", false)]
        public static void MenuOpenAttributesDoc()
        {
            Application.OpenURL("https://github.com/dbrizov/NaughtyAttributes/blob/master/README.md");
        }
        [MenuItem("XcelerateGames/EditorTools/Docs/EditorIcons", false)]
        public static void MenuOpenEditorIconsDoc()
        {
            Application.OpenURL("https://github.com/halak/unity-editor-icons/blob/master/README.md");
        }
        [MenuItem("XcelerateGames/EditorTools/Data/Application", false)]
        public static void MenuDataApplication()
        {
            EditorUtility.RevealInFinder(Application.dataPath);
        }
        [MenuItem("XcelerateGames/EditorTools/Data/Cache", false)]
        public static void MenuDataCache()
        {
            EditorUtility.RevealInFinder(Application.temporaryCachePath);
        }
        [MenuItem("XcelerateGames/EditorTools/Data/Console", false)]
        public static void MenuDataConsole()
        {
            EditorUtility.RevealInFinder(Application.consoleLogPath);
        }
        [MenuItem("XcelerateGames/EditorTools/Data/Persistent", false)]
        public static void MenuDataPersistent()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
        [MenuItem("XcelerateGames/EditorTools/Data/Streaming", false)]
        public static void MenuDataStreaming()
        {
            EditorUtility.RevealInFinder(Application.streamingAssetsPath);
        }
        [MenuItem("XcelerateGames/EditorTools/Object/Hiearchy", false)]
        public static void MenuObjectHiearchy()
        {
            ObjectHiearchy();
        }
        [MenuItem("XcelerateGames/EditorTools/Object/Toggle %&#_d", false)]
        public static void MenuObjectToggle()
        {
            ObjectToggle();
        }
        #endregion//============================================================[ Menu Item ]   

        #region Private
        private static void ObjectHiearchy()
        {
            var path = string.Empty;
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 1) path = GetGameObjectPath(selectedObjects[0]);
            Debug.Log("[ Hiearchy ] : " + path);
            EditorGUIUtility.systemCopyBuffer = path;
        }
        private static string GetGameObjectPath(GameObject obj)
        {
            var path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
        private static void ObjectToggle()
        {
            var selectedObjects = Selection.gameObjects;
            foreach (var gameObject in selectedObjects)
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
                else
                    gameObject.SetActive(true);
                EditorUtility.SetDirty(gameObject);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
        #endregion//============================================================[ Private ]
    }
}