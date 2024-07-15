using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class CmdShowAudioPlayer : Command
    {
        [InjectParameter] private string mUrl = null;

        [InjectSignal] private SigLoadAssetFromBundle mSigLoadAssetFromBundle = null;

        public override void Execute()
        {
            mSigLoadAssetFromBundle.Dispatch("audioplayer/PfUiAudioPlayer", null, true, 0, OnAudioPlayerObjLoaded);
        }

        private void OnAudioPlayerObjLoaded(GameObject obj)
        {
            UiAudioPlayer audioPlayer = obj.GetComponent<UiAudioPlayer>();
            audioPlayer.Init(mUrl);
        }
    }
}
