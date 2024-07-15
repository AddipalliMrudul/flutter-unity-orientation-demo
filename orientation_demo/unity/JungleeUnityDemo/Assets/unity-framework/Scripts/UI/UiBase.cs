using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.Audio;
using XcelerateGames.IOC;
using XcelerateGames.UI.Animations;

namespace XcelerateGames.UI
{
    [DisallowMultipleComponent]
    public class UiBase : BaseBehaviour
    {
        public UiItem _BackBtn = null;
        public RectTransform _Panel = null;

        public AudioVars _CloseSFX, _OpenSFX = null;

        public bool _DestroyOnHide = false;
        public bool _CanSetExclusive = false;
        //If enabled, sorting order of this UI will be set to +2 of max sorting order
        public bool _PushOnTop = false;

        [Header("Animations")]
        public string _InAnimation = "In";
        public string _OutAnimation = "Out";
        public string ID
        {
            get
            {
                return _ID;
            }
            private set
            {
                _ID = value;
            }
        }

        #region Delegates

        public System.Action<UiBase> OnShown = null;
        public System.Action<UiBase> OnHidden = null;

        #endregion Delegates

        #region Signals
        [InjectSignal] private SigSendBreadcrumbEvent mSigSendBreadcrumbEvent = null;
        #endregion Signals

        protected List<UiAnim> mAnims = null;
        protected static Dictionary<UiBase, int> mSortOrders = new Dictionary<UiBase, int>();

        int mAnimInCompleteCount = 0;
        int mAnimOutCompleteCount = 0;
        int mTotalAnimInCount = 0;
        int mTotalAnimOutCount = 0;

        private static bool mDisableBackButton = false;
        //This was written specifically for Flutter builds as we do not get Escape key event in flutter builds
        public static bool BackButtonPressed = false;

        private static List<UiBase> mExclusiveUIList = new List<UiBase>();
        //Last executed frame number
        private static int mLastExecutedFrameNumber = 0;

        private static UiBase mExclusiveUI = null;
        private static UiItem mMouseOverItem = null;
        [SerializeField]
        private string _ID = null;

        public static UiItem pMouseOverItem
        {
            get { return mMouseOverItem; }
            set { mMouseOverItem = value; }
        }

        public static UiBase pExclusiveItem
        {
            get
            {
                if (mExclusiveUIList.Count == 0)
                    return null;
                return mExclusiveUIList[mExclusiveUIList.Count - 1];
            }
        }

        public static bool pIsAnyExclusiveUiActive
        {
            get { return mExclusiveUIList.Count != 0; }
        }

        public static bool DisableBackButton => mDisableBackButton;

        [ContextMenu("Get References")]
        private void GetReferences()
        {
            if (_Panel == null)
            {
                Transform panel = transform.Find("Panel");
                if (panel != null)
                    _Panel = panel.GetComponent<RectTransform>();
                if (_Panel != null)
                {
                    UiAnim uiAnim = _Panel.GetComponent<UiAnim>();
                    if (uiAnim != null)
                    {
                        UiAnimBase animBase = uiAnim._Anims.Find(e => e._Category == UiAnimBase.Category.In);
                        if (animBase != null)
                            _InAnimation = animBase._Name;
                        animBase = uiAnim._Anims.Find(e => e._Category == UiAnimBase.Category.Out);
                        if (animBase != null)
                            _OutAnimation = animBase._Name;
                    }
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            GetReferences();
        }
#endif //UNITY_EDITOR

        protected override void Awake()
        {
            base.Awake();
            SendBreadcrumbEvent(BreadcrumbEventContants.UiInstantiated);
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
                mSortOrders.Add(this, canvas.sortingOrder);
            GetReferences();
            if (_Panel != null)
            {
                UiAnim anim = _Panel.GetComponent<UiAnim>();
                if (anim != null)
                    anim._OnAnimationDone.AddListener(OnAnimEnd);
            }
            CacheAnims();

            if (XDebug.CanLog(XDebug.Mask.UI))
                Debug.Log($"Created->{gameObject.GetObjectPath()}");
        }

        protected virtual void SendBreadcrumbEvent(string eventName)
        {
            mSigSendBreadcrumbEvent?.Dispatch(eventName, new Dictionary<string, object> {
                {BreadcrumbEventContants.ClassName ,this.GetType().Name },
                {BreadcrumbEventContants.GameObjectName, gameObject.name },
            });
        }

        [ExecuteInEditMode]
        public void UpdateID(string ID)
        {
            _ID = ID;
        }

#if AUTOMATION_ENABLED
        private void RegisterID()
        {
            if (!string.IsNullOrEmpty(ID))
            {
                IDMapper.RegisterID(ID, gameObject.GetObjectPath());
            }
        }
#endif

        void OnTransformParentChanged()
        {
#if AUTOMATION_ENABLED
            if (!string.IsNullOrEmpty(ID))
                IDMapper.ChangeIDPath(ID, gameObject.GetObjectPath());
#endif
        }
        private void CacheAnims()
        {
            mTotalAnimInCount = 0;
            mTotalAnimOutCount = 0;
            mAnims = new List<UiAnim>(GetComponentsInChildren<UiAnim>(true));

            foreach (UiAnim anim in mAnims)
            {
                mTotalAnimInCount += anim._Anims.FindAll(e => e._Category == UiAnimBase.Category.In).Count;
                mTotalAnimOutCount += anim._Anims.FindAll(e => e._Category == UiAnimBase.Category.Out).Count;
            }
            if (mTotalAnimInCount == 0)
                mTotalAnimInCount = -1;
            if (mTotalAnimOutCount == 0)
                mTotalAnimOutCount = -1;
        }

        public bool IsVisible()
        {
            return gameObject.activeInHierarchy;
        }

        public static void DisableBackButtonProcessing(bool disable)
        {
            mDisableBackButton = disable;
        }

        protected virtual void Start()
        {
            Show();
#if AUTOMATION_ENABLED
            RegisterID();
#endif
        }

        protected virtual void Update()
        {
            if (!mDisableBackButton && _BackBtn != null && mLastExecutedFrameNumber != Time.frameCount && ProcessBackButton())
                ExecuteBackButton();
        }

        protected virtual bool ProcessBackButton()
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || BackButtonPressed))
            {
                if (_BackBtn.IsVisible() && (pExclusiveItem == this || pExclusiveItem == null))
                {
                    if (pExclusiveItem != null)
                        RemoveExclusive(pExclusiveItem);                           //removing panel from list ,avoiding multiple calls.
                    return true;
                }
            }
            return false;
        }

