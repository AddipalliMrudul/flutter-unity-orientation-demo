using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    public class UiInputBlocker : BaseBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] protected bool _ShowLog = false;
#endif
        protected int mRefCount = 0;

        protected static UiInputBlocker mInstance = null;
        protected float mDuration = 0f;
        //Easy option would have been to enable/disable the script, but we have a derived class & that requires it to be enabled all the time. Hence the variable
        protected bool mTimerRunning = false;

        public virtual void Init() { }

        /// <summary>
        /// Shows a exclusive/modal loading cursor. Every call to show(true) will increase the refcount & every call to Show(false) will reduce the ref count.
        /// When the ref count is zero, the UI will be hidden.
        /// </summary>
        /// <param name="inShow"></param>
        public static void Show(bool inShow)
        {
            if (mInstance == null)
                CreateInstance();
            if (inShow)
            {
                mInstance.mRefCount++;
                mInstance.gameObject.SetActive(true);
            }
            else
            {
                mInstance.mRefCount--;
                if (mInstance.mRefCount <= 0)
                {
                    mInstance.mRefCount = 0;
                    mInstance.gameObject.SetActive(false);
                }
            }
            mInstance.ShowInputBlocker(inShow);
#if UNITY_EDITOR
            if (mInstance._ShowLog)
                Debug.Log($"InputBlocker? {inShow} : refCount: {mInstance.mRefCount}");
#endif
        }

        /// <summary>
        /// Blocks input for the given duration
        /// </summary>
        /// <param name="duration"></param>
        public static void BlockInput(float duration)
        {
            mInstance?.PreInvokeBlockInput(duration);
            if (!IsVisible())
                Show(true);
            mInstance.mDuration = duration;
            mInstance.mTimerRunning = true;
        }

        /// <summary>
        /// Triggered when Block Input has been triggered from code before it's blocking input
        /// </summary>
        /// <param name="duration"></param>
        public virtual void PreInvokeBlockInput(float duration){}

        /// <summary>
        /// Returs true if the cursor is visible else false.
        /// </summary>
        /// <returns></returns>
        public static bool IsVisible()
        {
            if (mInstance == null)
                return false;
            return mInstance.mRefCount > 0;
        }

        /// <summary>
        /// Creates the static instance of the exclusive loading cursor.
        /// </summary>
        protected static void CreateInstance()
        {
            GameObject obj = ResourceManager.LoadFromResources("PfUiInputBlocker") as GameObject;
            obj = Instantiate(obj) as GameObject;
            obj.name = "PfUiInputBlocker";
            mInstance = obj.GetComponent<UiInputBlocker>();
            DontDestroyOnLoad(obj);
        }

        [ContextMenu("Reset State")]
        protected void ResetState()
        {
            mInstance.mRefCount = 0;
            Show(false);
        }

        protected virtual void ShowInputBlocker(bool inShow)
        {

        }

        protected virtual void Update()
        {
            if(mTimerRunning)
            {
                mDuration -= Time.deltaTime;
                if (mDuration <= 0f)
                {
                    Show(false);
                    mTimerRunning = false;
                }
            }
        }
    }
}