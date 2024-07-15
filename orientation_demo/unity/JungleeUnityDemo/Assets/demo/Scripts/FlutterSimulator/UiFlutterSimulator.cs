using System.Diagnostics;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace JungleeGames.UnityDemo
{   
    public class UiFlutterSimulator : UiBase
    {
#if UNITY_EDITOR
        [InjectSignal] private SigOnFlutterMessage mSigOnFlutterMessage = null; 
#endif
        public void SimulateInitMessageFromFlutter()
        {
#if UNITY_EDITOR
            mSigOnFlutterMessage.Dispatch(new FlutterMessage() { type = FlutterMessageType.Init });
#endif
        }
    }
}
