using UnityEngine;

namespace XcelerateGames.Keyboard
{
    using UnityEditor;

    [CustomEditor(typeof(XKeyboard))]
    public class XKeyboardEditor : Editor
    {
        #region Data
        //Private
        private XKeyboard instance;
        #endregion//============================================================[ Data ]

        #region Unity
        public override void OnInspectorGUI()
        {
            instance = (XKeyboard)target;
            RenderEditor();
        }
        #endregion//============================================================[ Unity ]

        #region Public
        public void RenderEditor()
        {
            EditorHeader("Data");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Key Board Visible", instance.isKeyboardVisible.ToString());
            }
            EditorHeader("Config");            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyboardMap"), new GUIContent("keyboard Map"));
            instance.keyboardOrientation = (XKeyboard.KeyboardOrientation)EditorGUILayout.EnumPopup("Orientation", instance.keyboardOrientation);            
            if (instance.keyboardMap == null)
            {
                EditorGUILayout.HelpBox("Assign a Keymap", MessageType.Warning);
            }
            instance.animationTime = EditorGUILayout.FloatField("Animation Time", instance.animationTime);
            EditorHeader("References");            
            instance.panel = (RectTransform)EditorGUILayout.ObjectField("Panel RectTransform", instance.panel, typeof(RectTransform), true);
            instance.canvasGroup = (CanvasGroup)EditorGUILayout.ObjectField("Panel Canvas Group", instance.canvasGroup, typeof(CanvasGroup), true);            
            EditorHeader("Actions");
            if (!EditorApplication.isPlaying && GUILayout.Button("Show Keyboard"))
            {
                ShowKeyboard();
            }
            if (!EditorApplication.isPlaying && GUILayout.Button("Hide Keyboard"))
            {
                HideKeyboard();
            }
            EditorGUILayout.Space();
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(instance);
            }
        }
        #endregion//============================================================[ Public ]

        #region Private
        private void ShowKeyboard()
        {
            instance.canvasGroup.alpha = 1;
            instance.panel.anchoredPosition = Vector2.zero;
            instance.gameObject.SetActive(true);            
        }
        private void HideKeyboard()
        {
            instance.canvasGroup.alpha = 0;
            instance.panel.anchoredPosition = new Vector2(0, -instance.panel.rect.height);
            instance.gameObject.SetActive(true);
            instance.panel.offsetMin = new Vector2(0, instance.panel.offsetMin.y);
            instance.panel.offsetMax = new Vector2(0, instance.panel.offsetMax.y);
        }
        #endregion//============================================================[ Private ]

        #region Utils
        public static void EditorHeader(string header)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[ " + header + " ]", EditorStyles.boldLabel);
            EditorGUILayout.Space(-100);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        #endregion//============================================================[ Utils ]   
    }
}