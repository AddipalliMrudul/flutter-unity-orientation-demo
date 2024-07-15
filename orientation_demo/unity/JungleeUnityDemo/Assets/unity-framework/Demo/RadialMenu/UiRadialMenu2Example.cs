using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.UI;

namespace XcelerateGames.UIDemo
{
    public class UiRadialMenu2Example : UiRadialMenu2
    {
        public int _Count = 3;

        private void OnEnable()
        {
            _Grid.DestroyChildren();
            for (int i = 0; i < _Count; ++i)
            {
                UiItem item = AddWidget();
                RectTransform rectTransform = item.GetComponent<RectTransform>();
                rectTransform.SetActive(true);
                rectTransform.SetParent(_Grid, Vector3.zero, Vector3.one, false);
                item.SetText((i + 1).ToString());
            }

            Align();
        }
    }
}