        protected virtual void ExecuteBackButton()
        {
            mLastExecutedFrameNumber = Time.frameCount;
            _BackBtn.OnClicked();
        }

        /// <summary>
        /// Make sure the child names are unique, else you will get the first encountered transform.
        /// </summary>
        /// <param name="inItemName"></param>
        /// <returns></returns>
        public virtual UiItem Find(string inItemName)
        {
            Transform t = Utilities.FindChildTransform(gameObject, inItemName, true);
            if (t != null)
                return t.GetComponent<UiItem>();
            UnityEngine.Debug.LogError("Could not find : " + inItemName + " under : " + name);
            return null;
        }

        protected virtual UiItem[] GetChildren()
        {
            return gameObject.GetComponentsInChildren<UiItem>();
        }

        //Makes this UI interactive
        public virtual void SetInteractive(bool isInteractive)
        {
            GetComponent<GraphicRaycaster>().enabled = isInteractive;
        }

        public static bool IsExclusive()
        {
            return mExclusiveUI != null;
        }

        protected static void SetExclusive(UiBase inUI)
        {
            if (inUI != null)
            {
                if (!mExclusiveUIList.Contains(inUI))
                {
                    if (XDebug.CanLog(XDebug.Mask.UI))
                        XDebug.Log($"SetExclusive: Adding: {inUI.GetObjectPath()}, Exclusive Count: {mExclusiveUIList.Count}", XDebug.Mask.UI);
                    mExclusiveUIList.Add(inUI);
                }
            }
            else
                XDebug.LogWarning("Trying to set a null UI as exclusive.");
        }

        protected static void RemoveExclusive(UiBase inUI)
        {
            if (XDebug.CanLog(XDebug.Mask.UI))
                XDebug.Log("RemoveExclusive, Exclusive Count: " + mExclusiveUIList.Count, XDebug.Mask.UI);

            if (inUI == null)
                XDebug.LogError("RemoveExclusive:Interface reference is not valid");
            else if (mExclusiveUIList.Count > 0)
            {
                //Find and remove any _UI that's already been destroyed before it could properly be removed from exclusion list.
                mExclusiveUIList.RemoveAll(item => item == null);

                // Find the current item in the local list
                UiBase item = mExclusiveUIList.Find(item => item == inUI);
                if (XDebug.CanLog(XDebug.Mask.UI))
                    XDebug.Log($"Removing exclusive item: {item.GetObjectPath()}", XDebug.Mask.UI);
                if (item != null)
                    mExclusiveUIList.Remove(item);
            }
        }

        protected virtual void OnDisable()
        {
            if (_CanSetExclusive)
                RemoveExclusive(this);
        }

