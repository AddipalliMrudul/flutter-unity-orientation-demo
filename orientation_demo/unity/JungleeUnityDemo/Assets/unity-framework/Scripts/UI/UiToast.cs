using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;
using XcelerateGames.Locale;

/* Add the prefab (PfUiToast) to scene & keep the game object enabled
 */

namespace XcelerateGames
{
    public enum ToastLength
    {
        //2 sec
        Short = 0,
        //3 sec
        Medium,
        //5 sec
        Long
    }
}

namespace XcelerateGames.UI
{
    public class UiToast : UiBase
    {
        protected class ToastMessage
        {
            public string messsage;
            public bool localize;
            public ToastLength toastLength;

            public ToastMessage(string messsage, bool localize, ToastLength toastLength)
            {
                this.messsage = messsage;
                this.localize = localize;
                this.toastLength = toastLength;
            }
        }

        public float _HoldTime = 3f;
        public float _Short = 2f;
        public float _Medium = 3f;
        public float _Long = 5f;
        public UiItem _Text = null;

        protected float mElapsedTime = 0;

        [InjectSignal] private SigShowToast mSigShowToast = null;
        [InjectSignal] private SigHideToast mSigHideToast = null;


        private bool mShowing = false;
        protected Queue<ToastMessage> mToastMessages = new Queue<ToastMessage>();

        protected override void Awake()
        {
            base.Awake();
            mSigShowToast.AddListener(OnShowToast);
            mSigHideToast.AddListener(OnHideToast);
            enabled = false;
            OnVisibilityChanged(false);
            //Remove this canvas from the list as we want this to be always on top
            RemoveSortingOrder();
        }

        protected override void OnDestroy()
        {
            mSigShowToast.RemoveListener(OnShowToast);
            mSigHideToast.RemoveListener(OnHideToast);
            base.OnDestroy();
        }

        private void OnShowToast(string messsage, bool localize, ToastLength toastLength)
        {
            enabled = true;
            mToastMessages.Enqueue(new ToastMessage(messsage, localize, toastLength));
            Show();
            XDebug.Log($"Showing Toast: {messsage}, length: {toastLength}",XDebug.Mask.Notifications);
        }

        public override void Show()
        {
            if (mToastMessages.Count > 0 && !mShowing)
            {
                mShowing = true;
                var tm = mToastMessages.Dequeue();
                if (tm.localize)
                    tm.messsage = Localization.Get(tm.messsage);
                _Text.text = tm.messsage;
                mElapsedTime = 0;
                _HoldTime = GetHoldTime(tm.toastLength);
                base.Show();
            }
        }

        private void OnHideToast()
        {
            mToastMessages.Clear();
            Hide();
        }

        protected override void Update()
        {
            base.Update();
            mElapsedTime += Time.deltaTime;
            if (mElapsedTime >= _HoldTime)
            {
                mElapsedTime = 0;
                ShowNextToastMessage();
            }
        }

        private void ShowNextToastMessage()
        {
            mShowing = false;
            if (mToastMessages.Count > 0)
                Show();
            else { Hide(); }
        }

        public override void Hide()
        {
            enabled = false;
            base.Hide();
        }

        protected float GetHoldTime(ToastLength toastLength)
        {
            switch (toastLength)
            {
                case ToastLength.Short:
                    return _Short;
                case ToastLength.Medium:
                    return _Medium;
                default:
                    return _Long;
            }
        }
    }
}