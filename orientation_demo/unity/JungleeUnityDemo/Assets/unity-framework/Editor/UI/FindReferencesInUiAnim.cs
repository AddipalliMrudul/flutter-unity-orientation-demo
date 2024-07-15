using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.UI.Animations;
using XcelerateGames.UI;

namespace XcelerateGames.Editor.UI
{
    internal class FindReferencesInUiAnim : EditorWindow
    {
        public class AnimReferencesData
        {
            public GameObject _GameObject = null;
            public UiAnimBase _AnimBase = null;

            public AnimReferencesData(GameObject go, UiAnimBase animBase)
            {
                _GameObject = go;
                _AnimBase = animBase;
            }
        }
        private GameObject mSelectedObject = null;
        private List<GameObject> mUiItems = new List<GameObject>();
        private List<AnimReferencesData> mUiAnims = new List<AnimReferencesData>();

        private Vector2 mScroll = Vector2.zero;

        [MenuItem(Utilities.MenuName + "UI/Anim/Find references in UiAnim")]
        private static void CreateFindReferencesInUiAnim()
        {
            GetWindow<FindReferencesInUiAnim>();
        }

        private void Awake()
        {
            titleContent.text = "Find references";
            mSelectedObject = Selection.activeGameObject;
        }

        private void OnWizardCreate()
        { }

        private void OnWizardOtherButton()
        {
        }

        private void OnWizardUpdate()
        {
            //mSelectedObject = Selection.activeTransform.gameObject;
            //UpdateAnimList();
        }

        private void UpdateAnimList()
        {
            mUiItems.Clear();
            mUiAnims.Clear();

            #region UI Anims

            UiAnim[] anims = Resources.FindObjectsOfTypeAll<UiAnim>();
            foreach (UiAnim anim in anims)
            {
                anim._Anims.ForEach(animBase =>
               {
                   FindReference(animBase._OnAnimationStart, anim.gameObject, animBase);

                   FindReference(animBase._OnAnimationDone, anim.gameObject, animBase);

                   FindReference(anim._OnAnimationDone, anim.gameObject, animBase);
               });
            }
            #endregion

            #region UiItem references
            UiItem[] items = Resources.FindObjectsOfTypeAll<UiItem>();
            foreach (UiItem item in items)
            {
                int count = item._OnClick.GetPersistentEventCount();
                if (count > 0)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        GameObject itemObj = item._OnClick.GetPersistentTarget(i) as GameObject;
                        if (itemObj != null)
                        {
                            if (item._OnClick.GetPersistentTarget(i).GetInstanceID() == mSelectedObject.GetInstanceID())
                                mUiItems.Add(item.gameObject);
                        }
                    }
                }
            }

            #endregion
        }

        private void FindReference(CGUiAnimEvent unityEvent, GameObject gameObject, UiAnimBase animBase)
        {
            int count = unityEvent.GetPersistentEventCount();
            if (count > 0)
            {
                for (int i = 0; i < count; ++i)
                {
                    GameObject itemObj = unityEvent.GetPersistentTarget(i) as GameObject;
                    if (itemObj != null)
                    {
                        if (itemObj.GetInstanceID() == mSelectedObject.GetInstanceID())
                            mUiAnims.Add(new AnimReferencesData(gameObject, animBase));
                    }
                }
            }

        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            mSelectedObject = (GameObject)EditorGUILayout.ObjectField(mSelectedObject, typeof(GameObject), allowSceneObjects: true);
            if (mSelectedObject != null)
                GUILayout.Label(mSelectedObject.GetObjectPath());
            GUILayout.EndVertical();

            if (GUILayout.Button("Search"))
            {
                UpdateAnimList();
            }
            GUILayout.EndHorizontal();
            mScroll = GUILayout.BeginScrollView(mScroll);
            if (mUiItems.Count > 0)
            {
                GUILayout.Label("----------------------------UiItem----------------------------", GUILayout.Width(512));

                foreach (GameObject go in mUiItems)
                {
                    if (go == null)
                        continue;
                    if (GUILayout.Button(go.GetObjectPath()))
                    {
                        EditorGUIUtility.PingObject(go);
                    }
                }
            }

            if (mUiAnims.Count > 0)
            {
                GUILayout.Label("----------------------------UiAnim----------------------------", GUILayout.Width(512));

                foreach (AnimReferencesData go in mUiAnims)
                {
                    if (go == null)
                        continue;
                    if (GUILayout.Button(go._GameObject.GetObjectPath() + "->" + go._AnimBase._Name))
                    {
                        EditorGUIUtility.PingObject(go._GameObject);
                    }
                }
            }
            GUILayout.EndScrollView();
        }
    }
}