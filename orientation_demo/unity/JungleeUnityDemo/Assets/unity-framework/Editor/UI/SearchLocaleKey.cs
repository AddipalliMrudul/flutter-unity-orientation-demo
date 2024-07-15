using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Locale;

/*  Author : Altaf
 *  Purpose : To find all UILocalizeTMP scripts for the given locale key
 */

namespace XcelerateGames.Editor.UI
{
    internal class SearchLocaleKey : EditorWindow
    {
        private List<GameObject> mObjects = new List<GameObject>();

        private Vector2 mScroll = Vector2.zero;
        private string mKey = null;

        [MenuItem(Utilities.MenuName + "Localization/Search key")]
        private static void DoSearchLocaleKey()
        {
            GetWindow<SearchLocaleKey>();
        }

        private void Awake()
        {
            titleContent.text = "Search Key";
        }

        private void UpdateAnimList()
        {
            mObjects.Clear();
            UILocalizeTMP[] localeScripts = Resources.FindObjectsOfTypeAll<UILocalizeTMP>();
            foreach(UILocalizeTMP obj in localeScripts)
            {
                if(Utilities.Equals(obj.key, mKey))
                {
                    if (obj.gameObject.scene.rootCount > 0)
                        mObjects.Add(obj.gameObject);
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            mKey = GUILayout.TextField(mKey);
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