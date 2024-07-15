using TMPro;
using UnityEngine;

namespace XcelerateGames.UI
{
    /// <summary>
    /// Script to add scrolling to text. Refeer to PfUiScrollTextDemo prefab to see how to setup the UI
    /// </summary>
    [DisallowMultipleComponent]
    public class UiTextScroller : MonoBehaviour
    {
        [SerializeField] protected RectTransform _TextItem = null;        /**<The text item that needs to be scrolled */
        [SerializeField] RectTransform _Parent = null;          /**<RectTransform under which text will scroll */
        [SerializeField] float _Speed = 250f;                   /**<Scrolling Speed */
        [SerializeField] float _Offset = 200f;                  /**<Distance between the text elements. Keep i slightly high to show the difference in text */
        [SerializeField] float _StartDelay = 3f;                /**<Text will start scrolling after the delay given here */

        protected RectTransform _TextItemCloned = null;                   /**<Reference to cloned text element */
        float mWidth = 0;                                       /**<Width of text */
        Rect mParentRect;                                       /**<Rect of parent RectTransform */

        void Start()
        {
            enabled = false;
            Compute();
        }

        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        void Compute()
        {
            //Disable the script, will be enalbed if need be
            enabled = false;
            mWidth = _TextItem.GetComponent<TextMeshProUGUI>().preferredWidth;
            mParentRect = _Parent.WorldRect();

            //Check if the text is larger than the display area. We will start scrolling only if text is larger than the display area.
            if (mWidth > mParentRect.width)
            {
                //Clone & add the same text component to the right of the text item
                if (_TextItemCloned == null)
                    _TextItemCloned = Instantiate<RectTransform>(_TextItem, transform);
                else
                    _TextItemCloned.GetComponent<TextMeshProUGUI>().text = _TextItem.GetComponent<TextMeshProUGUI>().text;

                _TextItemCloned.name = $"{_TextItem.name}-Clone";
                Vector3 pos = _TextItem.localPosition;
                pos.x += mWidth + _Offset;
                _TextItemCloned.localPosition = pos;
                //Stop any invoke (if in progress)
                CancelInvoke(nameof(StartScrolling));
                //Start scrolling after the given dely
                Invoke(nameof(StartScrolling), _StartDelay);
            }
            else
            {
                if (_TextItemCloned != null)
                    _TextItemCloned.SetActive(false);
            }
        }

        void OnTextChanged(Object obj)
        {
            if (obj == _TextItem.GetComponent<TextMeshProUGUI>())
            {
                Compute();
            }
        }

        /// <summary>
        /// Update the text elements every frame to move left with the given speed.
        /// </summary>
        protected virtual void Update()
        {
            Vector3 delta = Vector3.left * _Speed * Time.deltaTime;
            _TextItem.localPosition += delta;
            if(_TextItemCloned != null)
                _TextItemCloned.localPosition += delta;
            CheckOutOfView(_TextItem);
            if(_TextItemCloned != null)
                CheckOutOfView(_TextItemCloned);
        }

        /// <summary>
        /// Check if the text has scrolled out of screen. If yes, then add it back to end
        /// </summary>
        /// <param name="rectTransform">RectTransform  of text element to check against parent position</param>
        protected void CheckOutOfView(RectTransform rectTransform)
        {
            Rect childRect = rectTransform.WorldRect();
            float x = childRect.x + childRect.width;
            if (mParentRect.x > x)
            {
                RectTransform t = rectTransform == _TextItem ? _TextItemCloned : _TextItem;
                Vector3 pos = t.localPosition;
                pos.x += mWidth + _Offset;
                pos.y = 0;
                rectTransform.localPosition = pos;
            }
            else if ((mParentRect.x + mParentRect.width) < childRect.x)
            {
                RectTransform t = rectTransform == _TextItem ? _TextItemCloned : _TextItem;
                Vector3 pos = t.localPosition;
                pos.x -= mWidth + _Offset;
                pos.y = 0;
                rectTransform.localPosition = pos;
            }
        }

        /// <summary>
        /// Get references of text element & parent container. Works on IDE only to get.
        /// </summary>
        private void OnValidate()
        {
            if (_TextItem == null)
                _TextItem = GetComponentInChildren<RectTransform>(true);
            if (_Parent == null)
                _Parent = GetComponentInParent<RectTransform>();
        }

        /// <summary>
        /// Start scrolling
        /// </summary>
        private void StartScrolling()
        {
            enabled = true;
        }
    }
}
