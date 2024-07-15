using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class UiMenuOptions
    {
        [MenuItem("GameObject/UI/Custom/TextMesh Pro", false, 100)]
        public static void CreateTextMeshText()
        {
            CreateFromTemplate("Assets/Framework/Prefabs/UiTemplates/TextTemplateTextMesh.prefab", "Text");
        }

        [MenuItem("GameObject/UI/Custom/TextMesh Pro Locale", false, 100)]
        public static void CreateTextMeshTextWithLocale()
        {
            CreateFromTemplate("Assets/Framework/Prefabs/UiTemplates/TextTemplateTextMeshLocale.prefab", "Text");
        }

        [MenuItem("GameObject/UI/Custom/Button With Text", false, 100)]
        public static void CreateButtonWithText()
        {
            CreateFromTemplate("Assets/Framework/Prefabs/UiTemplates/ButtonTemplateWithText.prefab", "Button");
        }

        [MenuItem("GameObject/UI/Custom/Toggle", false, 100)]
        public static void CreateToggle()
        {
            CreateFromTemplate("Assets/Framework/Prefabs/UiTemplates/ToggleTemplate.prefab", "Toggle");
        }

        [MenuItem("GameObject/UI/Custom/Input Box", false, 100)]
        public static void CreateInputBox()
        {
            CreateFromTemplate("Assets/Framework/Prefabs/UiTemplates/InputTemplate.prefab", "Input");
        }

        [MenuItem("GameObject/UI/Custom/DropDown Custom", false, 100)]
        public static void CreateCustomDropDown()
        {
            CreateFromTemplate("Assets/Framework/Prefabs/UiTemplates/DropDownTemplate.prefab", "DropDown");
        }

        private static GameObject CreateFromTemplate(string templatePath, string templateName)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath(templatePath, typeof(GameObject)) as GameObject;
            if (go != null)
            {
                go = GameObject.Instantiate<GameObject>(go);
                go.name = templateName;
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                Debug.Log($"Created {templateName} from template : {templatePath}");
                return go;
            }
            else
            {
                Debug.LogError($"Failed to find asset: {templatePath}");
                return null;
            }
        }
    }
}
