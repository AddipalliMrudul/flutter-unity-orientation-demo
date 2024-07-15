namespace XcelerateGames
{
    /// <summary>
    /// Interface to notify that the module in querstion is ready
    /// </summary>
    public interface IWaitList
    {
        bool IsReady();
    }

    /// <summary>
    /// Attach this script to any GameObject that is doing any async process ex: load player data, API etc
    /// </summary>
    public class WaitList : BaseBehaviour, IWaitList
    {
        [System.NonSerialized] public bool _IsReady = false; /**< Set this to true to mark process complete. Loading screen will not be destroyed intil this is true*/

        /// <summary>
        /// Interface to notify if we are ready
        /// </summary>
        public virtual bool IsReady()
        {
            return _IsReady;
        }
    }
}