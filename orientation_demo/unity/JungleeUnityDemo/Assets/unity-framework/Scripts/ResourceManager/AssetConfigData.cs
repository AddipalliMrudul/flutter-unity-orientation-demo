using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace XcelerateGames.AssetLoading
{
    /// <summary>
    /// DataStructure to manage asset id & its mapping
    /// </summary>
    [Serializable]
    public class AssetConfig
    {
        public string Name;
        public string AssetName = null;

        public override string ToString()
        {
            return $"{Name},{AssetName}";
        }
    }

    /// <summary>
    /// Asset for mapping asset to an ID. This is used to load assets by ID & also list all assets being loaded in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "AssetMapping", menuName = Utilities.MenuName + "Asset Mapping")]
    public class AssetConfigData : ScriptableObject
    {
        public string moduleName; /*<Name of the module */

        // HideInInspector to hide the Serialized Field in Inspector
        [HideInInspector, SerializeField] private List<AssetConfig> Configs = null;  /*<List of all mappings */

        private static AssetConfigData mInstance = null;            /*<Static instance of the class */
    
        /// <summary>
        /// Initialise the mappings. Pass the asset name to load & initialise the mappings. 
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static bool Init(string assetName)
        {
            if (XDebug.CanLog(XDebug.Mask.Resources))
                Debug.Log($"Initializing AssetConfigData with {assetName}");
            mInstance = ResourceManager.LoadFromResources<AssetConfigData>(assetName);
            if (mInstance == null)
                XDebug.LogException($"Failed to load AssetMapping asset \"{assetName}\"");
            return mInstance != null;
        }

        /// <summary>
        /// Get the asset name by id.
        /// </summary>
        /// <param name="name">id of the asset</param>
        /// <returns></returns>
        public static string GetAssetName(string name)
        {
            if (mInstance == null)
            {
                XDebug.LogException("AssetMapping not initialised, Call Init");
                return null;
            }

            AssetConfig assetConfig = mInstance.Configs.Find(e => name.Equals(e.Name));
            if (assetConfig != null)
            {
                return assetConfig.AssetName;
            }
            XDebug.LogError($"Failed to find AssetConfig for {name}");
            return null;
        }

        /// <summary>
        /// Returns a list of all asset paths added in the mapping
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllAssetPaths()
        {
            List<string> allAssets = new List<string>();
            foreach (AssetConfig config in Configs)
                allAssets.Add(config.AssetName);
            return allAssets;
        }

        /// <summary>
        /// This function is used to populate the list with common assets when its first created
        /// </summary>
        private void OnEnable()
        {
            if (Configs == null)
                Configs = new List<AssetConfig>();
            if (Configs.Count == 0)
            {
                Debug.Log("New asset created or is empty, populating framework level assets");
                Configs.Add(new AssetConfig() { Name = "tutorialMgr", AssetName = "tutorials/PfUiTutorialManager" });
            }
        }

#if UNITY_EDITOR

        public bool ContainsAssetName(string assetName)
        {
            return Configs.Find(item => item.AssetName == assetName) != null;
        }

        public bool ContainsName(string bundleName)
        {
            return Configs.Find(item => item.Name == bundleName) != null;
        }

        public void PopulateList()
        {
            foreach (var item in pConfigs)
            {
                string assetName = System.IO.Path.GetFileNameWithoutExtension(item.AssetName);
                if (!ContainsAssetName(assetName))
                    Configs.Add(new AssetConfig() { Name = assetName, AssetName = assetName });
            }
        }

        public void OnValidate()
        {
            if (moduleName.IsNullOrEmpty())
            {
                moduleName = name.Replace("AssetMapping", "");
            }
        }

        public List<AssetConfig> GetConfigs()
        {
            return Configs;
        }

        public void BackUp()
        {
            Assets = Configs;
        }

        public void Searching(string searchItem)
        {
            if (searchItem.IsNullOrEmpty())
            {
                Assets = Configs;
                return;
            }

            var configs = Configs.FindAll(x => x.Name.Contains(searchItem) || x.AssetName.Contains(searchItem));
            Assets = configs;
        }

        public List<AssetConfig> pConfigs { get; set; }
        [SerializeField] private List<AssetConfig> Assets = null;
#endif
    }
}
