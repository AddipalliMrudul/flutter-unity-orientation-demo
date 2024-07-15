using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

namespace XcelerateGames.Keyboard
{
    using UnityEditor;

    [CustomEditor(typeof(XKeyboardKey))]
    public class XKeyboardKeyEditor : Editor
    {
        #region Data
        //Private
        private XKeyboardKey instance;
        private List<string> keyMapItems = new List<string>();        
        #endregion//============================================================[ Data ]

        #region Unity
        public override void OnInspectorGUI()
        {
            instance = (XKeyboardKey)target;
            if (XKeyboard.instace == null)
            {
                XKeyboard.instace = instance.gameObject.GetComponentInParent<XKeyboard>();
            }            
            RenderEditor();
        }
        #endregion//============================================================[ Unity ]

        #region Public
        public void RenderEditor()
        {
            XKeyboardEditor.EditorHeader("Data");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Key Label", instance.keyMap.keyCharacter);                
            }            
            XKeyboardEditor.EditorHeader("Config");
            GetKeysList();            
            instance.capsLock = EditorGUILayout.Toggle("Upper Case", instance.capsLock);
            instance._ProcessEventOnHoldDown = (bool)EditorGUILayout.Toggle("ProcessEventOnHoldDown", instance._ProcessEventOnHoldDown);
            XKeyboardEditor.EditorHeader("References");
            instance.keyButton = (Button)EditorGUILayout.ObjectField("Button", instance.keyButton, typeof(Button), true);
            if (instance.keyObject == null)
            {
                instance.keyText = (TMP_Text)EditorGUILayout.ObjectField("Text", instance.keyText, typeof(TMP_Text), true);
            }                
            if(instance.keyText == null)
            {
                instance.keyObject = (GameObject)EditorGUILayout.ObjectField("Object", instance.keyObject, typeof(GameObject), true);
            }
            instance.UpdateKey();           
            EditorGUILayout.Space();
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(instance);
            }            
        }
        #endregion//============================================================[ Public ]

        #region Private        
        private void GetKeysList() {            
            if (XKeyboard.instace != null)
            {
                if (XKeyboard.instace.keyboardMap != null)
                {
                    keyMapItems = new List<string>();
                    foreach (var keyMap in XKeyboard.instace.keyboardMap.keyMap)
                    {
                        keyMapItems.Add(keyMap.keyCode.ToString());
                    }                    
                    instance.keyMapIndex = EditorGUILayout.Popup("Key Code", instance.keyMapIndex, keyMapItems.ToArray());
                    instance.keyMap = XKeyboard.instace.keyboardMap.keyMap[instance.keyMapIndex];
                    if (instance.gameObject.scene.IsValid())
                    {
                        instance.gameObject.name = "Key - " + instance.keyMap.keyCode.ToString();
                    }                    
                }
                else
                {                    
                    EditorGUILayout.HelpBox("Assign a Keymap to the Keyboard", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Make it a child of Keyboard for more Options", MessageType.Warning);
            }
        }
        #endregion//============================================================[ Private ]
    }
}