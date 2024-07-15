using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    public class UiForceRebuildLayout : MonoBehaviour
    {
        #region UI Callbacks
        public void RebuildLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
        #endregion //UI Callbacks
    }
}
