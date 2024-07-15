using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.Locale
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
	[AddComponentMenu(Utilities.MenuName + "UI/Localize")]
    public class UILocalize : MonoBehaviour
    {
        /// <summary>
        /// Localization key.
        /// </summary>

        public string key;

        /// <summary>
        /// Manually change the value of whatever the localization component is attached to.
        /// </summary>

        public string value
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Text lbl = GetComponent<Text>();

                    if (lbl != null)
                        lbl.text = value;
                }
            }
        }

        private bool mStarted = false;

        /// <summary>
        /// Localize the widget on enable, but only if it has been started already.
        /// </summary>

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (mStarted)
                OnLocalize();
        }

        /// <summary>
        /// Localize the widget on start.
        /// </summary>

        private void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            mStarted = true;
            OnLocalize();
        }

        /// <summary>
        /// This function is called by the Localization manager via a broadcast SendMessage.
        /// </summary>

        private void OnLocalize()
        {
            // If no localization key has been specified, use the label's text as the key
            if (string.IsNullOrEmpty(key))
            {
                Text lbl = GetComponent<Text>();
                if (lbl != null)
                    key = lbl.text;
            }

            // If we still don't have a key, leave the value as blank
            if (!string.IsNullOrEmpty(key))
                value = Localization.Get(key);
        }
    }
}
