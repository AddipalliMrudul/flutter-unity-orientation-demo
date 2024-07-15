using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// This class to be used to dispatch events at framework level
    /// </summary>
    public class FrameworkEventManager : BaseBehaviour
    {
        #region Properties
        private static FrameworkEventManager mInstance = null;
        #endregion //Properties

        #region Signals
        [InjectSignal] private SigSendFrameworkEvent mSigSendFrameworkEvent = null;
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Awake()
        {
            if (mInstance == null)
            {
                base.Awake();
                mInstance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }
        #endregion //Private Methods

        #region Public Methods
        public static void LogEvent(string eventName)
        {
            XDebug.Log("framework:" + eventName, inPriority: XDebug.Priority.High);
            if (mInstance == null)
                return;
            mInstance.mSigSendFrameworkEvent.Dispatch(eventName);
        }
        #endregion //Public Methods
    }
}
