using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    public enum KeyboardType
    {
        Standard,
        Num,
        Alphabet
    }

    public class JGKeyboard : UiMenu
    {
        #region Signals
        [InjectSignal] private SigShowKeyboard mSigShowKeyboard = null;
        [InjectSignal] private SigHideKeyboard mSigHideKeyboard = null;
        [InjectSignal] private SigOnShowKeyboard mSigOnShowKeyboard = null;
        #endregion //Signals

        #region Properties
        [SerializeField] private GameObject[] mKeyboardPanel = null;
        #endregion //Properties

        #region Static Variables
        public static JGKeyboard pInstance = null;
        public static JGInputField mlastSelected;
        public static bool _IsKeyboardOpen;
        public static RectTransform KeyboardRect
        {
            get
            {
                return pInstance.mActiveKeyboard.transform.GetChild(0).transform.GetComponent<RectTransform>();
            }
        }

        #endregion // Static Variables

        private GameObject mActiveKeyboard;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            pInstance = this;
        }

        protected override void Start()
        {
            base.Start();
            HideAllKeyboard();
            mSigShowKeyboard.AddListener(ShowKeyboard);
            mSigHideKeyboard.AddListener(Hidekeyboard);
            Hide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSigShowKeyboard.RemoveListener(ShowKeyboard);
            mSigHideKeyboard.RemoveListener(Hidekeyboard);
        }

        private void ShowKeyboard(JGInputField inputfield, KeyboardType type, TextAnchor Pos)
        {
            Show();
            HideAllKeyboard();
            //mlastSelected = inputfield;
            int ktype = (int)type;
            mKeyboardPanel[ktype].SetActive(true);
            mKeyboardPanel[ktype].GetComponent<GridLayoutGroup>().childAlignment = Pos;
            mSigOnShowKeyboard.Dispatch();
            _IsKeyboardOpen = true;
            mActiveKeyboard = mKeyboardPanel[ktype];
            SetCurrentInputField(inputfield);
        }

        void Hidekeyboard()
        {
            // mlastSelected = null;
            SetCurrentInputField(null);
            if (mActiveKeyboard.activeSelf)
            {
                mActiveKeyboard.SetActive(false);
            }
            _IsKeyboardOpen = false;
            mActiveKeyboard = null;
            Hide();
        }
        void HideAllKeyboard()
        {
            for (int i = 0; i < mKeyboardPanel.Length; i++)
            {
                mKeyboardPanel[i].SetActive(false);
            }
            _IsKeyboardOpen = false;
            mActiveKeyboard = null;
        }

        void SetCurrentInputField(JGInputField inputfield)
        {
            if (inputfield != null)
            {
                if (mlastSelected == null)
                {
                    mlastSelected = inputfield;
                    mlastSelected.OnFocusthisField();
                }
                else
                {
                    if (mlastSelected != inputfield)
                    {
                        mlastSelected.OnFocusRemoveFromthisField(); //remove from last one 
                        mlastSelected = inputfield;
                        mlastSelected.OnFocusthisField(); //set on current one
                    }
                }
            }
            else
            {
                if (mlastSelected != null)
                {
                    mlastSelected.OnFocusRemoveFromthisField();
                }
                mlastSelected = null;
            }
        }

        //public void Init(JGInputField inputfield, KeyboardType type, TextAnchor Pos)
        //{
        //    ShowKeyboard(inputfield, type, Pos);
        //}
    }
}
