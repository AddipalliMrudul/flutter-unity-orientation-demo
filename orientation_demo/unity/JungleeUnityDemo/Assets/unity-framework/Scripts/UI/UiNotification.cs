using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.Locale;

namespace XcelerateGames.UI
{
    [Serializable]
    public class NotificationUI
    {
        public UiItem mNotificationtypeContainer = null;
        public UiItem mButton = null;
        public UiItem mImage = null;
        public UiItem mLoader = null;
        public GameObject mAnimatedObj = null;
    }

    public class UiNotification : UiBase
    {
        [SerializeField] private float mHoldTime = 3f;
        [SerializeField] private NotificationUI[] mNotificationUI = null;
        [SerializeField] private UiItem BlockPanel = null;
        [SerializeField] private Canvas mCanvas = null;
        [SerializeField] private Color[] mColorPallete = null;

        private RectTransform mCurrentRectTransform = null;
        private float mElapsedTime = 0;
        private Action<string> mDoneCallback;
        private Notification mNotification;

        private void OnEnable()
        {
            mDoneCallback = null;
            mNotification = null;
            mElapsedTime = 0;
            mCurrentRectTransform = null;
            mCanvas.worldCamera = Camera.main;
        }

        protected override void Update()
        {
            base.Update();
            if (mNotification.ActionType == NotificationHideAction.Timer)
            {
                mElapsedTime += Time.deltaTime;
                if (mElapsedTime >= mHoldTime)
                {
                    HideUI();
                }
            }
        }

        private void HideUI()
        {
            Hide();
            mDoneCallback?.Invoke(mNotification.ID);
        }

        public void ShowUI(Notification notification, Action<string> callback, Action<Transform> sendanimiconcallbck)
        {
            Canvas c = transform.GetComponent<Canvas>();
            c.enabled = notification.CanvasEnabled;
            if (notification.CanChangeSortOrder)
                c.sortingOrder = notification.SortOrder;
            BlockPanel.SetVisibility(notification.ShowBlockPanel);
            mHoldTime = notification.ShowTime;
            mNotification = notification;
            int index = notification.Type;
            for (int i = 0; i < mNotificationUI.Length; i++)
            {
                mNotificationUI[i].mNotificationtypeContainer.SetVisibility(false);
                if (mNotificationUI[i].mButton != null)
                {
                    mNotificationUI[i].mButton.SetVisibility(false);
                }
                if (mNotificationUI[i].mImage != null)
                {
                    mNotificationUI[i].mImage.SetVisibility(false);
                }
                if (mNotificationUI[i].mLoader != null)
                {
                    mNotificationUI[i].mLoader.SetVisibility(false);
                }
            }
            mNotificationUI[index].mNotificationtypeContainer.SetVisibility(true);

            if (mNotificationUI[index].mImage != null)
            {
                if (notification.Icon != null)
                    mNotificationUI[index].mImage.SetSprite(notification.Icon);
                mNotificationUI[index].mImage.SetVisibility(true);
            }

           
            if (mNotificationUI[index].mLoader != null)
                if (notification.ShowLoader)
                    mNotificationUI[index].mLoader.SetVisibility(true);
                else
                    mNotificationUI[index].mLoader.SetVisibility(false);


            mCurrentRectTransform = mNotificationUI[index].mNotificationtypeContainer.GetComponent<RectTransform>();
            mCurrentRectTransform.SetActive(true);
            mDoneCallback = callback;

            SetPosition(notification.Position, notification.CustomPosition);

            mElapsedTime = 0f;

            string msg = Localization.Get(notification.Message);
            if (msg == null)
            {
                msg = notification.Message;
            }

            mNotificationUI[index].mNotificationtypeContainer.text = msg;

            if (notification.ActionType == NotificationHideAction.ButtonClick)
            {
                mNotificationUI[index].mNotificationtypeContainer.SetInteractive(true);
                if (mNotificationUI[index].mButton != null)
                {
                    mNotificationUI[index].mButton.SetVisibility(true);
                    mNotificationUI[index].mButton._OnClick.AddListener(OnClick);
                    if (notification.ButtonText != null)
                    {
                        mNotificationUI[index].mButton.text = notification.ButtonText;
                    }
                    else
                    {
                        mNotificationUI[index].mButton.text = Localization.Get("btn_Name_OK");
                    }
                    if (notification.IsLinkButton)
                    {
                        mNotificationUI[index].mButton.GetComponent<Image>().enabled = false;
                        mNotificationUI[index].mButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Underline;
                    }
                    else
                    {
                        mNotificationUI[index].mButton.GetComponent<Image>().enabled = true;
                        mNotificationUI[index].mButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Normal;
                    }
                    if (notification.ButtonTextColorIndex > -1)
                    {
                        if (mColorPallete != null)
                            if (mColorPallete.Length > notification.ButtonTextColorIndex)
                                mNotificationUI[index].mButton._TextItem.color = mColorPallete[notification.ButtonTextColorIndex];
                    }
                }
            }

            if(mNotificationUI[index].mAnimatedObj != null && sendanimiconcallbck != null){
                sendanimiconcallbck?.Invoke(mNotificationUI[index].mAnimatedObj.transform);
            }

            Show();
            Canvas.ForceUpdateCanvases();
        }

