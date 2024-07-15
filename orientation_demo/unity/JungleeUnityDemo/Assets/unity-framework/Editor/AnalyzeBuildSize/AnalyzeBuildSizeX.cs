using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.AssetLoading;
using XcelerateGames.Editor;
using XcelerateGames.Editor.Build;

namespace JungleeGames.Editor
{
    public class AnalyzeBuildSizeX : EditorWindow
    {
        protected string FileName { get { return ResourceManager.BundledAssetsAssetName + ".json"; } }
        //This list contains the name of asset bundles that must be embedded in the app
        protected List<string> MustEmbedAssetBundleds;

        protected Dictionary<string, List<string>> mAssetMappingByGame = new Dictionary<string, List<string>>();
        protected Dictionary<string, string> mShippedAssets = null;

        protected List<string> mPrefetchList = null;

        protected Dictionary<Game, GameAssetInfo> mAssetInfo = new Dictionary<Game, GameAssetInfo>();
        protected Vector2 mScroll = Vector2.zero;
        protected bool mReloadData = false;

        protected AssetBundleManifest mAssetBundleManifest = null;

        // [MenuItem("Game/TeenPatti/Analyze Build Size")]
        // protected static void CreateWizard()
        // {
        //     AnalyzeBuildSizeX obj = Create();
        //     obj.titleContent.text = "Analyze Build Size";
        //     obj.OnWizardCreate();
        // }

        // protected static AnalyzeBuildSizeX Create()
        // {
        //     return GetWindow<AnalyzeBuildSizeX>();
        // }

        protected virtual void OnWizardCreate()
        {
            mReloadData = false;
            mShippedAssets = ResourceManager.GetDefaultVersionList();
            mPrefetchList = ShipAssetWithApp.GetSecondaryPrefetchList();

            AddBundlesToEmbed();

            mAssetBundleManifest = EditorUtilities.LoadManifest();

            LoadAssetConfig();
        }

        /// <summary>
        /// Add the asset bundles that must be embedded in the app
        /// </summary>
        protected virtual void AddBundlesToEmbed()
        {
            MustEmbedAssetBundleds = new List<string>
            {
                "resources",
                "sounds",
                "fonts"
            };
        }

        protected virtual bool MustBeEmbedded(string assetName)
        {
            return MustEmbedAssetBundleds.Contains(assetName);
        }

        protected virtual string[] GetGameSpecificAssetNames()
        {
            //This function should be overridden in the child class
            return null;
        }

        protected void LoadAssetConfig()
        {
            mAssetMappingByGame.Clear();
            mAssetInfo.Clear();

            string[] gameSpecificAssetNames = GetGameSpecificAssetNames();
            foreach (string assetName in gameSpecificAssetNames)
            {
                AssetConfigData assetConfigData = ResourceManager.LoadFromResources<AssetConfigData>(assetName);
                mAssetMappingByGame.Add(assetName, assetConfigData.GetAllAssetPaths());
            }

            AssetBundleManifest assetBundleManifest = EditorUtilities.LoadManifest();
            List<string> assets = assetBundleManifest.GetAllAssetBundles().ToList();
            //Remove assets that are to be ignored
            assets.RemoveAll(asset => BuildAppSettings.pInstance.AssetBundleToIgnore.Contains(asset));

            foreach (string asset in assets)
            {
                string gameName = GetGameNameByAssetName(asset);
                if (!gameName.IsNullOrEmpty())
                {
                    Game gameType = JungleeEditorConfig.GetGameTypeByAssetConfigName(gameName);
                    FileInfo fInfo = new FileInfo(EditorUtilities.mAssetsDir + asset);
                    if (!mAssetInfo.ContainsKey(gameType))
                        mAssetInfo.Add(gameType, new GameAssetInfo());
                    bool bundledInApp = IsBundledInApp(asset);
                    AssetInfo assetInfo = new AssetInfo(asset, fInfo.Length, bundledInApp, assetBundleManifest.GetAssetBundleHash(asset).ToString(), gameName) { };
                    if (bundledInApp)
                    {
                        List<string> dependentAssetBundles = new List<string>();
                        if (HasDepenedentAssetBundlesDownloads(assetInfo._Name, gameType, ref dependentAssetBundles))
                        {
                            assetInfo._HasDownloadableDependencies = true;
                        }

                        mAssetInfo[gameType].bundledSize += fInfo.Length;
                        mAssetInfo[gameType].bundledAssetInfo.Add(assetInfo);
                    }
                    else
                    {
                        mAssetInfo[gameType].downloadableSize += fInfo.Length;
                        mAssetInfo[gameType].downloadableAssetInfo.Add(assetInfo);
                    }
                }
            }

            //Sort by size
            foreach (KeyValuePair<Game, GameAssetInfo> kvp in mAssetInfo)
            {
                kvp.Value.bundledAssetInfo.Sort();
                kvp.Value.downloadableAssetInfo.Sort();
            }
        }