        public virtual void PlayAnim(string animName)
        {
            mAnimOutCompleteCount = mAnimInCompleteCount = 0;

            gameObject.BroadcastMessage("Play", animName, SendMessageOptions.DontRequireReceiver);
        }

        protected virtual void PlayInOutAnim(string methodName)
        {
            mAnimOutCompleteCount = mAnimInCompleteCount = 0;

            gameObject.BroadcastMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }

        private void SetVisibility(bool visible)
        {
            if (XDebug.CanLog(XDebug.Mask.UI))
            {
                string actionType = visible ? "Show" : "Hide";
                XDebug.Log($"{actionType}->{gameObject.GetObjectPath()}", XDebug.Mask.UI);
            }
            if (_CanSetExclusive)
            {
                if (visible)
                    SetExclusive(this);
                else
                    RemoveExclusive(this);
            }
            if (visible)
            {
                gameObject.SetActive(true);
                if (_PushOnTop)
                {
                    int max = GetMaxSortOrder();
                    SetSortingOrder(max + 2);
                }
                if (_OpenSFX != null)
                    _OpenSFX.Play();
            }
            else
            {
                if (_CloseSFX != null)
                    _CloseSFX.Play();
            }

            if (HasAnims(visible))
            {
                PlayInOutAnim(visible ? "PlayInAnimation" : "PlayOutAnimation");
            }
            else
            {
                OnVisibilityChanged(visible);
            }
        }

        bool HasAnims(bool isVisible)
        {
            if (isVisible && mTotalAnimInCount > 0)
                return true;
            if (!isVisible && mTotalAnimOutCount > 0)
                return true;

            return false;
        }

        [ContextMenu("Show")]
        public virtual void Show()
        {
            SetVisibility(true);
        }

        [ContextMenu("Hide")]
        public virtual void Hide()
        {
            SetVisibility(false);
        }

        [ContextMenu("SetExclusive")]
        public virtual void SetExclusive()
        {
            SetExclusive(this);
        }

        protected virtual void OnVisibilityChanged(bool visible)
        {
            if (visible)
            {
                OnShown?.Invoke(this);
            }
            else
            {
                OnHidden?.Invoke(this);
                if (_DestroyOnHide)
                    GameObject.Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            SendBreadcrumbEvent(BreadcrumbEventContants.UiDestroyed);
            if (XDebug.CanLog(XDebug.Mask.UI))
                XDebug.Log($"Destroying->{gameObject.GetObjectPath()}", XDebug.Mask.UI);
            RemoveSortingOrder();
#if AUTOMATION_ENABLED
            if (!string.IsNullOrEmpty(ID))
                IDMapper.UnRegisterID(ID);
#endif
            base.OnDestroy();
        }

        protected virtual void RemoveSortingOrder()
        {
            mSortOrders.Remove(this);
        }

        /// <summary>
        /// Called from UiAnim class when the anim objct is about to be destroyed.
        /// </summary>
        /// <param name="inAnim"></param>
        protected virtual void OnAnimDestroy(UiAnim inAnim)
        {
            mAnims.Remove(inAnim);
        }

        private IEnumerator OneFrame(Action callback)
        {
            yield return null;
            callback();
        }

        public void SetSortingOrder(int sortingOrder)
        {
            StartCoroutine(OneFrame(() =>
            {
                Canvas canvas = GetComponent<Canvas>();
                if (canvas != null)
                {
                    mSortOrders[this] = sortingOrder;
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = sortingOrder;
                }
            }));
        }

        public virtual void OnAnimEnd(UiAnim anim, string animName)
        {
            if (animName.Equals(_InAnimation, StringComparison.OrdinalIgnoreCase))
            {
                mAnimInCompleteCount++;
                if (mAnimInCompleteCount == mTotalAnimInCount)
                {
                    OnVisibilityChanged(true);
                }
            }
            else if (animName.Equals(_OutAnimation, StringComparison.OrdinalIgnoreCase))
            {
                mAnimOutCompleteCount++;
                if (mAnimOutCompleteCount == mTotalAnimOutCount)
                {
                    OnVisibilityChanged(false);
                }
            }

            if (animName == _OutAnimation)
            {
                if (_DestroyOnHide)
                    GameObject.Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }

        private static int GetMaxSortOrder()
        {
            int max = int.MinValue;
            foreach (int sortOrder in mSortOrders.Values)
            {
                if (max < sortOrder)
                    max = sortOrder;
            }
            return max;
        }

        protected bool ContainsExclusiveItem(Type uiType)
        {
            for (int i = 0; i < mExclusiveUIList.Count; i++)
            {
                if (mExclusiveUIList[i].GetType() == uiType)
                    return true;
            }
            return false;
        }
    }
}