using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.UI;

namespace XcelerateGames.SlotMachine
{
    /// <summary>
    /// Sets up each reel's symbols and it's respective sprite and once UI is
    /// successfully set it dispatches a callback
    /// </summary>
    public class UiReelBehaviour : UiMenu
    {
        public void Init(Sprite[] spritesPerReel, int spaceBetweenTwoSymbolsInSameReel,
            Action onSlotMachineSetUpDone)
        {
            var verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.enabled = false;
            verticalLayoutGroup.spacing = spaceBetweenTwoSymbolsInSameReel;
            verticalLayoutGroup.padding.bottom = spaceBetweenTwoSymbolsInSameReel;
            for (int i = 0; i < spritesPerReel.Length; i++)
            {
                UiItem uiSlotMachineSymbol = AddWidget("Symbol");
                uiSlotMachineSymbol.SetSprite(spritesPerReel[i]);
            }
            verticalLayoutGroup.enabled = true;
            GetComponent<ContentSizeFitter>().enabled = true;
            onSlotMachineSetUpDone?.Invoke();
        }
    }
}