        protected string GetGameNameByAssetName(string assetName)
        {
            assetName = Path.GetFileNameWithoutExtension(assetName);
            foreach (KeyValuePair<string, List<string>> kvp in mAssetMappingByGame)
            {
                foreach (string asset in kvp.Value)
                {
                    (string bundleName, string assetName) data = ResourceManager.SplitAssetName(asset);
                    if (assetName.Equals(data.bundleName))
                    {
                        return kvp.Key;
                    }
                }
            }
            Debug.LogWarning($"Could not find game name for asset: {assetName}");
            return null;
        }

        protected AssetInfo GetAssetInfoByBundleName(string bundleName)
        {
            foreach (KeyValuePair<Game, GameAssetInfo> kvp in mAssetInfo)
            {
                foreach (AssetInfo assetInfo in kvp.Value.bundledAssetInfo)
                {
                    if (assetInfo._Name == bundleName)
                        return assetInfo;
                }
            }
            Debug.LogError($"Could not find AssetInfo for bundle name: {bundleName}");
            return null;
        }

        protected bool IsBundledInApp(string bundleName)
        {
            return mShippedAssets.ContainsKey(bundleName);
        }

        protected long GetBundledAssetsSize()
        {
            long size = 0;
            foreach (KeyValuePair<Game, GameAssetInfo> kvp in mAssetInfo)
            {
                foreach (AssetInfo assetInfo in kvp.Value.bundledAssetInfo)
                {
                    if (assetInfo._BundledInApp)
                        size += assetInfo._Size;
                }
            }
            return size;
        }

