using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiRadialMenu2 : UiMenu
    {
        public float _AngleOffset = 0;

        [ContextMenu("Align")]
        protected virtual void Align()
        {
            int childCount = _Grid.childCount;
            float angle = (360f / childCount);
            for (int i = 0; i < childCount; ++i)
            {
                _Grid.GetChild(i).rotation = Quaternion.Euler(0, 0, -(angle * i) - _AngleOffset);
            }
        }
    }
}
