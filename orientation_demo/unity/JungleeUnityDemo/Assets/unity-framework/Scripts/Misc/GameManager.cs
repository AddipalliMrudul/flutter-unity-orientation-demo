using DG.Tweening;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class GameManager : BaseBehaviour
    {
        #region Properties
        #endregion //Properties

        #region Signals
        [InjectSignal] private SigUploadDeviceLogs mSigUploadDeviceLogs = null;
        #endregion //Signals

        #region Private Methods
        protected override void Awake()
        {
            base.Awake();

            Application.logMessageReceived += OnDebugLogCallbackHandler;
            Application.lowMemory += OnLowMemory;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DOTween.KillAll();
            Application.lowMemory -= OnLowMemory;
            Application.logMessageReceived -= OnDebugLogCallbackHandler;
        }

        protected virtual void OnLowMemory()
        {
            XDebug.LogError("OnLowMemory warning received");
        }

        protected virtual void OnDebugLogCallbackHandler(string logString, string inStack, LogType inType)
        {
            if (inType == LogType.Exception)
            {
                mSigUploadDeviceLogs.Dispatch(null, logString + inStack, null);
            }
        }
        #endregion //Private Methods
    }
}
