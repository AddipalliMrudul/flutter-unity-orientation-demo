using UnityEngine;

namespace XcelerateGames.Keyboard
{
    using UnityEditor;

    [CustomEditor(typeof(XKeyboardMap))]
    public class XKeyboardAssetEditor : Editor
    {
        #region Data
        //Private
        private XKeyboardMap instance;
        #endregion//============================================================[ Data ]

        #region Unity
        public override void OnInspectorGUI()
        {
            instance = (XKeyboardMap)target;
            RenderEditor();
        }
        #endregion//============================================================[ Unity ]

        #region Public
        public void RenderEditor()
        {
            XKeyboardEditor.EditorHeader("Config");
            instance.languageCode = EditorGUILayout.TextField("Language Code", instance.languageCode);
            XKeyboardEditor.EditorHeader("References");            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyMap"), new GUIContent("key Map"));
            EditorGUILayout.Space();
            if (GUI.changed)
            {                
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(instance);
            }
        }
        #endregion//============================================================[ Public ]
    }
}
