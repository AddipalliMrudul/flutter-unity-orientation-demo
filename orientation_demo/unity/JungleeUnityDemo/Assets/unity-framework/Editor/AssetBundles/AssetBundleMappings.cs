using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.AssetBundles
{
    /// <summary>
    /// 
    /// </summary>
    public class AssetBundleMappings : ScriptableWizard
    {
        private static Dictionary<string, List<string>> mAssetBundleMapping = new Dictionary<string, List<string>>();
        private const string mMenuCommand = BuildAssetBundle.AssetBundleMenu + "Asset Bundle Mappings/";
        private const string mFileName = "Assets/AssetBundleMappings.json";

        [MenuItem(mMenuCommand + "Get", false, 30)]
        public static string GetAssetBundleMappings()
        {
            //Build the asset bundles first
            BuildAssetBundle.CreateAssetBundleFromAssetDatabase();

            mAssetBundleMapping.Clear();
            AssetBundleManifest assetBundleManifest = EditorUtilities.LoadManifest();
            List<string> assetBundles = new List<string>(assetBundleManifest.GetAllAssetBundles());
            foreach (string assetBundle in assetBundles)
            {
                string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundle);
                List<string> bundleSource = new List<string>();
                foreach (string asset in assets)
                {
                    (string assetPath, string bundleName, string variantName) currentInfo = AssetBundleUtilities.GetBundleNameAndVariantRecursive(asset);
                    bundleSource.AddItemOnce(currentInfo.assetPath);
                    Debug.Log($"{asset} -> {currentInfo.bundleName} {currentInfo.variantName}");
                }
                mAssetBundleMapping.Add(assetBundle, bundleSource);
            }

            File.WriteAllText(mFileName, mAssetBundleMapping.ToJson());
            AssetDatabase.ImportAsset(mFileName);
            return mFileName;
        }

        [MenuItem(mMenuCommand + "Set", false, 31)]
        public static void SetAssetBundleMappings()
        {
            mAssetBundleMapping = File.ReadAllText(mFileName).FromJson<Dictionary<string, List<string>>>();
            List<string> updatedAssets = new List<string>();
            foreach (KeyValuePair<string, List<string>> kvp in mAssetBundleMapping)
            {
                if (kvp.Value == null || kvp.Value.Count == 0)
                    continue;

                foreach (string assetPath in kvp.Value)
                {
                    (string bundleName, string variantName) info = AssetBundleUtilities.SplitBundleAndVariantName(kvp.Key);
                    (string bundleName, string variantName) currentInfo = AssetBundleUtilities.GetBundleNameAndVariant(assetPath);
                    if ((info.bundleName != null && !info.bundleName.Equals(currentInfo.bundleName)) ||
                        (info.variantName != null && !info.variantName.Equals(currentInfo.variantName)))
                    {
                        AssetBundleUtilities.SetBundleNameAndVariant(assetPath, info.bundleName, info.variantName);
                        //Debug.Log($"Updated assetbundle name & variant for {assetPath}");
                        string data = $"{assetPath} added Bundle Name: {info.bundleName}";
                        if (!info.variantName.IsNullOrEmpty())
                            data += $", Variant Name: {info.variantName}";
                        updatedAssets.Add(data);
                    }
                    //else
                    //    Debug.Log($"Not updating assetbundle name & variant for {assetPath}");
                }
            }
            Debug.Log($"Updated assetbundle name & variant for, \n{updatedAssets.Printable()}");
        }

        [MenuItem(mMenuCommand + "Set", true, 31)]
        static bool ValidateSetAssetBundleMappings()
        {
            if (Selection.activeObject != null)
            {
                return AssetDatabase.GetAssetPath(Selection.activeObject).Equals(mFileName);
            }
            return false;
        }
    }
}