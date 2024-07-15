using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.EditorTools
{
    public class TextPad : EditorWindow
    {
        #region Constant
        public const string TEXT_PAD_PLAYER_PREF = "TEXT_PAD";
        #endregion//============================================================[ Constant ]

        #region Data
        //Private
        private string text;
        private Vector2 scrollPos;
        private bool wrap;
        #endregion//============================================================[ Data ]

        #region Unity
        private void OnGUI()
        {
            DrawWindow();
        }
        #endregion//============================================================[ Unity ]

        #region Public
        [MenuItem("XcelerateGames/EditorTools/TextPad %&#_t", false)]
        public static void ShowWindow()
        {
            var window = (TextPad)GetWindow(typeof(TextPad), false, "Text Pad");
            window.Show();
        }
        #endregion//============================================================[ Public ]

        #region Private
        private void DrawWindow()
        {
            if (PlayerPrefs.HasKey(TEXT_PAD_PLAYER_PREF))
            {
                text = PlayerPrefs.GetString(TEXT_PAD_PLAYER_PREF);
            }
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Data", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy", "Copies text"), false, () =>
                {
                    EditorGUIUtility.systemCopyBuffer = text;
                    GUI.FocusControl("");
                });
                menu.AddItem(new GUIContent("Cut", "Copies text and clears"), false, () =>
                {
                    EditorGUIUtility.systemCopyBuffer = text;
                    text = string.Empty;
                    GUI.FocusControl("");
                });
                menu.AddItem(new GUIContent("Paste", "Pastes text"), false, () =>
                {
                    text = EditorGUIUtility.systemCopyBuffer;
                    GUI.FocusControl("");
                });
                menu.ShowAsContext();
            }
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                text = string.Empty;
                GUI.FocusControl("");
            }
            if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(17)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Spaces/All", "Remove all spaces"), false, () =>
                {
                    text = Regex.Replace(text, @"\s", "");
                    GUI.FocusControl("");
                });
                menu.AddItem(new GUIContent("Spaces/Multiple", "Replace multiple spaces with single space"), false,
                    () =>
                    {
                        text = Regex.Replace(text, " {2,}", " ");
                        GUI.FocusControl("");
                    });
                menu.AddItem(new GUIContent("New Line/All", "Remove all new lines"), false, () =>
                {
                    text = Regex.Replace(text, @"\n", "");
                    GUI.FocusControl("");
                });
                menu.AddItem(new GUIContent("New Line/Multiple", "Replace multiple new lines with single new line"),
                    false, () =>
                    {
                        text = Regex.Replace(text, "(\\n){2,}", "\n");
                        GUI.FocusControl("");
                    });
                menu.ShowAsContext();
            }
            if (GUILayout.Button("Format", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                text = Format(text);
                GUI.FocusControl("");
            }
            if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(17)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Escape", "Escapes text for URL"), false, () =>
                {
                    text = UnityEngine.Networking.UnityWebRequest.EscapeURL(text).Replace("+", "%20").Trim();
                    GUI.FocusControl("");
                });
                menu.AddItem(new GUIContent("Trim", "Trims text"), false, () =>
                {
                    text = text.Trim();
                    GUI.FocusControl("");
                });
                menu.ShowAsContext();
            }
            if (GUILayout.Button("Minify", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                text = Regex.Replace(text, @"\s", "");
                text = Regex.Replace(text, @"\n", "");
                text = text.Trim();
                GUI.FocusControl("");
            }
            EditorGUILayout.EndHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            text = EditorGUILayout.TextArea(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField("©2021 Xcelerate Games",
                    new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter },
                    GUILayout.ExpandWidth(true), GUILayout.Height(15));
            }
            EditorUtility.SetDirty(this);
            PlayerPrefs.SetString(TEXT_PAD_PLAYER_PREF, text);
            PlayerPrefs.Save();
        }
        #endregion//============================================================[ Private ]

        #region Utils
        private string Format(string json)
        {
            const int indentWidth = 4;
            const string pattern =
                "(?>([{\\[][}\\]],?)|([{\\[])|([}\\]],?)|([^{}:]+:)([^{}\\[\\],]*(?>([{\\[])|,)?)|([^{}\\[\\],]+,?))";
            var match = Regex.Match(json, pattern);
            var beautified = new StringBuilder();
            var indent = 0;
            while (match.Success)
            {
                if (match.Groups[3].Length > 0)
                    indent--;

                beautified.AppendLine(
                    new string(' ', indent * indentWidth) +
                    (match.Groups[4].Length > 0
                        ? match.Groups[4].Value + " " + match.Groups[5].Value
                        : match.Groups[7].Length > 0
                            ? match.Groups[7].Value
                            : match.Value)
                );

                if (match.Groups[2].Length > 0 || match.Groups[6].Length > 0)
                    indent++;

                match = match.NextMatch();
            }
            return beautified.ToString();
        }
        #endregion//============================================================[ Utils ]
    }
}