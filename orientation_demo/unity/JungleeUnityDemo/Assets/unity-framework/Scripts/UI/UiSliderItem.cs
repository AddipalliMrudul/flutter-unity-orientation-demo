using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(Slider))]
    public class UiSliderItem : UiItem
    {
        #region Properties
        public TextMeshProUGUI _MinText = null;
        public TextMeshProUGUI _MaxText = null;
        public TextMeshProUGUI _ValueText = null;
        public bool _NeedToUpdateValueTextPosition = true;

        public Slider _Slider = null;

        public float Value { get { return _Slider.value; } set { _Slider.value = value; } }
        protected Vector3 mValueTextPos = Vector3.zero;
        protected float mWidth = 100f;
        public bool _UseSteps = false;
        public float _StepSize = 1;
        protected List<float> _StepRange = new List<float>();
        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Awake()
        {
            base.Awake();
            _Slider.onValueChanged.AddListener(OnValueChange);
            if (_ValueText != null)
            {
                mValueTextPos = _ValueText.transform.localPosition;
            }
            mWidth = _Slider.GetComponent<RectTransform>().sizeDelta.x;
            if (_UseSteps)
                ComputeStepRange(_StepSize);
            UpdateValues();
        }

        public void ComputeStepRange(float stepSize)
        {
            ComputeStepRange(stepSize, 2);
        }

        public virtual void ComputeStepRange(float stepSize, int decimalPlaces)
        {
            _StepSize = stepSize;
            _StepRange.Clear();
            for (float v = _Slider.minValue; v <= _Slider.maxValue; v += _StepSize)
            {
                float rounded = (float)Math.Round(v, decimalPlaces);
                _StepRange.Add(rounded);
            }
            if (!_StepRange.Contains(_Slider.maxValue))
                _StepRange.Add(_Slider.maxValue);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _Slider.onValueChanged.RemoveListener(OnValueChange);
        }

        protected virtual void UpdateValues()
        {
            if (_MinText != null)
                _MinText.text = _Slider.minValue.ToString();
            if (_MaxText != null)
                _MaxText.text = _Slider.maxValue.ToString();
            OnValueChange(_Slider.value);
        }

        protected virtual void OnValueChange(float value)
        {
            if (_UseSteps)
            {
                value = GetStepValue(value);
            }
            if (_ValueText != null)
            {
                if (_NeedToUpdateValueTextPosition)
                {
                    mValueTextPos.x = mWidth * (value / _Slider.maxValue);
                    _ValueText.transform.localPosition = mValueTextPos;
                }
                _ValueText.text = value.ToString();
            }
        }

        public float GetStepValue(float value)
        {
            if (_StepRange.Count == 0)
                return value;
            for (int i = _StepRange.Count - 1; i >= 0; --i)
            {
                if (value >= _StepRange[i])
                {
                    return _StepRange[i];
                }
            }
            return _StepRange[0];
        }
        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
    }
}
