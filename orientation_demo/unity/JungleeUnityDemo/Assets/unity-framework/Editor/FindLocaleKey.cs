//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using GameJam;
//using System.Collections.Generic;

//public class FindLocaleKey : EditorWindow
//{
//    private Vector2 mScrollPos = new Vector2(400, 300);
//    class SelectedItems
//    {
//        public string _Name = "";
//        public string _Text = "";
//        public GameObject _GameObject = null;
//    }

//    private List<SelectedItems> mSelectedItems = new List<SelectedItems>();
//    public string _Key = null;
//    public bool _Exact = true;

//    [MenuItem(Utilities.MenuName + "Locale/Find Locale by Key")]
//    static void DoFindLocaleKey()
//    {
//        //ScriptableWizard.DisplayWizard("Find Locale Key", typeof(FindLocaleKey), "Find");
//        EditorWindow.GetWindow<FindLocaleKey>();
//    }

//    void OnWizardCreate()
//    {
//        mSelectedItems.Clear();
//        UILocalize[] locScripts = Resources.FindObjectsOfTypeAll<UILocalize>();
//        if (locScripts != null)
//        {
//            foreach (UILocalize loc in locScripts)
//            {
//                bool canAdd = false;
//                if (_Exact)
//                {
//                    if (loc.key.Equals(_Key, System.StringComparison.OrdinalIgnoreCase))
//                        canAdd = true;
//                }
//                else if (loc.key.ToLower().Contains(_Key.ToLower()))
//                    canAdd = true;
//                if (canAdd)
//                {
//                    //Debug.Log(Utilities.GetObjectPath(loc.gameObject));
//                    SelectedItems item = new SelectedItems();
//                    item._Text = loc.key;
//                    item._Name = Utilities.GetObjectPath(loc.gameObject);
//                    item._GameObject = loc.gameObject;
//                    mSelectedItems.Add(item);
//                }
//            }
//        }
//    }

//    void OnGUI()
//    {
//        EditorGUILayout.BeginHorizontal();
//        GUILayout.Label("Key : ");
//        _Key = EditorGUILayout.TextField(_Key);
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.Space();

//        EditorGUILayout.BeginHorizontal();
//        GUILayout.Label("Exact Match : ");
//        _Exact = EditorGUILayout.Toggle(_Exact);
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.Space();

//        if (GUILayout.Button("Find"))
//            OnWizardCreate();

//        if (mSelectedItems.Count > 0)
//        {
//            EditorGUILayout.BeginHorizontal();
//            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos, GUILayout.Width(256), GUILayout.Height(600));
//            {
//                foreach (SelectedItems s in mSelectedItems)
//                {
//                    if (GUILayout.Button(s._Text))// + "--->" + s._Name))
//                        EditorGUIUtility.PingObject(s._GameObject);
//                }
//            }
//            EditorGUILayout.EndScrollView();
//            EditorGUILayout.EndHorizontal();
//        }
//    }

//    [MenuItem(Utilities.MenuName + "Locale/Remove empty Locale scripts")]
//    static void RemoveEmptyLocaleScripts()
//    {
//        int removed = 0;
//        int total = 0;
//        foreach (Transform t in Selection.transforms)
//        {
//            //Delete all extra UILocalize scripts attached to the same label & have empty key, we need only one
//            UILocalize[] locs = t.GetComponents<UILocalize>();
//            if (locs != null)
//            {
//                total += locs.Length;
//                int count = 0;
//                foreach (UILocalize locScript in locs)
//                {
//                    if (string.IsNullOrEmpty(locScript.key) || count != locs.Length - 1)
//                    {
//                        removed++;
//                        Debug.Log(Utilities.GetObjectPath(t.gameObject));
//                        DestroyImmediate(locScript, true);
//                    }
//                    count++;
//                }
//            }
//        }
//        Debug.LogError("Removed : " + removed + "/" + total);
//    }
//}