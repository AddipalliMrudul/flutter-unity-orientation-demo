using UnityEngine;
using XcelerateGames.AssetLoading;

public class ResExample : MonoBehaviour
{
    void Start()
    {
        ResourceManager.Load("assetbundlename/assetname", OnAssetBundleLoaded, ResourceManager.ResourceType.Object);
    }

    private void OnAssetBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
    {
        if (inEvent == ResourceEvent.COMPLETE)
        {
            //Instantiate object here.
        }
    }
}