        protected void OnGUI()
        {
            mScroll = GUILayout.BeginScrollView(mScroll);
            GUIColor.Push(Color.cyan);
            GUILayout.Label("App Info", EditorStyles.boldLabel, new GUILayoutOption[0] { });
            GUIColor.Pop();

            (long totalFiles, long totalSize, long totalBundledSize, long totalDownloadableSize) info = GetTotalSizeInfo();

            GUIColor.Push(Color.green);
            GUILayout.Label($"\tFiles: {info.totalFiles}");
            GUILayout.Label($"\tTotal Size: {EditorUtility.FormatBytes(info.totalSize)}");
            GUILayout.Label($"\tBundled : {EditorUtility.FormatBytes(info.totalBundledSize)}");
            GUILayout.Label($"\tDownloadable : {EditorUtility.FormatBytes(info.totalDownloadableSize)}");
            GUIColor.Pop();

            foreach (KeyValuePair<Game, GameAssetInfo> kvp in mAssetInfo)
            {
                GUIColor.Push(Color.yellow);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{kvp.Key}");
                if (GUILayout.Button("Embed All"))
                {
                    EmbedAllAssets(kvp.Key);
                }
                if (GUILayout.Button("Download All"))
                {
                    DownloadAllAssets(kvp.Key);
                }
                GUILayout.EndHorizontal();

                GUIColor.Pop();
                GUIColor.Push(Color.green);
                GUILayout.Label($"\tFiles: {kvp.Value.bundledAssetInfo.Count + kvp.Value.downloadableAssetInfo.Count}");
                GUILayout.Label($"\tTotal Size: {EditorUtility.FormatBytes(kvp.Value.Size)}");
                GUILayout.Label($"\tBundled : {EditorUtility.FormatBytes(kvp.Value.bundledSize)}");
                GUILayout.Label($"\tDownloadable : {EditorUtility.FormatBytes(kvp.Value.downloadableSize)}");
                GUIColor.Pop();

                DrawAssetInfo("Bundled Assets", kvp.Value.bundledAssetInfo, true, kvp.Key);
                DrawAssetInfo("Downloadable Assets", kvp.Value.downloadableAssetInfo, false, kvp.Key);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Export as CSV");
            if (GUILayout.Button("Export"))
            {
                string path = EditorUtility.SaveFilePanel("Build Size Analysis", string.Empty, FileUtilities.GetTimeStamp("BuildSizeAnalysis.csv"), "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    Export(path);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            if (mReloadData)
                OnWizardCreate();
        }

        protected void EmbedAllAssets(Game game)
        {
            foreach (AssetInfo assetInfo in mAssetInfo[game].downloadableAssetInfo)
            {
                if (!mShippedAssets.ContainsKey(assetInfo._Name))
                {
                    mShippedAssets.Add(assetInfo._Name, assetInfo._Hash);
                }
                mPrefetchList.Remove(assetInfo._Name);
            }
            Save(mShippedAssets);
        }

        protected (long totalFiles, long totalSize, long totalBundledSize, long totalDownloadableSize) GetTotalSizeInfo()
        {
            long totalFiles = 0, totalSize = 0, totalBundledSize = 0, totalDownloadableSize = 0;
            //Add all combined info here
            foreach (KeyValuePair<Game, GameAssetInfo> kvp in mAssetInfo)
            {
                totalFiles += kvp.Value.bundledAssetInfo.Count + kvp.Value.downloadableAssetInfo.Count;
                totalSize += kvp.Value.Size;
                totalBundledSize += kvp.Value.bundledSize;
                totalDownloadableSize += kvp.Value.downloadableSize;
            }
            return (totalFiles, totalSize, totalBundledSize, totalDownloadableSize);
        }

        protected void DownloadAllAssets(Game game)
        {
            foreach (AssetInfo assetInfo in mAssetInfo[game].bundledAssetInfo)
            {
                if (mShippedAssets.ContainsKey(assetInfo._Name))
                {
                    mShippedAssets.Remove(assetInfo._Name);
                }
                mPrefetchList.AddItemOnce(assetInfo._Name);
            }
            Save(mShippedAssets);
        }

        protected void Save(Dictionary<string, string> assetsToEmbed)
        {
            ShipAssetWithApp.SaveSecondaryPrefetch(mPrefetchList);

            string filePath = "Assets/Resources/" + FileName;
            File.WriteAllText(filePath, assetsToEmbed.ToJson());
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"Total {assetsToEmbed.Count} assets will be embedded in the app.");

            //Reload all data
            mReloadData = true;
        }

        protected void Export(string filePath)
        {
            List<string> data = new List<string>();

            (long totalFiles, long totalSize, long totalBundledSize, long totalDownloadableSize) info = GetTotalSizeInfo();

            data.Add($"Date & Time, {DateTime.Now.ToString()}");
            data.Add($"Files, {info.totalFiles}");
            data.Add($"Total Size, {EditorUtility.FormatBytes(info.totalSize)}");
            data.Add($"Bundled, {EditorUtility.FormatBytes(info.totalBundledSize)}");
            data.Add($"Downloadable, {EditorUtility.FormatBytes(info.totalDownloadableSize)}");
            data.Add("");

            foreach (KeyValuePair<Game, GameAssetInfo> kvp in mAssetInfo)
            {
                data.Add($"{kvp.Key}");
                data.Add($",Files, {kvp.Value.bundledAssetInfo.Count + kvp.Value.downloadableAssetInfo.Count}");
                data.Add($",Total Size, {EditorUtility.FormatBytes(kvp.Value.Size)}");
                data.Add($",Bundled, {EditorUtility.FormatBytes(kvp.Value.bundledSize)}");
                data.Add($",Downloadable, {EditorUtility.FormatBytes(kvp.Value.downloadableSize)}");

                data.Add($"\n,Bundled Assets, {kvp.Value.bundledAssetInfo.Count}");

                foreach (AssetInfo assetInfo in kvp.Value.bundledAssetInfo)
                {
                    data.Add($",{assetInfo._Name},{EditorUtility.FormatBytes(assetInfo._Size)}");
                }
                data.Add($"\n,Downloadable Assets, {kvp.Value.downloadableAssetInfo.Count}");

                foreach (AssetInfo assetInfo in kvp.Value.downloadableAssetInfo)
                {
                    data.Add($",{assetInfo._Name},{EditorUtility.FormatBytes(assetInfo._Size)}");
                }
                data.Add("");
            }

            File.WriteAllLines(filePath, data.ToArray());
        }

        protected void DrawAssetInfo(string title, List<AssetInfo> assetInfos, bool isEmbedded, Game game)
        {
            GUIColor.Push(Color.yellow);
            GUILayout.Label(title, EditorStyles.boldLabel, new GUILayoutOption[0] { });
            GUIColor.Pop();
            //GUIColor.Push(Color.green);

            int index = 1;
            string buttonText = isEmbedded ? "Download" : "Embed";
            foreach (AssetInfo assetInfo in assetInfos)
            {
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                bool temp = assetInfo._HasDownloadableDependencies;
                if (temp)
                    GUIColor.Push(Color.red);
                GUILayout.Label(index++ + ".", GUILayout.Width(30f));
                GUILayout.Button(assetInfo._Name, "TextField", GUILayout.Height(20f));
                GUILayout.Label(EditorUtility.FormatBytes(assetInfo._Size), GUILayout.Width(80));

                if (GUILayout.Button(buttonText, GUILayout.Width(100)))
                {
                    if (isEmbedded)
                    {
                        //The asset was embedded, so download it
                        if (MustBeEmbedded(assetInfo._Name))
                        {
                            Debug.LogError($"Cannot move \"{assetInfo._Name}\" to ODD. It is a must embed AssetBundle.");
                            EditorUtility.DisplayDialog("Error", $"Cannot move \"{assetInfo._Name}\" to ODD. It is a must embed AssetBundle.", "Ok");
                        }
                        else
                        {
                            mShippedAssets.Remove(assetInfo._Name);
                            mPrefetchList.AddItemOnce(assetInfo._Name);
                        }
                    }
                    else
                    {
                        //The asset was to be downloaded, so embed it
                        List<string> dependentAssetBundles = new List<string>();
                        if (HasDepenedentAssetBundlesDownloads(assetInfo._Name, game, ref dependentAssetBundles))
                        {
                            assetInfo._HasDownloadableDependencies = true;
                            string message = $" \"{assetInfo._Name}\" has dependent AssetBundles that are to be downloaded\n {dependentAssetBundles.Printable()}.";
                            Debug.LogWarning(message);
                            EditorUtility.DisplayDialog("Warning", $"\"{assetInfo._Name}\" has dependencies. See console for more details", "Ok");
                        }
                        // else
                        //     assetInfo.HasDownloadableDependencies = false;

                        mShippedAssets.Add(assetInfo._Name, assetInfo._Hash);
                        mPrefetchList.Remove(assetInfo._Name);
                    }

                    Save(mShippedAssets);
                }
                if (temp)
                    GUIColor.Pop();

                GUILayout.EndHorizontal();
            }
            //GUIColor.Pop();
        }

        /// <summary>
        /// Checks if the specified asset bundle that is being embedded has any dependent asset bundles that need to be downloaded.
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle.</param>
        /// <param name="game">The game for which the asset bundle is being checked.</param>
        /// <param name="dependentAssetBundles">A reference to a list that will contain the names of the dependent asset bundles.</param>
        /// <returns>True if the asset bundle has dependent asset bundles that need to be downloaded, false otherwise.</returns>
        protected virtual bool HasDepenedentAssetBundlesDownloads(string bundleName, Game game, ref List<string> dependentAssetBundles)
        {
            string[] dependencies = mAssetBundleManifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                foreach (AssetInfo assetInfo in mAssetInfo[game].downloadableAssetInfo)
                {
                    if (assetInfo._Name == dependency)
                    {
                        dependentAssetBundles.Add(assetInfo._Name);
                    }
                }
            }
            return dependentAssetBundles.Count > 0;
        }
    }
}