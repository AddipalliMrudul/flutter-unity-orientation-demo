using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames
{
    public class CmdShowZoomAndPan : Command
    {
        [InjectParameter] private Texture mTexture = null;
        [InjectParameter] private string mUrl = null;

        public override void Execute()
        {
            ResourceManager.Load("zoomandpan/PfUiZoomAndPan", OnBundleLoaded, ResourceManager.ResourceType.Object);
        }

        private void OnBundleLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                UiZoomAndPan zoomAndPan = Utilities.Instantiate<UiZoomAndPan>(inObject, "PfUiZoomAndPan");
                zoomAndPan._Image.SetTexture(mTexture);
                zoomAndPan._Url = mUrl;

                Release();
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                Release();
            }
        }
    }
}
