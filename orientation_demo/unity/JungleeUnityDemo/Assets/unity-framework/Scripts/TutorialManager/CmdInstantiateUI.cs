using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Tutorials
{
    public class CmdInstantiateUI : Command
    {
        [InjectParameter] string mAssetPath = null;
        [InjectParameter] bool mShowLoadingGear = false;

        public override void Execute()
        {
            base.Execute();
            if (mShowLoadingGear)
                UiLoadingCursor.Show(true);
            ResourceManager.Load(mAssetPath, OnBundleLoaded, ResourceManager.ResourceType.Object);
        }

        private void OnBundleLoaded(ResourceEvent inEvent, string inUrl, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;

            if (mShowLoadingGear)
                UiLoadingCursor.Show(false);

            if (inEvent == ResourceEvent.COMPLETE)
            {
                UiBase viewTwoCardMinigame = Utilities.Instantiate<UiBase>(inObject as GameObject, inUrl.Split('/')[1], null);
                viewTwoCardMinigame.Show();
                Release();
            }
        }
    }
}
