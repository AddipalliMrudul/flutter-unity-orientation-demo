using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor
{
    public static class AssetConfigDataHelper
    {
        public static string GetModuleNameByName(string bundleName)
        {
            AssetConfigData[] assetConfigs = Resources.LoadAll<AssetConfigData>(string.Empty);
            if (assetConfigs == null)
            {
                Debug.LogError("AssetConfigDataHelper.GetModuleNameByAssetName: AssetConfigData not found");
                return null;
            }
            foreach (AssetConfigData assetConfigData in assetConfigs)
            {
                if (assetConfigData.ContainsName(bundleName))
                {
                    return assetConfigData.moduleName;
                }
            }

            return null;
        }
    }
}
