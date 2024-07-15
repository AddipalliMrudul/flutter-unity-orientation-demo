using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(Toggle))]
    public class UiToggleItem : UiItem, IPointerClickHandler
    {
        [Serializable] public class XGToggleEvent : UnityEvent<bool> { }
        [Serializable] public class XGToggleEvent2 : UnityEvent<UiToggleItem> { }

        public XGToggleEvent _OnValueChange;
        public XGToggleEvent _OnToggleClick;
        public XGToggleEvent2 _OnToggleClick2;

        public GameObject[] _OnGroup, _OffGroup;

        protected Toggle mToggle = null;

        protected Toggle Toggle
        {
            get
            {
                if (mToggle == null)
                    mToggle = GetComponent<Toggle>();
                return mToggle;
            }
            set
            {
                if (mToggle == null)
                    mToggle = GetComponent<Toggle>();
                mToggle.isOn = value;
            }
        }

        public bool isOn
        {
            get => Toggle.isOn;
            set
            {
                Toggle.isOn = value;
                UpdateState();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Toggle.onValueChanged.AddListener(OnValueChange);
        }

        protected override void Start()
        {
            base.Start();
            UpdateState();
        }

        public virtual void OnValueChange(bool isOn)
        {
            if (!GuiManager.pBlockClickEvents)
            {
                GuiManager.pInstance.StartCoroutine(GuiManager.pInstance.WaitAndEnable(_InvokeDelay));
                Invoke("SendValueChangeEvent", _InvokeDelay);
                //AudioController.PlaySFX(_ClickSound);
            }
        }

        public void SendValueChangeEvent()
        {
            UpdateState();
            _OnValueChange?.Invoke(Toggle.isOn);
        }

        public void SendToggleClickEvent()
        {
            UpdateState();
            _OnToggleClick?.Invoke(Toggle.isOn);
            _OnToggleClick2?.Invoke(this);
        }

        public override void SetInteractive(bool isInteractive)
        {
            if (Toggle != null)
                Toggle.interactable = isInteractive;
        }

        public override bool IsInteractive()
        {
            if (Toggle != null)
                return Toggle.interactable;
            return false;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (Toggle.interactable)
            {
                Invoke("SendToggleClickEvent", _InvokeDelay);
                PlaySound(_ClickSound);
            }
        }

        private void UpdateState()
        {
            Array.ForEach(_OffGroup, (GameObject obj) => obj.SetActive(!Toggle.isOn));
            Array.ForEach(_OnGroup, (GameObject obj) => obj.SetActive(Toggle.isOn));
        }

        #region Editor Only code
#if UNITY_EDITOR
        [ContextMenu("On")]
        private void SetOn()
        {
            isOn = true;
        }

        [ContextMenu("Off")]
        private void SetOff()
        {
            isOn = false;
        }
#endif
        #endregion
    }
}
