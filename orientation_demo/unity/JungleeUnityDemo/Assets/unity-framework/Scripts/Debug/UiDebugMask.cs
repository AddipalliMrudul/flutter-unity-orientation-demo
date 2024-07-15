using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.UI;

namespace XcelerateGames.Debugging
{
    public class UiDebugMask : UiMenu
    {
        #region Properties
        #endregion //Properties

        #region Signals
        #endregion //Signals

        #region UI Callbacks
        public void OnClickItem(UiToggleItem toggleItem)
        {
            XDebug.Mask mask = (XDebug.Mask)toggleItem._UserData;
            if (toggleItem.isOn)
                XDebug.AddMask(mask);
            else
                XDebug.RemoveMask(mask);
            Debug.Log(XDebug.pMask.ToString());
        }

        public void OnClickClear()
        {
            XDebug.ClearMask();
            List<UiToggleItem> toggleItems = GetChildren<UiToggleItem>(false);
            foreach(UiToggleItem toggleItem in toggleItems)
            {
                toggleItem.isOn = false;
            }
            PlayerPrefs.DeleteKey(GameConfig.Mask);
            PlayerPrefs.Save();
        }

        public void OnClickSave()
        {
            PlayerPrefs.SetString(GameConfig.Mask, XDebug.pMask.ToString());
            PlayerPrefs.Save();
        }
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Start()
        {
            base.Start();

            PopulateMenu();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void PopulateMenu()
        {
            Array array = Enum.GetValues(typeof(XDebug.Mask));
            foreach (var mask in array)
            {
                UiToggleItem toggleItem = AddWidget<UiToggleItem>(mask.ToString());
                XDebug.Mask mask1 = (XDebug.Mask)mask;
                toggleItem._UserData = mask1;
                toggleItem.name = mask.ToString();
                toggleItem.isOn = (XDebug.pMask & mask1) != 0; 
            }
        }
        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
    }
}