        public void HideImmediately()
        {
            if (mNotification.ActionType == NotificationHideAction.Timer)
            {
                mElapsedTime = mHoldTime + 1;
            }
            else
            {
                HideUI();
            }
        }

        private void OnClick(UiItem uiItem)
        {
            mNotificationUI[(int)mNotification.Type].mButton._OnClick.RemoveListener(OnClick);
            if (mNotification.ActionType == NotificationHideAction.ButtonClick)
            {
                if (mNotification.ButtonCallback != null)
                {
                    mNotification.ButtonCallback?.Invoke();
                }
                else
                {
                    XDebug.Log("ButtonCallback is NULL", XDebug.Mask.Game);
                }
            }
            HideImmediately();
        }

        private void SetPosition(NotificationPosition pos, Vector2 customPos)
        {
            if (pos == NotificationPosition.Default)
                return;

            int xpos = (int)mCurrentRectTransform.GetSize().x / 2;
            int ypos = (int)mCurrentRectTransform.GetSize().y / 2;
            AnchorPresets allign;
            if (pos == NotificationPosition.UpperLeft)
            {
                allign = AnchorPresets.TopLeft;
                mCurrentRectTransform.SetAnchor(allign, xpos, -ypos);
            }
            else if (pos == NotificationPosition.UpperCenter)
            {
                allign = AnchorPresets.TopCenter;
                mCurrentRectTransform.SetAnchor(allign, 0, -ypos);
            }
            else if (pos == NotificationPosition.UpperRight)
            {
                allign = AnchorPresets.TopRight;
                mCurrentRectTransform.SetAnchor(allign, -xpos, -ypos);
            }
            else if (pos == NotificationPosition.MiddleLeft)
            {
                allign = AnchorPresets.MiddleLeft;
                mCurrentRectTransform.SetAnchor(allign, xpos, 0);
            }
            else if (pos == NotificationPosition.MiddleCenter)
            {
                allign = AnchorPresets.MiddleCenter;
                mCurrentRectTransform.SetAnchor(allign, 0, 0);
            }
            else if (pos == NotificationPosition.MiddleRight)
            {
                allign = AnchorPresets.MiddleRight;
                mCurrentRectTransform.SetAnchor(allign, -xpos, 0);
            }
            else if (pos == NotificationPosition.LowerLeft)
            {
                allign = AnchorPresets.BottomLeft;
                mCurrentRectTransform.SetAnchor(allign, xpos, ypos);
            }
            else if (pos == NotificationPosition.LowerCenter)
            {
                allign = AnchorPresets.BottomCenter;
                mCurrentRectTransform.SetAnchor(allign, 0, ypos);
            }
            else if (pos == NotificationPosition.LowerRight)
            {
                allign = AnchorPresets.BottomRight;
                mCurrentRectTransform.SetAnchor(allign, -xpos, ypos);
            }
            else if (pos == NotificationPosition.Custom)
            {
                allign = AnchorPresets.MiddleCenter;
                mCurrentRectTransform.SetAnchor(allign, (int)customPos.x, (int)customPos.y);
            }
        }

        public void SetVisibility(bool isEnabled){
            mCanvas.enabled = isEnabled;
        }
    }
}