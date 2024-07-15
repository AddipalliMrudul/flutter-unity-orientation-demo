using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.UI.Animations;

namespace XcelerateGames.Editor.UI
{
    internal class SearchUiAnim : EditorWindow
    {
        private List<UiAnim> mAnimNames = new List<UiAnim>();

        private Vector2 mScroll = Vector2.zero;
        private string mAnimName = null;

        [MenuItem(Utilities.MenuName + "UI/Anim/Search Anim")]
        private static void CreatePlayAnim3DWizard()
        {
            GetWindow<SearchUiAnim>();
        }

        private void Awake()
        {
            titleContent.text = "Search UI Anim";
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

        private void UpdateAnimList()
        {
            mAnimNames.Clear();
            UiAnim[] anims = Resources.FindObjectsOfTypeAll<UiAnim>();
            foreach(UiAnim anim in anims)
            {
                if(mAnimName.Equals("*.*"))
                {
                    mAnimNames.Add(anim);
                }
                else
                {
                    UiAnimBase animBase = anim._Anims.Find(e => Utilities.Equals(e._Name, mAnimName));
                    if (animBase != null && anim.gameObject.scene.rootCount > 0)
                        mAnimNames.Add(anim);
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            mAnimName = GUILayout.TextField(mAnimName);
            if (GUILayout.Button("Search"))
            {
                UpdateAnimList();
            }
            //if (GUILayout.Button("Show All"))
            //{
                //mAnimName = "*.*";
                //UpdateAnimList();
            //}
            GUILayout.EndHorizontal();
            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach(UiAnim anim in mAnimNames)
            {
                if (anim == null)
                    continue;
                if(GUILayout.Button(anim.GetObjectPath()))
                {
                    EditorGUIUtility.PingObject(anim);
                }
            }
            GUILayout.EndScrollView();
        }
    }
}