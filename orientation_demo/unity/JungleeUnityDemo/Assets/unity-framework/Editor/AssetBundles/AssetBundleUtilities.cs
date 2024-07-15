using UnityEditor;

namespace XcelerateGames.Editor.AssetBundles
{
    public static class AssetBundleUtilities
    {
        /// <summary>
        /// Helper function to set asset bundle name & variant
        /// </summary>
        /// <param name="assetPath">Full asset path starting from Assets filder. Ex: "Assets/Carrom/Resources"</param>
        /// <param name="bundleName"></param>
        /// <param name="variantName"></param>
        /// <returns></returns>
        public static bool SetBundleNameAndVariant(string assetPath, string bundleName = null, string variantName = null)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                if (!bundleName.IsNullOrEmpty())
                    importer.assetBundleName = bundleName;
                if (!variantName.IsNullOrEmpty())
                    importer.assetBundleVariant = variantName;

                importer.SaveAndReimport();
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath(assetPath, typeof(object)));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns Asset bundle name & variant set on the given asset
        /// </summary>
        /// <param name="assetPath">Full path of the asset</param>
        /// @example GetBundleNameAndVariant("Assets/Game/Prefabs/PfUiPlayer")
        /// <returns></returns>
        public static (string bundleName, string variantName) GetBundleNameAndVariant(string assetPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                return (importer.assetBundleName, importer.assetBundleVariant);
            }
            return (null, null);
        }

        /// <summary>
        /// Recursively search directories upwards till we get the asset that has bundle name attached.
        /// This is useful in case of assets under Resources folder.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns>a tuple of three strings. 1. The asset path on which asset bundle name was marked, 2. name of asset bundle 3. name of varaint</returns>
        public static (string assetPath, string bundleName, string variantName) GetBundleNameAndVariantRecursive(string assetPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                if (importer.assetBundleName.IsNullOrEmpty())
                    return GetBundleNameAndVariantRecursive(System.IO.Path.GetDirectoryName(assetPath));
                else
                    return (assetPath, importer.assetBundleName, importer.assetBundleVariant);
            }
            return (assetPath, null, null);
        }

        /// <summary>
        /// Helper function to split a full asset bundle name to asset bundle name & varinat
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public static (string bundleName, string variantName) SplitBundleAndVariantName(string assetBundleName)
        {
            string[] data = assetBundleName.Split('.');
            string bundleName = data[0];
            string variantName = null;
            if (data.Length == 2)
                variantName = data[1];
            return (bundleName, variantName);
        }
    }
}