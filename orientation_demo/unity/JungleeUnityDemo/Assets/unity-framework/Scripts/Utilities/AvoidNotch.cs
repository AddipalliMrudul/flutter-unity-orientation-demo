using System.Collections;
using UnityEngine;

namespace XcelerateGames
{
    public class AvoidNotch : MonoBehaviour
    {
        #region Data Members
        //Public
        [Header("[ Config ]")]
        [Tooltip("Offset between 'Gameobject' and 'Nocth'")]
        public Vector2 Offset;
        //Private
        private RectTransform rectTransform;        
        private Vector2 initialPosition;
        #endregion//============================================================[ Data Members ]

        #region Unity Functions
        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = gameObject.GetComponent<RectTransform>();
            }
        }        

        private void OnEnable()
        {
            initialPosition = rectTransform.anchoredPosition;
            OnOrientationChange(Screen.orientation);
            Orientation.OnOrientationChange += OnOrientationChange;
        }

        private void OnDisable()
        {
            Orientation.OnOrientationChange -= OnOrientationChange;            
        }
        #endregion//============================================================[ Unity Functions ]

        #region Callback Listeners
        private void OnOrientationChange(ScreenOrientation screenOrientation)
        {
            rectTransform.anchoredPosition = initialPosition;
            StartCoroutine(SetObjectPosition(Screen.orientation));
        }
        #endregion//============================================================[ Callback Listeners ]

        #region Public Functions
        /// <summary>
        /// Forces the object to update it's position to avoid the 'Notch'
        /// </summary>
        public void SetDirty()
        {            
            OnOrientationChange(Screen.orientation);
        }
        #endregion//============================================================[ Public Functions ]

        #region Private Functions
        private IEnumerator SetObjectPosition(ScreenOrientation screenOrientation)
        {
            yield return new WaitForEndOfFrame();            
            if (NotchController.Instance != null)
            {
                yield return new WaitForEndOfFrame();
                float overlapArea = NotchController.Instance.OverlapsWithNotch(rectTransform);
                if (overlapArea > 0.0f)
                {
                    if (screenOrientation == ScreenOrientation.Portrait)
                    {
                        rectTransform.position = new Vector2(rectTransform.position.x + Offset.x, rectTransform.position.y + Offset.y - NotchController.Instance.NotchSize.y);
                    }
                    else if (screenOrientation == ScreenOrientation.PortraitUpsideDown)
                    {
                        rectTransform.position = new Vector2(rectTransform.position.x + Offset.x, rectTransform.position.y + Offset.y + NotchController.Instance.NotchSize.y);
                    }
                    else if (screenOrientation == ScreenOrientation.LandscapeLeft)
                    {
                        rectTransform.position = new Vector2(rectTransform.position.x + Offset.x + NotchController.Instance.NotchSize.x, rectTransform.position.y + Offset.y);
                    }
                    else if (screenOrientation == ScreenOrientation.LandscapeRight)
                    {
                        rectTransform.position = new Vector2(rectTransform.position.x + Offset.x - NotchController.Instance.NotchSize.x, rectTransform.position.y + Offset.y);                        
                    }
                }
            }
        }        
        #endregion//============================================================[ Private Functions ]
    }
}
