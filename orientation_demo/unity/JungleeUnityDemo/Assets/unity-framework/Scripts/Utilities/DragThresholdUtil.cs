using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;
using XcelerateGames;
using UnityEngine.EventSystems;

namespace XcelerateGames.UI
{
     public class DragThresholdUtil : BaseBehaviour
     {
        //this is only control drag on a particular ui .. add on gameobject to get it work.. drag will reset once object destroys
        int defaultval = 0;
        void Start()
        {
            defaultval = EventSystem.current.pixelDragThreshold;
            int currentVal = defaultval;
            EventSystem.current.pixelDragThreshold =
                    Mathf.Max(
                         currentVal,
                         (int)(currentVal * Screen.dpi / 160f));
        }

        protected override void OnDestroy()
        {
            EventSystem.current.pixelDragThreshold = defaultval;

            base.OnDestroy();
        }

    }
}
