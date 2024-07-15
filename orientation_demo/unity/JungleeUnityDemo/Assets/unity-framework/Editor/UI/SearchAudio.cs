using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.UI.Animations;
using XcelerateGames.UI;

namespace XcelerateGames.Editor.UI
{
    internal class SearchAudio : EditorWindow
    {
        private List<GameObject> mObjects = new List<GameObject>();

        private Vector2 mScroll = Vector2.zero;
        private string mClipName = null;

        [MenuItem(Utilities.MenuName + "UI/Search Audio")]
        private static void CreateSearchAnimAudio()
        {
            GetWindow<SearchAudio>();
        }

        private void Awake()
        {
            titleContent.text = "Search Audio";
        }

        private void UpdateAnimList()
        {
            mObjects.Clear();
            UiAnim[] anims = Resources.FindObjectsOfTypeAll<UiAnim>();
            foreach(UiAnim anim in anims)
            {
                //UiAnimBase animBase = anim._Anims.Find(e => Utilities.Equals(e._AudioClip, mClipName));
                UiAnimBase animBase = anim._Anims.Find(e => Utilities.Equals(e._AudioVars._SoundClip, mClipName));
                if (animBase != null && anim.gameObject.scene.rootCount > 0)
                    mObjects.Add(anim.gameObject);
            }

            UiItem[] uiItems  = Resources.FindObjectsOfTypeAll<UiItem>();
            foreach (UiItem item in uiItems)
            {
                if (item.gameObject.scene.rootCount > 0 && Utilities.Equals(item._ClickSound, mClipName))
                    mObjects.Add(item.gameObject);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            mClipName = GUILayout.TextField(mClipName);
            if (GUILayout.Button("Search"))
            {
                UpdateAnimList();
            }
            GUILayout.EndHorizontal();
            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach(GameObject obj in mObjects)
            {
                if (obj == null)
                    continue;
                if(GUILayout.Button(obj.GetObjectPath()))
                {
                    EditorGUIUtility.PingObject(obj);
                }
            }
            GUILayout.EndScrollView();
        }
    }
}