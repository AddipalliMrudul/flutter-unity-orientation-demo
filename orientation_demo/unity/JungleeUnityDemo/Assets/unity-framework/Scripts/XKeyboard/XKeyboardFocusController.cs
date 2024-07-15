using UnityEngine;
using UnityEngine.EventSystems;

namespace XcelerateGames.Keyboard
{
    public class XKeyboardFocusController : MonoBehaviour, IPointerDownHandler
    {
        #region Data    
        //Public        
        public bool hideKeyboard = true;
        #endregion//============================================================[ Data ]

        #region Unity
        public void OnPointerDown(PointerEventData eventData)
        {
            if (XKeyboard.instace != null)
            {
                if (hideKeyboard)
                {
                    XKeyboard.instace.HideKeyboard();
                }                
            }
        }
        #endregion//============================================================[ Unity ]
    }
}
