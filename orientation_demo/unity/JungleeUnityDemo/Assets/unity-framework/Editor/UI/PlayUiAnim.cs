using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.UI;
using XcelerateGames.UI.Animations;

namespace XcelerateGames.Editor.UI
{
    internal class PlayUiAnim : EditorWindow
    {
        private List<string> mAnimNames = new List<string>();

        private Transform mPreviousTrans = null;
        private static PlayUiAnim mWindow = null;

        private UiAnim mAnimObject;
        private UiBase mUiBase = null;
        private bool mSupressAllTriggers = false;
        private Vector2 mScroll = Vector2.zero;

        [MenuItem(Utilities.MenuName + "UI/Anim/Play")]
        private static void CreatePlayAnim3DWizard()
        {
            mWindow = GetWindow<PlayUiAnim>();
            mWindow.titleContent.text = "Play UI Anim";
            Selection.selectionChanged += mWindow.OnSelectionChange;
        }

        private void OnWizardCreate()
        { }

        private void OnWizardOtherButton()
        {
        }

        private void OnWizardUpdate()
        {
            UpdateAnimList();
        }

        private void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChange;
        }

        private void UpdateAnimList()
        {
            if (Selection.activeTransform == null)
                return;

            if (Selection.activeTransform != mPreviousTrans)
            {
                mPreviousTrans = Selection.activeTransform;
                mAnimNames.Clear();

                mAnimObject = Selection.activeTransform.GetComponent(typeof(UiAnim)) as UiAnim;
                if (mAnimObject != null)
                {
                    foreach (UiAnimBase ab in mAnimObject._Anims)
                        mAnimNames.Add(ab._Name);
                }
                else
                {
                    mUiBase = Selection.activeTransform.GetComponent<UiBase>() as UiBase;
                    if (mUiBase != null)
                    {
                        UiAnim[] animObjs = mUiBase.GetComponentsInChildren<UiAnim>(true);
                        foreach (UiAnim anim3d in animObjs)
                        {
                            foreach (UiAnimBase ab in anim3d._Anims)
                            {
                                if (!mAnimNames.Contains(ab._Name))
                                    mAnimNames.Add(ab._Name);
                            }
                        }
                    }
                }

                mAnimNames.Sort();
            }
        }

        private void OnGUI()
        {
            mScroll = GUILayout.BeginScrollView(mScroll);
            GUILayout.BeginVertical();
            if (Selection.activeTransform == null)
                GUILayout.Label("Select an object");
            else if (mAnimNames.Count == 0)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("Neither UiAnim or UiBase component found under : " + Selection.activeTransform.gameObject.GetObjectPath());
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

                if (!Selection.activeTransform.gameObject.activeInHierarchy)
                    GUILayout.Label("GameObject is In-Active, No Animations will be played.");
                mSupressAllTriggers = GUILayout.Toggle(mSupressAllTriggers, "Supress All Triggers");
                if (mAnimNames != null)
                {
                    for (int i = 0; i < mAnimNames.Count; i++)
                    {
                        GUILayoutOption[] options = new GUILayoutOption[1];
                        options[0] = GUILayout.Width(256);
                        GUIColor.Push(Color.red);
                        GUIStyle headStyle = new GUIStyle();
                        headStyle.normal.textColor = Color.red;
                        headStyle.fontSize = 25;
                        headStyle.fontStyle = FontStyle.Bold;
                        GUILayout.Label(mAnimNames[i], headStyle, new GUILayoutOption[0] { });
                        GUIColor.Pop();
                        if (GUILayout.Button("Play", options))
                        {
                            if (mSupressAllTriggers)
                            {
                                if (mAnimObject != null)
                                    mAnimObject.SilentPlay(mAnimNames[i]);
                                else if (mUiBase != null)
                                    mUiBase.BroadcastMessage("SilentPlay", mAnimNames[i]);
                            }
                            else
                            {
                                if (mAnimObject != null)
                                    mAnimObject.Play(mAnimNames[i]);
                                else if (mUiBase != null)
                                    mUiBase.BroadcastMessage("Play", mAnimNames[i]);
                            }
                        }
                        if (GUILayout.Button("Play Reverse", options))
                        {
                            mAnimObject.PlayAnimReverse(mAnimNames[i]);
                        }
                        if (GUILayout.Button("Stop", options))
                        {
                            mAnimObject.Stop();
                        }
                        if (GUILayout.Button("Stop & reset to beginning", options))
                        {
                            mAnimObject.StopAndResetToBeginning(mAnimNames[i]);
                        }
                        if (GUILayout.Button("Stop & reset to end", options))
                        {
                            mAnimObject.StopAndResetToEnd(mAnimNames[i]);
                        }
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Root UI Animations");
                if (GUILayout.Button("Show", GUILayout.Width(256)))
                {
                    UiBase uiBase = Utilities.GetUiBase(mUiBase.transform);
                    if (uiBase != null)
                    {
                        uiBase.gameObject.SetActive(true);
                        uiBase.Show();
                    }
                }
                if (GUILayout.Button("Hide", GUILayout.Width(256)))
                {
                    UiBase uiBase = Utilities.GetUiBase(mUiBase.transform);
                    if (uiBase != null)
                    {
                        uiBase.gameObject.SetActive(true);
                        uiBase.Hide();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void OnSelectionChange()
        {
            UpdateAnimList();
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnFocus()
        {
            mPreviousTrans = null;
            UpdateAnimList();
        }
    }
}