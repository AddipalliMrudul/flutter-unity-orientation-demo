using UnityEngine;
using XcelerateGames.AssetLoading;
namespace XcelerateGames
{
    /// <summary>
    /// Load a scene after the given delay
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] protected string _SceneName = null;  /**< Name of the scene to load*/
        [SerializeField] protected float _Delay = 0.5f;       /**< Delay after which to load the scene*/

        /// <summary>
        /// Initiate scene loading with delay
        /// </summary>
        private void Awake()
        {
            Invoke(nameof(LoadSene), _Delay);
        }

        /// <summary>
        /// Start Loading the scene
        /// </summary>
        protected virtual void LoadSene()
        {
            ResourceManager.LoadScene(_SceneName);
        }
    }
}
