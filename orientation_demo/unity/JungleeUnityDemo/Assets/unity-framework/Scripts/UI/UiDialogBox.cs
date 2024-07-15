using UnityEngine;
using System.Collections;
using XcelerateGames;
using XcelerateGames.AssetLoading;
using XcelerateGames.Locale;

namespace XcelerateGames.UI
{

    public enum DialogBoxType
    {
        NONE,
        YES_NO,
        OK_CANCEL,
        CLOSE,
        OKAY
    }

    public enum DialogBoxHeaderType
    {
        ERROR,
        WARNING,
        INFO,
    }

    public class UiDialogBox : UiBase
    {
        [SerializeField] protected UiItem mBtnClose = null;

        public UiItem pBtnClose { get { return mBtnClose; } }

        [SerializeField] protected UiItem mBtnYes = null;

        public UiItem pBtnYes { get { return mBtnYes; } }

        [SerializeField] protected UiItem mBtnNo = null;
        public UiItem pBtnNo { get { return mBtnNo; } }

        public Sprite _SelectedSprite, _UnselectedSprite;
        [HideInInspector] public bool _AutoHide = true;

        [SerializeField] protected UiItem mBtnOk = null;
        [SerializeField] protected UiItem mBtnOkay = null;
        public UiItem pBtnOkay { get { return mBtnOkay; } }
        [SerializeField] protected UiItem mBtnCancel = null;
        [SerializeField] protected UiItem mTxtMessage = null;
        [SerializeField] protected UiItem mTxtHeader = null;
        [SerializeField] protected UiItem mSelectedItem = null;

        public string _WarningMessage = "Warning!";
        public string _ErrorMessage = "Error!";
        public string _InfoMessage = "Info!";

        public System.Action OnYes = null;
        public System.Action OnNo = null;
        public System.Action OnClose = null;
        public System.Action OnOk = null;
        public System.Action OnOkay = null;
        public System.Action OnCancel = null;

        public void OnItemClicked(UiItem inItem)
        {
            mSelectedItem = inItem;
            SetExclusive(this);
            SendEvent();
            if (_AutoHide)
                Hide();
        }

        public void SetButtonText(string noBtnKey = null, string yesBtnKey = null, string closeBtnKey = null, string okayBtnKey = null)
        {
            if (!string.IsNullOrEmpty(noBtnKey))
                mBtnNo.SetLocaleText(noBtnKey);
            if (!string.IsNullOrEmpty(yesBtnKey))
                mBtnYes.SetLocaleText(yesBtnKey);
            if (!string.IsNullOrEmpty(closeBtnKey))
                mBtnClose.SetLocaleText(closeBtnKey);
            if (!string.IsNullOrEmpty(okayBtnKey))
                mBtnOkay.SetLocaleText(okayBtnKey);
        }

        public virtual void SetButtonSprite(UiItem button, Sprite sprite)
        {
            if (button != null)
                button.SetSprite(sprite);
        }

        public virtual void SetButtonTextColor(UiItem button, Color textColor)
        {
            if (button != null)
                button._TextItem.color = textColor;
        }

        public virtual void SetSelected(bool noBtnKey, bool yesBtnKey)
        {
            mBtnNo.SetSprite(noBtnKey ? _SelectedSprite : _UnselectedSprite);
            mBtnYes.SetSprite(yesBtnKey ? _SelectedSprite : _UnselectedSprite);
        }

