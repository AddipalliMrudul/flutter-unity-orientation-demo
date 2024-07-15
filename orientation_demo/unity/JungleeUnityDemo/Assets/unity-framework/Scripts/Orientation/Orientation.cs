using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Orientation script to be used to get notified when device orientation changes.
    /// To get notified add a listener to OnOrientationChange callback
    /// @note Works only on mobile platform. On other platforms the GameObject is destroyed in Awake function
    /// </summary>
    public class Orientation : MonoBehaviour
    {
        /// <summary>
        /// Callback to get notified on orientation change
        /// </summary>
        public static System.Action<ScreenOrientation> OnOrientationChange = null;

        /// <summary>
        /// Previous orientation
        /// </summary>
        private ScreenOrientation mPrevOrientation;

        /// <summary>
        /// Screen width
        /// </summary>
        public static int Width { get; protected set; } = 0;

        /// <summary>
        /// Screen height
        /// </summary>
        public static int Height { get; protected set; } = 0;

        /// <summary>
        /// Check if we are on mobile platform, if no, then delete the GameObject else cache Height, Width & current orientation.
        /// </summary>
        void Awake()
        {
            if (PlatformUtilities.IsMobile())
            {
                Width = Screen.width;
                Height = Screen.height;
                Debug.Log($"Orientation:On App launch: {Width}x{Height}");
                //Cache the default state
                mPrevOrientation = Screen.orientation;

                GameObject.DontDestroyOnLoad(gameObject);
            }
            else
                GameObject.Destroy(gameObject);
        }

        /// <summary>
        /// Check if the orientation has changed. If yes, trigger callback
        /// </summary>
        void Update()
        {
            if (Screen.orientation != mPrevOrientation)
            {
                Debug.Log($"Orientation changed to : {Screen.orientation}, Previous : {mPrevOrientation}\n" +
                    $"Res: {Width}x{Height}, Scree Res: {Screen.width}x{Screen.height}");
                mPrevOrientation = Screen.orientation;
                OnOrientationChange?.Invoke(Screen.orientation);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor only code to test orientation change
        /// </summary>
        [ContextMenu("LandscapeLeft")]
        public void SetLandScapeLeft()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            OnOrientationChange?.Invoke(ScreenOrientation.LandscapeLeft);
        }

        /// <summary>
        /// Editor only code to test orientation change
        /// </summary>
        [ContextMenu("LandscapeRight")]
        public void SetLandScapeRight()
        {
            Screen.orientation = ScreenOrientation.LandscapeRight;
            OnOrientationChange?.Invoke(ScreenOrientation.LandscapeRight);
        }

        /// <summary>
        /// Editor only code to test orientation change
        /// </summary>
        [ContextMenu("Portrait")]
        public void SetPortrait()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            OnOrientationChange?.Invoke(ScreenOrientation.Portrait);
        }
#endif
    }
}
