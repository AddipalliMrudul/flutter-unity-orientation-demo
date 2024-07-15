using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiRadialMenu : UiMenu
    {
        public Vector3 _PositionOffset = Vector3.zero;
        public float _Radius = 100;

        protected Vector3 FindPoint(Vector3 c, float r, int i, int count)
        {
            return _PositionOffset + c + Quaternion.AngleAxis((360f / count) * i, Vector3.forward) * (Vector3.right * r);
        }

        [ContextMenu("Align")]
        public virtual void Align()
        {
            List<UiItem> widgets = GetChildren(false);
            int childCount = widgets.Count;
            for (int i = 0; i < childCount; ++i)
            {
                widgets[i].transform.localPosition = FindPoint(Vector3.zero, _Radius, i, childCount);
            }
        }
    }
}
