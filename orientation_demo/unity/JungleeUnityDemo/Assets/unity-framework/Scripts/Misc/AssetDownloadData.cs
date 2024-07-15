namespace XcelerateGames
{
    public class AssetDownloadData
    {
        public int totalCount;
        public int downloadedCount;
        public double downloadDuration;
        public AssetDownloadingState downloadingState;
    }

    public enum AssetDownloadingState
    {
        Waiting,
        Started,
        Completed,
        PopupClosed
    }
}
