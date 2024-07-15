
using XcelerateGames.AssetLoading;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;

namespace JungleeGames.UnityDemo
{
    public class CmdLoadStartUpUI : Command
    {
        [InjectModel] private GameDataModel mGameDataModel = null;
        [InjectSignal] private SigSendMessageToFlutter mSigSendMessageToFlutter = null;
        
        public override void Execute()
        {
            ResourceManager.Load("card", OnAssetLoaded, ResourceManager.ResourceType.AssetBundle,downloadOnly:true);
        }
        
        private void OnAssetLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.PROGRESS)
                return;

            mGameDataModel.IsUnityReady = true;
            mSigSendMessageToFlutter.Dispatch(new FlutterMessage() { type = MessageType.UnityReady });
            base.Execute();
        }
    }
}
