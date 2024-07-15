using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    /// <summary>
    /// Destroy Loading Screen. Attach this to an empty GameObject.
    /// </summary>
    public class DestroyLoadingScreen : MonoBehaviour
    {
        private void Awake()
        {
            ResourceManager.OnSceneLoadedEvent += OnSceneWasLoaded1;
        }

        private void OnSceneWasLoaded1(string obj)
        {
            ResourceManager.DestroyLoadingScreen();
            GameObject.Destroy(gameObject);
        }
    }
}
