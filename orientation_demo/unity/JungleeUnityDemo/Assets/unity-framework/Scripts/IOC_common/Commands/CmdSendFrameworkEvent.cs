using JungleeGames;
using UnityEngine;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class CmdSendFrameworkEvent : Command
    {
        [InjectParameter] private string mEvent = null;

        [InjectSignal] private SigSendMessageToFlutter mSigSendMessageToFlutter = null; 

        public override void Execute()
        {
            mSigSendMessageToFlutter.Dispatch(new FlutterMessage() { type = FlutterMessageType.FrameworkEvent, data = mEvent });
            base.Execute();
        }
    }
}
