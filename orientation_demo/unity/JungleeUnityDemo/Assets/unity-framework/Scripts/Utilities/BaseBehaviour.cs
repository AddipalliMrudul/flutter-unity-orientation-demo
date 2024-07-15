using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// Base class for all classes. If using IOC framwork, every class must derive from this class.
    /// It handles all dependecy injections
    /// </summary>
    public class BaseBehaviour : MonoBehaviour
    {
        public bool _UnloadAssetBundle = false;
        public string _AssetBundle = null;

        public bool IsDestroying { get; set; }

        protected virtual void Awake()
        {
            InjectBindings.Inject(this);
        }

        protected virtual void OnDestroy()
        {
            if (_UnloadAssetBundle && !_AssetBundle.IsNullOrEmpty())
            {
                ResourceManager.Unload(_AssetBundle);
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            MonoBehaviour[] monoBehaviour = GetComponents<MonoBehaviour>();
            if(monoBehaviour.Length == 1 && gameObject.name == "GameObject")
            {
                gameObject.name = this.GetType().ToString().Split('.').Last();
            }
        }
#endif //UNITY_EDIOR
    }
}
