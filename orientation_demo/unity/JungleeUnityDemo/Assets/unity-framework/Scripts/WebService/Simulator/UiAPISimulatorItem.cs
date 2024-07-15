using System;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.UI;
using XcelerateGames.WebServices;

namespace JungleeGames
{
    public class UiAPISimulatorItem : UiItem
     {
        #region Properties
        [SerializeField] UiItem _Name = null;
        [SerializeField] UiSliderItem _FailPercentage = null;
        [SerializeField] UiSliderItem _Delay = null;
        [SerializeField] UiDropDown _ErrorType = null;

        private APIConfig mConfig = null;

        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Start()
        {
            base.Start();
            _Delay._Slider.onValueChanged.AddListener(OnDelayChange);
            _FailPercentage._Slider.onValueChanged.AddListener(OnFailPercentageChange);
            _ErrorType.OnSelectionChange.AddListener(OnValueChange);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _Delay._Slider.onValueChanged.RemoveListener(OnDelayChange);
            _FailPercentage._Slider.onValueChanged.RemoveListener(OnFailPercentageChange);
            _ErrorType.OnSelectionChange.RemoveListener(OnValueChange);
        }

        private void OnFailPercentageChange(float failPercentage)
        {
            mConfig.simulation.failProbability = (int)failPercentage;
        }

        private void OnDelayChange(float delay)
        {
            mConfig.simulation.delay = delay;
        }

        private void OnValueChange(string selectedItem)
        {
            mConfig.simulation.APIErrorType = (APIErrorType)_ErrorType.GetSelectedIndex();
        }
        #endregion //Private Methods

        #region Public Methods
        public void Init(APIConfig endPoint)
        {
            mConfig = endPoint;
            _Name.text = endPoint.endpoint;
            _FailPercentage.Value = endPoint.simulation.failProbability;
            _Delay.Value = endPoint.simulation.delay;
            _ErrorType.SetSelected((int)mConfig.simulation.APIErrorType);
        }
        #endregion //Public Methods
    }
}
