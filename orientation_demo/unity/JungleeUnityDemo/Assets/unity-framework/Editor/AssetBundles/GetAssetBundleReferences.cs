using XcelerateGames;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author: Altaf
/// Date:May 16, 2017
/// Purpose: To get references to the selected asset bundles.
/// </summary>
namespace XcelerateGames.Editor.AssetBundles
{
    public class GetAssetBundleReferences : EditorWindow
    {
        private AssetBundleManifest mAssetBundleManifest = null;
        private Dictionary<string, List<string>> mDependenciesMap = null;

        private Vector2 mScroll = Vector2.zero;

        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Get References", false, 35)]
        private static void DoGetAssetBundleReferences()
        {
            GetWindow<GetAssetBundleReferences>().titleContent.text = "References";
        }

        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Get References", true, 35)]
        static bool ValidateDoGetAssetBundleReferences()
        {
            if (Selection.activeObject == null)
                return false;
            //foreach (Object obj in Selection.objects)
            //{
            //    if (!AssetDatabase.GetAssetPath(obj).EndsWith("unity3d"))
            //        return false;
            //}
            return true;
        }

        private void Awake()
        {
            mAssetBundleManifest = EditorUtilities.LoadManifest();
            mDependenciesMap = new Dictionary<string, List<string>>();
            foreach (Object obj in Selection.objects)
            {
                string bundleName = Path.GetFileName(AssetDatabase.GetAssetPath(obj));
                mDependenciesMap.Add(bundleName, GetReferences(bundleName));
            }
        }

        private List<string> GetReferences(string bundleName)
        {
            List<string> references = new List<string>();

            string[] allAssetBundles = mAssetBundleManifest.GetAllAssetBundles();
            foreach (string ab in allAssetBundles)
            {
                if (ab == bundleName)
                    continue;

                string[] dependencies = mAssetBundleManifest.GetAllDependencies(ab);
                foreach (string dep in dependencies)
                {
                    if (dep == bundleName)
                        references.Add(ab);
                }
            }

            return references;
        }

        private void OnGUI()
        {
            if (mDependenciesMap == null)
                return;
            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach (string bundleName in mDependenciesMap.Keys)
            {
                GUILayout.BeginVertical();
                int count = 1;
                EditorGUILayout.LabelField("References of " + bundleName, EditorStyles.boldLabel);
                foreach (string ab in mDependenciesMap[bundleName])
                {
                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                    GUILayout.Label(count++ + ".", GUILayout.Width(30f));
                    GUILayout.Label("\t");
                    GUILayout.Button(ab, "OL TextField", GUILayout.Height(20f));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
    }
}