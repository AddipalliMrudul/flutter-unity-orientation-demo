using UnityEngine;

namespace JungleeGames.Editor
{
    public static class JungleeEditorConfig
    {
        #region AssetMapping asset name
        public const string RummyAssetConfig = "AssetMappingRummy";
        public const string TeenPattiAssetConfig = "AssetMappingTeenPatti";
        public const string LudoAssetConfig = "AssetMappingLudo";
        public const string PokerAssetConfig = "AssetMappingPoker";
        public const string CarromAssetConfig = "AssetMappingCarrom";
        #endregion AssetMapping asset name

        public static Game GetGameTypeByAssetConfigName(string assetConfig)
        {
            if (assetConfig == RummyAssetConfig)
                return Game.Rummy;
            if (assetConfig == TeenPattiAssetConfig)
                return Game.Teenpatti;
            if (assetConfig == LudoAssetConfig)
                return Game.Ludo;
            if (assetConfig == CarromAssetConfig)
                return Game.Carrom;
            if (assetConfig == PokerAssetConfig)
                return Game.Poker;

            Debug.LogError($"Could not find game type for config asset: {assetConfig}");
            return Game.None;
        }
    }
}
