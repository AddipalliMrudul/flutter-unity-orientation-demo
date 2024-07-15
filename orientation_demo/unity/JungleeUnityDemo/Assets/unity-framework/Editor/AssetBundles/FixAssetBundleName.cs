using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.AssetBundles
{
    public class FixAssetBundleName : EditorWindow
    {
        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Fix Names", false, 32)]
        private static void DoSetAssetBundleName()
        {
            GetWindow<FixAssetBundleName>();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Add .Unity3d Extn"))
                Rename(true);
            else if (GUILayout.Button("Remove .Unity3d Extn"))
                Rename(false);
        }

        private void Rename(bool addExtn)
        {
            foreach (Object obj in Selection.objects)
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
                if (string.IsNullOrEmpty(importer.assetBundleName))
                    continue;
                string bundleName = null;
                if (addExtn)
                {
                    if (!importer.assetBundleName.EndsWith("unity3d"))
                        bundleName = importer.assetBundleName + ".unity3d";
                    else
                        continue;
                }
                else
                {
                    if (importer.assetBundleName.EndsWith("unity3d"))
                        bundleName = System.IO.Path.GetFileNameWithoutExtension(importer.assetBundleName);
                    else
                        continue;
                }
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(obj);
            }
        }
    }
}