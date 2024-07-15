using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#pragma warning disable 618

namespace XcelerateGames.Editor.AssetBundles
{
    public class GetAssetBundleItems : ScriptableWizard
    {

        class ObjectInfo
        {
            public string _Name = "";
            public string _Info = "";
        }

        private WWW mDownloader = null;

        private Vector2 mBundlesScrollPosition = Vector2.zero;
        private Rect mBundlesViewRect = new Rect(0, 0, 300, 300);
        private Rect mBundleLayoutPosition = new Rect(20, 30, 600, 500);

        private List<ObjectInfo> mPrefabs = new List<ObjectInfo>();

        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Get AssetBundle Items", false, 34)]
        static void CreateGetAssetBundleItems()
        {
            GetAssetBundleItems window = ScriptableWizard.DisplayWizard("Get AssetBundle Items", typeof(GetAssetBundleItems), "", "") as GetAssetBundleItems;
            window.position = new Rect(100, 100, 400, 300);
            window.GetAssetBundleData(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        /// <summary>
        /// Enables the menu only for AssetBundles
        /// </summary>
        /// <returns></returns>
        //[MenuItem(BuildAssetBundle.AssetBundleMenu + "Get AssetBundle Items", true)]
        //static bool ValidateGetAssetBundleItems()
        //{
        //    if (Selection.activeObject != null)
        //        return AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith("unity3d");
        //    else
        //        return false;
        //}

        void GetAssetBundleData(string bundleName)
        {
            bundleName = bundleName.Replace("Assets/", Application.dataPath + "/");
            mPrefabs.Clear();

            string inURL = "file://" + bundleName;
            //		Debug.LogError(inURL);
            mDownloader = new WWW(inURL);
            if (mDownloader.assetBundle != null)
            {
                Object[] allObjects = mDownloader.assetBundle.LoadAllAssets();
                if (allObjects != null)
                {
                    foreach (Object obj in allObjects)
                    {
                        ObjectInfo objInfo = new ObjectInfo();
                        objInfo._Name = obj.name;
                        string typeName = obj.GetType().ToString();
                        typeName = typeName.Replace("UnityEngine.", "");
                        typeName = typeName.Replace("UnityEditor.", "");
                        objInfo._Info = typeName + ", ";
                        if (obj is Texture)
                        {
                            Texture tex = obj as Texture;
                            objInfo._Info += tex.width.ToString() + "x" + tex.height.ToString();
                            Texture2D tex2D = obj as Texture2D;
                            if (tex2D != null)
                                objInfo._Info += ", " + tex2D.format.ToString();
                        }
                        else if (obj is Mesh)
                        {
                            Mesh msh = obj as Mesh;
                            if (msh != null)
                                objInfo._Info += "Vertices " + msh.vertexCount + ", Trianles " + msh.triangles.Length;
                        }
                        else if (obj is Material)
                        {
                            Material mat = obj as Material;
                            if (mat != null)
                                objInfo._Info += mat.shader.name;
                        }
                        else if (obj is GameObject)
                        {
                        }
                        else if (obj is MonoScript)
                        {
                        }
                        mPrefabs.Add(objInfo);
                    }
                }
                mDownloader.assetBundle.Unload(true);
            }
            else
                Debug.LogError("Bundle not compatible with the current platform.");
        }

        void OnGUI()
        {
            //Bundles
            mBundlesScrollPosition = GUI.BeginScrollView(mBundleLayoutPosition, mBundlesScrollPosition, mBundlesViewRect);
            mBundlesViewRect.height = mPrefabs.Count * 20;
            GUI.Label(new Rect(10, 5, 100, 20), "No Of Items : " + mPrefabs.Count);
            GUI.Label(new Rect(10, 10, 100, 20), "------------------------------");
            Rect r = new Rect(10, 30, 800, 16);
            int count = 1;
            foreach (ObjectInfo obj in mPrefabs)
            {
                GUI.Label(r, count + ". " + obj._Name + ", " + obj._Info);
                count++;
                r.y += r.height;
            }
            GUI.EndScrollView();
        }
    }
}