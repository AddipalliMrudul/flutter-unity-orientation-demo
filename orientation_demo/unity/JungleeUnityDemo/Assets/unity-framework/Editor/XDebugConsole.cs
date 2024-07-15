using UnityEngine;
using UnityEditor;
//using XcelerateGames.Inspectors;
using System;

namespace XcelerateGames.Editor
{
    public class XDebugConsole : EditorWindow
    {
        private XDebug.Mask mMask = XDebug.Mask.None;
        private XDebug.Priority mPriority = XDebug.Priority.None;
        private Vector2 mLogScroll = Vector2.zero;
        private Vector2 mStackScroll = Vector2.zero;

        private GUISkin mButonSkin = null;
        private GUISkin mPreviousSkin = null;
        private EditorGUISplitView mSplitView = new EditorGUISplitView(EditorGUISplitView.Direction.Vertical);
        private XDebug.DebugLog mSelectedLog = null;

        [MenuItem(Utilities.MenuName + "Debug Console")]
        private static void DoXDebugConsole()
        {
            GetWindow<XDebugConsole>().titleContent.text = "Debug Console";
        }

        private void Awake()
        {
            mButonSkin = (GUISkin)EditorGUIUtility.Load("DebugConsoleButton.guiskin");
            if (mButonSkin == null)
                Debug.LogError("Failed to load skin DebugConsoleButton");
        }

        private void OnDisable()
        {
            XGEditorPrefs.SetString("Mask", mMask.ToString());
        }

        private void OnEnable()
        {
            try
            {
                if (XGEditorPrefs.HasKey("Mask"))
                {
                    string[] masks = XGEditorPrefs.GetString("Mask").Split(',');
                    foreach (string m in masks)
                        mMask |= ((XDebug.Mask)Enum.Parse(typeof(XDebug.Mask), m, true));
                }
            }
            catch (Exception e)
            {
                XDebug.LogError("AddMask:: Failed to Add mask : " + e.Message);
            }
        }

        private void OnGUI()
        {
            mPreviousSkin = GUI.skin;
            GUI.skin = mButonSkin;

            mSplitView.BeginSplitView();
            #region Mask Selection
            EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));

            EditorGUILayout.LabelField("Mask");
            mMask = (XDebug.Mask)EditorGUILayout.EnumFlagsField(mMask);

            EditorGUILayout.LabelField("Priority");
            mPriority = (XDebug.Priority)EditorGUILayout.EnumPopup(mPriority);

            if (GUILayout.Button("Clear"))
                XDebug.ClearLogs();

            EditorGUILayout.EndHorizontal();
            #endregion Mask Selection

            mLogScroll = EditorGUILayout.BeginScrollView(mLogScroll);
            for (int i = XDebug.mDebugLogs.Count - 1; i >= 0; --i)
            {
                XDebug.DebugLog logInfo = XDebug.mDebugLogs[i];
                if ((mMask & logInfo.mask) != 0)
                {
                    if (GUILayout.Button(logInfo.logString))
                    {
                        mSelectedLog = logInfo;
                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            mSplitView.Split();
            if (mSelectedLog != null)
            {
                #region Stack view controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Log"))
                    EditorGUIUtility.systemCopyBuffer = mSelectedLog.logString;
                if (GUILayout.Button("Search on Web"))
                {
                    string query = "https://www.google.com/search?q=" + mSelectedLog.logString.Replace(" ", "+");
                    Application.OpenURL(query);
                }
                if (GUILayout.Button("Copy Stack"))
                    EditorGUIUtility.systemCopyBuffer = mSelectedLog.stack;
                if (GUILayout.Button("Copy All"))
                    EditorGUIUtility.systemCopyBuffer = mSelectedLog.logString + "\n\n" + mSelectedLog.stack;
                EditorGUILayout.EndHorizontal();
                #endregion

                mStackScroll = EditorGUILayout.BeginScrollView(mStackScroll);
                EditorGUILayout.SelectableLabel(mSelectedLog.logString + "\n\n" + mSelectedLog.stack, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
            mSplitView.EndSplitView();
            GUI.skin = mPreviousSkin;
        }
    }
}
