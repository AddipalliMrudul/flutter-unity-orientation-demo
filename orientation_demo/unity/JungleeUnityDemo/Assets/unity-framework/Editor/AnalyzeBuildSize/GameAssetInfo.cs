using System.Collections.Generic;

namespace JungleeGames.Editor
{
    public class GameAssetInfo
    {
        public long Size => bundledSize + downloadableSize;
        public long bundledSize;
        public long downloadableSize;
        public List<AssetInfo> bundledAssetInfo;
        public List<AssetInfo> downloadableAssetInfo;

        public GameAssetInfo()
        {
            bundledAssetInfo = new List<AssetInfo>();
            downloadableAssetInfo = new List<AssetInfo>();
        }
    }
}
