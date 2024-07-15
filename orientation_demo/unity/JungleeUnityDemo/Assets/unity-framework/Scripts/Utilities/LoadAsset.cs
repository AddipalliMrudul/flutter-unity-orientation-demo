using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    public class LoadAsset : MonoBehaviour
     {
        #region Properties
        public bool _ShowLoadingGear = false;
        public string _AssetPath = null;
        public ResourceManager.ResourceType _ResourceType = ResourceManager.ResourceType.Object;
        #endregion //Properties

        #region UI Callbacks
        public void LoadFromBundle()
        {
            if (_ShowLoadingGear) UiLoadingCursor.Show(true);
            ResourceManager.Load(_AssetPath, OnAssetBundleLoaded, _ResourceType);
        }

        public void LoadFromResources()
        {
            OnObjectLoaded(ResourceManager.LoadFromResources<GameObject>(_AssetPath));
        }
        #endregion //UI Callbacks

        #region Private Methods
        private void OnAssetBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;
            if (_ShowLoadingGear) UiLoadingCursor.Show(false);

            if (inEvent == ResourceEvent.COMPLETE)
            {
                OnObjectLoaded(inObject as GameObject);
            }
        }

        private void OnObjectLoaded(GameObject obj)
        {
            obj = Instantiate(obj);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
        }

        #endregion //Private Methods

        #region Public Methods
        #endregion //Public Methods
    }
}
