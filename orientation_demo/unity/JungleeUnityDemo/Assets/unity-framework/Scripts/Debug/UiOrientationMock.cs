using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiOrientationMock : MonoBehaviour
    {
#if UNITY_EDITOR
        public void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
#endif
        public void OnClickLandScapeLeft()
        {
#if UNITY_EDITOR
            Orientation orientation = Utilities.FindObjectOfType<Orientation>();
            if (orientation != null)
                orientation.SetLandScapeLeft();
            else
                XDebug.LogError("Could not find Orientation object");
#endif
        }

        public void OnClickLandScapeRight()
        {
#if UNITY_EDITOR
            Orientation orientation = Utilities.FindObjectOfType<Orientation>();
            if (orientation != null)
                orientation.SetLandScapeRight();
            else
                XDebug.LogError("Could not find Orientation object");
#endif
        }
    }
}
