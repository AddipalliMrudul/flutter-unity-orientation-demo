using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace XcelerateGames.UI
{
    [System.Serializable]
    public class TestData
    {
        public string _Name = null; 
        public int _Age = 0;
    }

     public class UiMenuBindingDemo : UiMenu
     {
        #region Properties
        [SerializeField] private List<TestData> _TestData = null;
        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Start()
        {
            base.Start();

            AddWidgets<TestData>(_TestData);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
     }
}
#endif