        private void SetDBType(DialogBoxType inType)
        {
            if (inType == DialogBoxType.NONE)
            {
                mBtnNo.SetVisibility(false);
                mBtnYes.SetVisibility(false);
                mBtnCancel.SetVisibility(false);
                mBtnOk.SetVisibility(false);
                mBtnClose.SetVisibility(false);
                mBtnOkay.SetVisibility(false);
            }
            else if (inType == DialogBoxType.CLOSE)
            {
                mBtnNo.SetVisibility(false);
                mBtnYes.SetVisibility(false);
                mBtnCancel.SetVisibility(false);
                mBtnOk.SetVisibility(false);
                mBtnOkay.SetVisibility(false);

                mBtnClose.SetVisibility(true);
            }
            else if (inType == DialogBoxType.OKAY)
            {
                mBtnNo.SetVisibility(false);
                mBtnYes.SetVisibility(false);
                mBtnCancel.SetVisibility(false);
                mBtnOk.SetVisibility(false);
                mBtnClose.SetVisibility(false);

                mBtnOkay.SetVisibility(true);
            }
            else if (inType == DialogBoxType.OK_CANCEL)
            {
                mBtnNo.SetVisibility(false);
                mBtnYes.SetVisibility(false);
                mBtnClose.SetVisibility(false);
                mBtnOkay.SetVisibility(false);

                mBtnCancel.SetVisibility(true);
                mBtnOk.SetVisibility(true);
            }
            else if (inType == DialogBoxType.YES_NO)
            {
                mBtnClose.SetVisibility(false);
                mBtnCancel.SetVisibility(false);
                mBtnOk.SetVisibility(false);
                mBtnOkay.SetVisibility(false);

                mBtnNo.SetVisibility(true);
                mBtnYes.SetVisibility(true);
            }
        }

        public void ShowDB(string inMessage, string inHeader, DialogBoxType inType)
        {
            mTxtHeader?.SetText(inHeader);
            mTxtMessage.SetText(inMessage);

            SetDBType(inType);

            SetExclusive(this);
            PlayAnim(_InAnimation);
        }

        public IEnumerator SetDBType(DialogBoxType inType, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            SetDBType(inType);
        }

        public static UiDialogBox Show(string inMessage, string inHeader, DialogBoxType inType)
        {
            UiDialogBox dlgBox = Create();
            dlgBox.ShowDB(inMessage, inHeader, inType);
            return dlgBox;
        }

        public static UiDialogBox ShowKey(string messageKey, string headerKey, DialogBoxType inType)
        {
            return Show(Localization.Get(messageKey), Localization.Get(headerKey), inType);
        }

        public static UiDialogBox Show(string inMessage, DialogBoxHeaderType inHeaderType, DialogBoxType inType)
        {
            UiDialogBox dlgBox = Create();
            if (inHeaderType == DialogBoxHeaderType.ERROR)
                dlgBox.ShowDB(inMessage, dlgBox._ErrorMessage, inType);
            else if (inHeaderType == DialogBoxHeaderType.WARNING)
                dlgBox.ShowDB(inMessage, dlgBox._WarningMessage, inType);
            else if (inHeaderType == DialogBoxHeaderType.INFO)
                dlgBox.ShowDB(inMessage, dlgBox._InfoMessage, inType);
            return dlgBox;
        }

        protected static UiDialogBox Create()
        {
            string prefabName = "PfUiDialogBox";
            GameObject go = ResourceManager.LoadFromResources(prefabName) as GameObject;
            if (go != null)
                return GameObject.Instantiate(go).GetComponent<UiDialogBox>();
            else
                Debug.LogError("Could not find : " + prefabName + " under resources.");
            return null;
        }

        /// <summary>
        /// This is called from Anim3D class as SendMessage
        /// </summary>
        /// <param name="animName">Animation name.</param>
        //   public override void OnAnimDone(string animName)
        //{
        //       SendEvent();

        //       base.OnAnimDone(animName);
        //	if(animName.Equals(_HideAnimName))
        //		GameObject.Destroy(gameObject);
        //	else if(animName.Equals(_ShowAnimName))
        //	{
        //		//Make the UI interactive here...
        //	}
        //}

        private void SendEvent()
        {
            if (mSelectedItem == mBtnYes)
            {
                OnYes?.Invoke();
            }
            else if (mSelectedItem == mBtnNo)
            {
                OnNo?.Invoke();
            }
            else if (mSelectedItem == mBtnOk)
            {
                OnOk?.Invoke();
            }
            else if (mSelectedItem == mBtnCancel)
            {
                OnCancel?.Invoke();
            }
            else if (mSelectedItem == mBtnClose)
            {
                OnClose?.Invoke();
            }
            else if (mSelectedItem == mBtnOkay)
            {
                OnOkay?.Invoke();
            }
        }
    }
}
