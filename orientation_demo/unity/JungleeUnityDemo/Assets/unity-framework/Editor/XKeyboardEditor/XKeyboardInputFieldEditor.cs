using TMPro;
using UnityEngine;

namespace XcelerateGames.Keyboard
{
    using UnityEditor;

    [CustomEditor(typeof(XKeyboardInputField))]
    public class XKeyboardInputFieldEditor : Editor
    {
        #region Data
        //Private
        XKeyboardInputField instance;
        #endregion//============================================================[ Data ]

        #region Unity
        public override void OnInspectorGUI()
        {
            instance = (XKeyboardInputField)target;
            RenderEditor();
        }
        #endregion//============================================================[ Unity ]

        #region Public
        public void RenderEditor()
        {
            XKeyboardEditor.EditorHeader("Config");
            instance.assetBundlePath = EditorGUILayout.TextField("Asset Bundle Name", instance.assetBundlePath);
            XKeyboardEditor.EditorHeader("References");
            instance.currentInputField = (XInputField)EditorGUILayout.ObjectField("Input Field", instance.currentInputField, typeof(XInputField), true);
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

