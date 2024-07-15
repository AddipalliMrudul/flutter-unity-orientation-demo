using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.UI;
using XcelerateGames.WebServices;

namespace JungleeGames
{
    public class UiAPISimulator : UiMenu
     {
        [SerializeField] Toggle _EnableSimulation = null;
        #region Signals
        #endregion //Signals

        #region UI Callbacks
        public void OnClickClear()
        {
            WebRequestSettings.Clear();
        }

        public void OnClickSave()
        {
            WebRequestSettings.pInstance._SimulationEnabled = _EnableSimulation.isOn;
            WebRequestSettings.Save();
        }

        public void OnClickRandomize()
        {
            WebRequestSettings.Randomize();
            PopulateMenu();
        }
        #endregion //UI Callbacks

        #region Private Methods

        protected override void Start()
        {
            base.Start();

            WebRequestSettings.Load();
            if(WebRequestSettings.pInstance != null)
            {
                _EnableSimulation.isOn = WebRequestSettings.pInstance._SimulationEnabled;
                PopulateMenu();
            }
            else
                _EnableSimulation.isOn = false;
        }
        
        private void PopulateMenu()
        {
            ClearWidgets();
            foreach(APIConfig config in WebRequestSettings.pConfigs)
            {
                AddWidget<UiAPISimulatorItem>(config.endpoint).Init(config);
            }
        }
#endregion //Private Methods
     }
}
