using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Keyboard;
using XcelerateGames.UI;

namespace XcelerateGames.EditorTools
{
    /// <summary>
    /// This class is works as a editor tool. It creates unique ids for all ui elements which have UiItem , XInputFiled or UiBase components attatched.
    /// </summary>
    public class UiIDUpdater : EditorWindow
    {
        public class UiData
        {
            public string assetpath;
            public string heirarchy;
        }

        private static Dictionary<string, UiData> mIDObj = new Dictionary<string, UiData>(); /**< Maintain key and path for UI */
        private const string FileName = "IdMapper.json"; /**< json file name where key and pathids are saved*/
#if AUTOMATION_ENABLED
        #region Private functions

        /// <summary>
        /// Updates IDs
        /// </summary>
        [MenuItem(Utilities.MenuName + "/EditorTools/Update UI ID")]
        static void UpdateID()
        {
            Debug.Log("Start Generating unique ids.....");

            //if file exists save ids to ID_Dict
            Debug.Log($"If IdMapper found {FileUtilities.FileExists(FileName)}");
            if (FileUtilities.FileExists(FileName))
            {
                string json = FileUtilities.ReadFromFile(FileName);
                if (!string.IsNullOrEmpty(json))
                {
                    mIDObj = json.FromJson<Dictionary<string, UiData>>();
                }
            }

            Object[] arrObj = Selection.objects;
            Debug.Log($"Objects found {arrObj.Length}");
            for (int i = 0; i < arrObj.Length; ++i)
            {
                Debug.Log($"gobject name {arrObj[i].name}");
                Object obj = arrObj[i];

                //if (obj is GameObject gObj)
                //{
                bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(obj);

                if (isPrefab)
                {
                    string assetPath = AssetDatabase.GetAssetPath(obj);
                    GameObject prefabobj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    //Update id for UiItem ********************************************
                    UiItem[] uiitem = prefabobj.GetComponentsInChildren<UiItem>(true);

                    Debug.Log($"all uiitem count {prefabobj.name} is {uiitem.Length}");
                    for (int j = 0; j < uiitem.Length; ++j)
                    {
                        string id = RegisterUniqueID(prefabobj, uiitem[j].transform.gameObject, uiitem[j].ID, assetPath);
                        if (string.IsNullOrEmpty(uiitem[j].ID) && !string.IsNullOrEmpty(id))
                        {
                            uiitem[j].UpdateID(id);
                        }
                    }
                    //Update id for UiItem ********************************************

                    //Update id for UiBase ********************************************
                    UiBase[] uibase = prefabobj.GetComponentsInChildren<UiBase>(true);
                    Debug.Log($"all uibase count {prefabobj.name} is {uibase.Length}");
                    for (int j = 0; j < uibase.Length; ++j)
                    {
                        string id = RegisterUniqueID(prefabobj, uibase[j].transform.gameObject, uibase[j].ID, assetPath);
                        if (string.IsNullOrEmpty(uibase[j].ID) && !string.IsNullOrEmpty(id))
                        {
                            uibase[j].UpdateID(id);
                        }
                    }
                    //Update id for UiBase ********************************************

                    //Update id for XInputField ********************************************
                    XInputField[] xinputfield = prefabobj.GetComponentsInChildren<XInputField>(true);
                    Debug.Log($"all xinputfield count {prefabobj.name} is {xinputfield.Length}");
                    for (int j = 0; j < xinputfield.Length; ++j)
                    {
                        string id = RegisterUniqueID(prefabobj, xinputfield[j].transform.gameObject, xinputfield[j].ID, assetPath);
                        if (string.IsNullOrEmpty(xinputfield[j].ID) && !string.IsNullOrEmpty(id))
                        {
                            xinputfield[j].UpdateID(id);
                        }
                    }
                    //Update id for XInputField ********************************************
                    PrefabUtility.SavePrefabAsset(prefabobj);
                }
                //}
            }

            FileUtilities.WriteToFile(FileName, mIDObj.ToJson()); //Save file to json
            Debug.Log("FILE SAVED !!!");
        }

