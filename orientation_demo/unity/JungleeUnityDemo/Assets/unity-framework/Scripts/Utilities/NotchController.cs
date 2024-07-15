using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.UI;

namespace XcelerateGames
{
    public class NotchController : MonoBehaviour
    {
        #region Instance
        private static NotchController instance;
        #endregion//============================================================[ Instance ]

        #region Data Members
        //Public
        [Header("[ Config ]")]
        [Tooltip("Visualizes notch area in develooment mode")]
        [SerializeField] private bool debugView = false;
        [Header("[ References ]")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private Camera mainCamaera;
        [SerializeField] private RectTransform parentCanvasRectTransform;        
        //Private           
        private bool updateNotch;
        private Rect notchRect;
        private Vector2 notchSize;
        private Vector2 notchPosition;
        #endregion//============================================================[ Data Members ]

        #region Getters And Setters
        public static NotchController Instance { get => instance; }
        public Vector2 NotchSize { get => notchSize; }
        public Vector2 NotchPosition { get => notchPosition; }
        #endregion//============================================================[ Getters And Setters ]

        #region Unity Functions
        private void Awake()
        {
            instance = this;           
        }

        private void Update()
        {
            if (updateNotch)
            {
                UpdateNotch();
            }
        }

        private void OnEnable()
        {
            OnOrientationChange(Screen.orientation);
            Orientation.OnOrientationChange += OnOrientationChange;
        }

        private void OnDisable()
        {
            Orientation.OnOrientationChange -= OnOrientationChange;
            updateNotch = false;
        }
        #endregion//============================================================[ Unity Functions ]

        #region Unity Editor
#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.name = "NotchController";
            if (rectTransform == null)
            {
                rectTransform = gameObject.GetComponent<RectTransform>();
            }
            if (mainCamaera == null)
            {
                mainCamaera = (Camera)FindObjectOfType(typeof(Camera));
            }
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
                parentCanvasRectTransform = parentCanvas.GetComponent<RectTransform>();
            }
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.position = Vector3.zero;
        }
#endif
        #endregion//============================================================[ Unity Editor ]

        #region Callback Listeners
        private void OnOrientationChange(ScreenOrientation screenOrientation)
        {
            updateNotch = true;
            DOVirtual.DelayedCall(3, () =>
            {
                updateNotch = false;
            });
        }
        #endregion//============================================================[ Callback Listeners ]

        #region Public Functions
        /// <summary>
        /// Returns a boolean if the given element(rectTransform) is overlapping with the Notch
        /// </summary>
        /// <param name="rectTransform">RectTransform of the Object</param>
        /// <returns></returns>
        public float OverlapsWithNotch(RectTransform rectTransform)
        {
            return RectTransformExtensions.OverlapArea(mainCamaera, rectTransform, this.rectTransform);
        }
        #endregion//============================================================[ Public Functions ]

        #region Private Functions
        private void UpdateNotch()
        {
            if (rectTransform == null)
            {
                rectTransform = gameObject.GetComponent<RectTransform>();
            }
            if (mainCamaera == null)
            {
                mainCamaera = (Camera)FindObjectOfType(typeof(Camera));
            }
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
                parentCanvasRectTransform = parentCanvas.GetComponent<RectTransform>();
            }
            if (debugView && gameObject.GetComponent<Image>() == null)
            {
                gameObject.AddComponent<Image>();
            }
            Rect[] screenCutouts = Screen.cutouts;
            if (screenCutouts.Length != 0)
            {
                notchRect = screenCutouts[0];                
                SetUpNotch();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetUpNotch()
        {            
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                var safeArea = RectTransformExtensions.RelativeToReal(RectTransformExtensions.ToScreenRelativeRect(Screen.safeArea));                
                notchSize = new Vector2(
                        notchRect.width / Screen.width * parentCanvasRectTransform.rect.width,
                        notchRect.height / Screen.height * parentCanvasRectTransform.rect.height
                    );
                notchPosition = new Vector2(
                        notchRect.position.x / Screen.width * parentCanvasRectTransform.rect.width,
                        notchRect.position.y / Screen.height * parentCanvasRectTransform.rect.height
                    );                
                rectTransform.sizeDelta = notchSize;
                rectTransform.anchoredPosition = notchPosition;
            }
            else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // TODO: Perform calculation for 'Screenspace-Camera' using ScreenPointToLocalPointInRectangle.                
            }
        }
        #endregion//============================================================[ Private Functions ]        
    }
}
