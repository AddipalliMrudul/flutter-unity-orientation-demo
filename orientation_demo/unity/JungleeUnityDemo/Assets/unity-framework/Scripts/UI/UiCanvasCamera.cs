using UnityEngine;
using UnityEngine.SceneManagement;

namespace XcelerateGames.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UiCanvasCamera : MonoBehaviour
    {
        private Canvas mCanvas = null;

        void Awake()
        {
            mCanvas = GetComponent<Canvas>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (mCanvas != null)
                mCanvas.worldCamera = Camera.main;
        }

        private void OnDestroy()
        {
            if (mCanvas != null)
            {
                if (mCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Set")]
        private void Set()
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
#endif
    }
}
