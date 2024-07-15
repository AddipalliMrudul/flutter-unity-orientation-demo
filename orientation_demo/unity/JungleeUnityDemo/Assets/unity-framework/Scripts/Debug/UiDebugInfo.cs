using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Debugging
{
    public class UiDebugInfo : BaseBehaviour, IDragHandler
    {
        public Color[] _Colors = { Color.white, Color.red, Color.green, Color.blue };
        public UiItem _Text = null;
        public int _SkipFrames = 5;
        public bool _ShowMemory = false;
        [SerializeField] private float _hudRefreshRate = 1f;
#if !LIVE_BUILD
        [SerializeField] bool _DontDestroyOnLoad = true;
#endif
        private int mIndex = 0;
        private float mTimer;
        public static int FPS { private set; get; }

        [SerializeField] protected string[] _Recipients = null;
        [SerializeField] protected string _Subject = "Debug Logs";
        [SerializeField] protected string _Message = "Debug Logs";
        [SerializeField] protected GameObject _Items = null;

        [InjectSignal] protected SigSendMailLog mSigSendMailLog = null;
        [InjectSignal] protected SigLoadAssetFromBundle mSigLoadAssetFromBundle = null;

#if UNITY_IOS || UNITY_EDITOR
        private float mMBSize = 1f / (1024f * 1024f);
#endif

        protected virtual void Start()
        {
#if LIVE_BUILD || DEMO_BUILD || BETA_BUILD
            Destroy(gameObject);
#else
            _Items?.SetActive(false);

            if (_DontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
#endif
        }

        protected virtual void Update()
        {
            if (Time.unscaledTime > mTimer)
            {
                StringBuilder data = new StringBuilder();
                data.AppendLine("FPS : " + GetFPS());

#if UNITY_IOS || UNITY_EDITOR
                if (_ShowMemory)
                    data.AppendLine("Free : " + (MobileUtilities.GetFreeMemory() * mMBSize) + " \nInUse : " + (MobileUtilities.GetMemoryInUse() * mMBSize));
#endif
                data.AppendLine("Excpt : " + LogConsole.pNumExceptions);
#if OTA_BUILD
                data.AppendLine("OTA: Yes");
#else 
                data.AppendLine("OTA: No");
#endif
                if (_Text != null)
                    _Text.text = data.ToString();
            }
        }

        private int GetFPS()
        {
            mTimer = Time.unscaledTime + _hudRefreshRate;
            FPS = (int)(1f / Time.unscaledDeltaTime);
            return FPS;
        }

        public void ChangeColor()
        {
            mIndex++;
            if (mIndex >= _Colors.Length)
                mIndex = 0;
            _Text._TextItem.color = _Colors[mIndex];
            if (UiConsole.Instance != null)
                UiConsole.Instance.Toggle();
        }

        public virtual void Show(bool show)
        {
            _Text.SetActive(show);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _Text.transform.position = Input.mousePosition;
        }

        public void Exception()
        {
            Debug.LogException(new System.Exception("Test Exception: Ignore"));
        }

        public void OnClickAddMemory(int length)
        {
#if DEV_BUILD || QA_BUILD
            length = Mathf.Clamp(length, 1, 50);
            for (int i = 0; i < length; i++)
            {
                var texture = TextureExtensions.ResizeTexture(new Texture2D(1, 1), 3890, 2190, false);
            }
#endif
        }

        public virtual void SendMail()
        {
            SendLogsViaMail(false, _Message);
        }

        public virtual void SendMailWithSS()
        {
            SendLogsViaMail(true, _Message);
        }

        public virtual void Abort()
        {
            XDebug.LogException("Exception Abort");
            if (!Application.isEditor)
                UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
        }

        public virtual void FatalError()
        {
            XDebug.LogException("Exception FatalError");
            if (!Application.isEditor)
                UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);
        }

        public virtual void PureVirtualFunction()
        {
            XDebug.LogException("Exception PureVirtualFunction");
            if (!Application.isEditor)
                UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.PureVirtualFunction);
        }

        public virtual void AccessViolation()
        {
            XDebug.LogException("Exception AccessViolation");
            if (!Application.isEditor)
                UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.AccessViolation);
        }

        public virtual void OnClickDebugMask()
        {
            mSigLoadAssetFromBundle.Dispatch("debugmask/PfUiDebugMask", null, false, 0, null);
        }

        public virtual void OnClickApiSimulator()
        {
            mSigLoadAssetFromBundle.Dispatch("apisimulator/PfUiAPISimulator", null, false, 0, null);
        }

        public void OnClickItems()
        {
            _Items.SetActive(!_Items.activeSelf);
        }

        protected virtual void SendLogsViaMail(bool takeScreenShot, string message)
        {
            mSigSendMailLog.Dispatch(takeScreenShot, (_Recipients.Printable(','), $"{_Subject}: {ProductSettings.GetVersionInfo()}", _Message));
        }
    }
}