﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.EditorTools
{
    public partial class UnityShellEditorWindow : EditorWindow
    {
        #region Data
        //Private
        private const string ConsoleTextAreaControlName = "ConsoleTextArea";
        private const string CommandName = "Unity > ";
        [SerializeField] private AutocompleteBox autocompleteBox;
        private string input = "";
        [SerializeField] private List<string> inputHistory = new List<string>();
        private Vector2 lastCursorPos;
        private string lastWord = "";
        private int positionInHistory;
        private bool requestFocusOnTextArea;
        private bool requestMoveCursorToEnd;
        private bool requestRevertNewLine;
        private string savedInput;
        [SerializeField] private Vector2 scrollPos = Vector2.zero;
        [SerializeField] private ShellEvaluator shellEvaluator;
        [SerializeField] private TextEditor textEditor;
        #endregion//============================================================[ Data ]

        #region GetterSetter
        private string text
        {
            get => textEditor.text;
            set => textEditor.text = value;
        }
        #endregion//============================================================[ GetterSetter ]

        #region Unity
        private void Awake()
        {
            ClearText();
            requestFocusOnTextArea = true;

            shellEvaluator = new ShellEvaluator();
            autocompleteBox = new AutocompleteBox();
        }
        private void OnEnable()
        {
            ScheduleMoveCursorToEnd();
            autocompleteBox.onConfirm += OnAutocompleteConfirm;
            autocompleteBox.Clear();
        }
        private void OnGUI()
        {
            textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (text == "")
            {
                AppendStartCommand();
                ScheduleMoveCursorToEnd();
            }

            HandleInvalidTypePositions();
            autocompleteBox.HandleEvents();
            HandleHistory();
            DoAutoComplete();
            HandleRequests();
            DrawAll();
        }
        #endregion//============================================================[ Unity ]

        #region Public
        [MenuItem("XcelerateGames/EditorTools/UnityShell %&#_s")]
        private static void CreateWindow()
        {
            GetWindow<UnityShellEditorWindow>("Unity Shell");
        }
        #endregion//============================================================[ Public ]

        #region Private
        private void OnAutocompleteConfirm(string confirmedInput)
        {
            text = text.Substring(0, text.Length - lastWord.Length);
            text += confirmedInput;
            lastWord = confirmedInput;
            requestRevertNewLine = true;
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void HandleHistory()
        {
            var current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                var changed = false;
                if (current.keyCode == KeyCode.DownArrow)
                {
                    positionInHistory++;
                    changed = true;
                    current.Use();
                }
                if (current.keyCode == KeyCode.UpArrow)
                {
                    positionInHistory--;
                    changed = true;
                    current.Use();
                }
                if (changed)
                {
                    if (savedInput == null) savedInput = input;
                    if (positionInHistory < 0)
                    {
                        positionInHistory = 0;
                    }
                    else if (positionInHistory >= inputHistory.Count)
                    {
                        ReplaceCurrentCommand(savedInput);
                        positionInHistory = inputHistory.Count;
                        savedInput = null;
                    }
                    else
                    {
                        ReplaceCurrentCommand(inputHistory[positionInHistory]);
                    }
                }
            }
        }

        private void ReplaceCurrentCommand(string replacement)
        {
            text = text.Substring(0, text.Length - input.Length);
            text += replacement;
            textEditor.MoveTextEnd();
        }

        private void DoAutoComplete()
        {
            var newInput = GetInput();
            if (newInput != null && input != newInput && !requestRevertNewLine)
            {
                input = newInput;
                lastWord = input;
                var lastWordIndex = input.LastIndexOfAny(new[] { '(', ' ' });
                if (lastWordIndex != -1) lastWord = input.Substring(lastWordIndex + 1);
                shellEvaluator.SetInput(lastWord);
            }
        }

        private string GetInput()
        {
            var commandStartIndex = text.LastIndexOf(CommandName, StringComparison.Ordinal);
            if (commandStartIndex != -1)
            {
                commandStartIndex += CommandName.Length;
                return text.Substring(commandStartIndex);
            }
            return null;
        }

        private void HandleRequests()
        {
            var current = Event.current;
            if (requestMoveCursorToEnd && current.type == EventType.Repaint)
            {
                textEditor.MoveTextEnd();
                requestMoveCursorToEnd = false;
                Repaint();
            }
            else if (focusedWindow == this && requestFocusOnTextArea)
            {
                GUI.FocusControl(ConsoleTextAreaControlName);
                requestFocusOnTextArea = false;
                Repaint();
            }
            var cursorPos = textEditor.graphicalCursorPos;
            if (current.type == EventType.Repaint && cursorPos.y > lastCursorPos.y && requestRevertNewLine)
            {
                textEditor.Backspace();
                textEditor.MoveTextEnd();
                Repaint();
                requestRevertNewLine = false;
            }
            lastCursorPos = cursorPos;
        }

        private void HandleInvalidTypePositions()
        {
            var current = Event.current;
            if (current.isKey && !current.command && !current.control)
            {
                var lastIndexCommand = text.LastIndexOf(CommandName, StringComparison.Ordinal) + CommandName.Length;
                var cursorIndex = textEditor.cursorIndex;
                if (current.keyCode == KeyCode.Backspace) cursorIndex--;
                if (cursorIndex < lastIndexCommand)
                {
                    ScheduleMoveCursorToEnd();
                    current.Use();
                }
            }
        }

        private void DrawAll()
        {
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), Styles.backgroundTexture, ScaleMode.StretchToFill);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton)) ClearText();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.LabelField("©2021 Xcelerate Games",
                        new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter },
                        GUILayout.Width(150));
                }
            }
            EditorGUILayout.EndHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                DrawConsole();
            }
            EditorGUILayout.EndScrollView();
            autocompleteBox.results = shellEvaluator.completions;
            var pos = textEditor.graphicalCursorPos;
            var rect = new Rect(pos.x, pos.y, 300, 200);
            rect.y += 34;
            autocompleteBox.OnGUI(lastWord, rect);
        }

        private void DrawConsole()
        {
            var current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                ScrollDown();
                if (current.keyCode == KeyCode.Return && !current.shift)
                {
                    textEditor.MoveTextEnd();
                    try
                    {
                        var result = CustomCommand(shellEvaluator.Evaluate(input));
                        Append(result);
                        inputHistory.Add(input);
                        positionInHistory = inputHistory.Count;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Append(e.Message);
                    }
                    AppendStartCommand();
                    ScheduleMoveCursorToEnd();
                    current.Use();
                    if (input == CLEAR) ClearText();
                }
            }
            GUI.SetNextControlName(ConsoleTextAreaControlName);
            GUILayout.TextArea(text, Styles.textAreaStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        }

        private void ScrollDown()
        {
            scrollPos.y = float.MaxValue;
        }

        private void AppendStartCommand()
        {
            text += CommandName;
        }

        private void ScheduleMoveCursorToEnd()
        {
            requestMoveCursorToEnd = true;
            ScrollDown();
        }

        private void Append(object result)
        {
            text += "\n\t" + result + "\n";
        }
        #endregion//============================================================[ Private ]

        #region Utils
        private static class Styles
        {
            public static readonly GUIStyle textAreaStyle;
            private static readonly Color bgColorLightSkin = new Color(0.87f, 0.87f, 0.87f);
            private static readonly Color bgColorDarkSkin = new Color(0.2f, 0.2f, 0.2f);
            private static readonly Color textColorLightSkin = new Color(0.0f, 0.0f, 0.0f);
            private static readonly Color textColorDarkSkin = new Color(0.706f, 0.706f, 0.706f);
            private static Texture2D _backgroundTexture;

            static Styles()
            {
                textAreaStyle = new GUIStyle(EditorStyles.textArea);
                textAreaStyle.padding = new RectOffset();
                var style = textAreaStyle.focused;
                style.background = backgroundTexture;
                style.textColor = EditorGUIUtility.isProSkin ? textColorDarkSkin : textColorLightSkin;
                textAreaStyle.focused = style;
                textAreaStyle.active = style;
                textAreaStyle.onActive = style;
                textAreaStyle.hover = style;
                textAreaStyle.normal = style;
                textAreaStyle.onNormal = style;
            }

            public static Texture2D backgroundTexture
            {
                get
                {
                    if (_backgroundTexture == null)
                    {
                        _backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                        _backgroundTexture.SetPixel(0, 0,
                            EditorGUIUtility.isProSkin ? bgColorDarkSkin : bgColorLightSkin);
                        _backgroundTexture.Apply();
                    }
                    return _backgroundTexture;
                }
            }
        }

        public void ClearText()
        {
            if (textEditor != null) text = "";
        }
        #endregion//============================================================[ Utils ]
    }
}