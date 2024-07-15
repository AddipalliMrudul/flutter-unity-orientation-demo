using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XcelerateGames.EditorTools
{
    public class ScriptsFinder : EditorWindow
    {
        #region Data
        //Private
        private static Texture2D csScriptIcon, jsScriptIcon;
        private Texture editButtonIcon, saveButtonIcon;
        private bool hasAnyScript;
        private List<string> pathOfScripts;
        private readonly Hashtable sets = new Hashtable();
        private GUIStyle styleHelpboxInner;
        private GUIStyle titleLabel, normalButtonStyle, helpButtonStyle;
        #endregion//============================================================[ Data ]

        #region Unity
        private void OnGUI()
        {
            InitStyles();
            styleHelpboxInner = new GUIStyle("box");
            styleHelpboxInner.padding = new RectOffset(6, 6, 6, 6);
            GUILayout.BeginVertical(styleHelpboxInner);
            GUILayout.Label("Scripts in current scene",
                new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            if (!hasAnyScript)
            {
                GUILayout.BeginHorizontal(styleHelpboxInner);
                GUILayout.Label("No scripts found in current scene", titleLabel);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }
            var guids = AssetDatabase.FindAssets("t:Script");
            pathOfScripts = new List<string>();
            foreach (var itemPath in guids) pathOfScripts.Add(AssetDatabase.GUIDToAssetPath(itemPath));
            foreach (Type type in sets.Keys)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var ext = "cs";
                if (!getPathOfFile(type.Name).Equals("NaN"))
                {
                    var tempArrString = getPathOfFile(type.Name).Split('.');
                    ext = tempArrString[1];
                }
                if (ext.Equals("cs"))
                    GUILayout.Box(new GUIContent(csScriptIcon, "CSharp Script"), GUILayout.MinWidth(20),
                        GUILayout.MinHeight(20));

                if (GUILayout.Button(type.Name + "." + ext, normalButtonStyle, GUILayout.MinWidth(200),
                    GUILayout.MaxWidth(1000), GUILayout.Height(20)))
                {
                    List<Object> arrayOfObjects;
                    arrayOfObjects = new List<Object>();
                    foreach (GameObject gameObject in (ArrayList)sets[type]) arrayOfObjects.Add(gameObject);
                    Selection.objects = arrayOfObjects.ToArray();
                }
                if (GUILayout.Button(new GUIContent("o", "Edit this script"), GUILayout.Width(20),
                    GUILayout.Height(20)))
                    if (!getPathOfFile(type.Name).Equals("NaN"))
                        InternalEditorUtility.OpenFileAtLineExternal(getPathOfFile(type.Name), 0);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField("©2021 Xcelerate Games",
                    new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter },
                    GUILayout.ExpandWidth(true), GUILayout.Height(15));
            }
        }
        #endregion//============================================================[ Unity ]

        #region Public
        [MenuItem("XcelerateGames/EditorTools/ScriptsFinder %&#_s", false)]
        public static void Init()
        {
            GetWindow(typeof(ScriptsFinder));
            GetWindow(typeof(ScriptsFinder)).minSize = new Vector2(300, 200);
            GetWindow(typeof(ScriptsFinder)).titleContent = new GUIContent("Scripts Finder");
        }
        #endregion//============================================================[ Public ]

        #region Private
        private void InitStyles()
        {
            styleHelpboxInner = new GUIStyle("box");
            styleHelpboxInner.padding = new RectOffset(6, 6, 6, 6);
            titleLabel = new GUIStyle();
            titleLabel.fontSize = 10;
            titleLabel.fontStyle = FontStyle.Bold;
            titleLabel.normal.textColor = Color.white;
            titleLabel.alignment = TextAnchor.UpperCenter;
            titleLabel.fixedHeight = 15;
            helpButtonStyle = new GUIStyle(GUI.skin.button);
            helpButtonStyle.fontSize = 10;
            helpButtonStyle.fontStyle = FontStyle.Bold;
            helpButtonStyle.normal.textColor = Color.white;
            helpButtonStyle.alignment = TextAnchor.MiddleCenter;
            normalButtonStyle = new GUIStyle(EditorStyles.foldoutHeader);
            normalButtonStyle.alignment = TextAnchor.MiddleLeft;
        }

        public void UpdateList()
        {
            Object[] objects;
            sets.Clear();
            objects = FindObjectsOfType(typeof(Component));
            foreach (Component component in objects)
                if (component.GetType().BaseType.ToString().Equals("UnityEngine.MonoBehaviour") &&
                    IsInsideProject(component.GetType().Name))
                {
                    if (!sets.ContainsKey(component.GetType())) sets[component.GetType()] = new ArrayList();
                    ((ArrayList)sets[component.GetType()]).Add(component.gameObject);
                }
            if (sets.Count > 0)
                hasAnyScript = true;
            else
                hasAnyScript = false;
        }

        private bool IsInsideProject(string fileName)
        {
            var tempPaths = new List<string>();
            var guids = AssetDatabase.FindAssets("t:Script");
            tempPaths = new List<string>();
            foreach (var itemPath in guids) tempPaths.Add(AssetDatabase.GUIDToAssetPath(itemPath));
            for (var count = 0; count < tempPaths.Count; count++)
                if (tempPaths[count].Contains(fileName))
                    return true;
            return false;
        }

        private void OnFocus()
        {
            UpdateList();
        }

        private string getPathOfFile(string tempName)
        {
            for (var count = 0; count < pathOfScripts.Count; count++)
                if (pathOfScripts[count].Contains(tempName + ".cs"))
                    return pathOfScripts[count];
            return "NaN";
        }
        #endregion//============================================================[ Private ]
    }
}