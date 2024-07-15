using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using System;

namespace XcelerateGames
{
    public class AssetBundleData
    {
        public Transform _Parent = null;
        public bool _ShowLoadingGear = false;
        public Action<GameObject> _Callback = null;
    }

    public class CmdLoadAssetFromBundle : Command
    {
        [InjectParameter] private string mAssetPath = null;
        [InjectParameter] private Transform mParent = null;
        [InjectParameter] private bool mShowLoadingGear = false;
        [InjectParameter] private int mLoadById = 0;
        [InjectParameter] private Action<GameObject> mCallback = null;

        public override void Execute()
        {
            if (mShowLoadingGear)
                UiLoadingCursor.Show(true);
            AssetBundleData data = new AssetBundleData();
            data._Parent = mParent;
            data._ShowLoadingGear = mShowLoadingGear;
            data._Callback = mCallback;
            if (mLoadById > 0)
                mAssetPath = AssetConfigData.GetAssetName(mAssetPath);
            ResourceManager.Load(mAssetPath, OnBundleLoaded, ResourceManager.ResourceType.Object, inUserData: data);
        }

        private void OnBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;

            AssetBundleData data = (AssetBundleData)inUserData;
            if (data._ShowLoadingGear)
                UiLoadingCursor.Show(false);

            if (inEvent == ResourceEvent.COMPLETE)
            {
#if HANDLE_EXCEPTIONS
                try
#endif
                {
                    GameObject gameObject = GameObject.Instantiate(inObject as GameObject, data._Parent);
                    BaseBehaviour baseClass = gameObject.GetComponent<BaseBehaviour>();
                    if (baseClass != null)
                    {
                        baseClass._AssetBundle = inURL;
                    }
                    gameObject.name = inURL.Split('/')[1];
                    data._Callback?.Invoke(gameObject);
                    data._Callback = null;
                    Release();
                }
#if HANDLE_EXCEPTIONS
                catch (Exception ex)
                {
                    XDebug.LogException($"Exception while loading: {inURL}, Object:{inObject}, userData:{data}, Message: {ex.Message}, Exception: {ex.InnerException}");
                    OnError();
                }
#endif
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                OnError();
            }

            void OnError()
            {
                data._Callback?.Invoke(null);
                data = null;
                Release();
            }
        }
    }
}