        /// <summary>
        /// Returns Uniqueids for UIs
        /// </summary>
        /// <param name="prefab">Prefab gameobject reference</param>
        /// <param name="itemgobj">GameObject reference</param>
        /// <param name="itemid">type of string - id of GameObject</param>
        /// <returns></returns>
        static string RegisterUniqueID(GameObject prefab, GameObject itemgobj, string itemid, string _path)
        {
            Debug.Log($"item id for gameobject {itemgobj.GetObjectPath()} is {itemgobj}");
            string assetPath = _path.Replace(".prefab", "");
            string uid = null;
            string prefabname = prefab.name;
            string parentname = (itemgobj.transform.parent != null) ? itemgobj.transform.parent.name : null;
            string currentgObjName = itemgobj.name;
            if (string.IsNullOrEmpty(itemid))
            {

                string pName = (string.Compare(prefabname, currentgObjName) == 0) ? null : prefabname;
                uid = pName + (!string.IsNullOrEmpty(pName) ? "_" : "") + currentgObjName.Replace(" (TMP)", "");

                if (mIDObj.ContainsKey(uid))
                {

                    if (!string.IsNullOrEmpty(pName) && !string.IsNullOrEmpty(parentname) && (string.Compare(parentname, "Panel") != 0) && (string.Compare(parentname, pName) != 0))
                    {
                        uid = pName + "_" + parentname + "_" + currentgObjName.Replace(" (TMP)", "");
                    }
                    else
                    {
                        XDebug.LogError($"Error : Can not set ID id with for Object path {itemgobj.GetObjectPath()} , needed condition not met, id {uid}");
                        uid = null;
                    }

                    if (!string.IsNullOrEmpty(uid) && mIDObj.ContainsKey(uid))
                    {
                        XDebug.LogError($"Error : Can not set ID id with for Object path {itemgobj.GetObjectPath()} , id already exists in dictionary, id {uid}");
                        uid = null;
                    }
                }

                Debug.Log($"id formed {uid}");
                if (!string.IsNullOrEmpty(uid))
                {
                    UiData uiData = new UiData();
                    uiData.assetpath = assetPath;
                    uiData.heirarchy = itemgobj.GetObjectPath();
                    mIDObj.Add(uid, uiData);
                }
                return uid;
            }
            else
            {
                if (!mIDObj.ContainsKey(itemid))
                {
                    UiData uiData = new UiData();
                    uiData.assetpath = assetPath;
                    uiData.heirarchy = itemgobj.GetObjectPath();
                    mIDObj.Add(itemid, uiData);
                }
                return itemid;
            }
        }

        //[MenuItem(Utilities.MenuName + "/EditorTools/Reset UI ID")]
        //static void ResetID()
        //{

        //    Object[] arrObj = Selection.objects;
        //    Debug.Log($"Objects found {arrObj.Length}");
        //    for (int i = 0; i < arrObj.Length; ++i)
        //    {
        //        Debug.Log($"gobject name {arrObj[i].name}");
        //        Object obj = arrObj[i];
        //        //if (obj is GameObject gObj)
        //        //{
        //        bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(obj);
        //        if (isPrefab)
        //        {
        //            string assetPath = AssetDatabase.GetAssetPath(obj);
        //            GameObject prefabobj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        //            UiItem[] uiitem = prefabobj.GetComponentsInChildren<UiItem>(true);
        //            Debug.Log($"all uiitem count {prefabobj.name} is {uiitem.Length}");
        //            for (int j = 0; j < uiitem.Length; ++j)
        //            {
        //                uiitem[j].UpdateID(null);

        //            }

        //            UiBase[] uibase = prefabobj.GetComponentsInChildren<UiBase>(true);
        //            Debug.Log($"all uibase count {prefabobj.name} is {uibase.Length}");
        //            for (int j = 0; j < uibase.Length; ++j)
        //            {

        //                uibase[j].UpdateID(null);

        //            }

        //            XInputField[] xinputfield = prefabobj.GetComponentsInChildren<XInputField>(true);
        //            Debug.Log($"all xinputfield count {prefabobj.name} is {xinputfield.Length}");
        //            for (int j = 0; j < xinputfield.Length; ++j)
        //            {

        //                xinputfield[j].UpdateID(null);

        //            }
        //            PrefabUtility.SavePrefabAsset(prefabobj);
        //        }
        //        //}
        //    }
        //}
        #endregion
#endif
    }
}
