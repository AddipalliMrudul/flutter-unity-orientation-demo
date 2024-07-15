using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;

namespace JungleeGames.UnityDemo
{
    public class GameBindings : BindingManager
    {
        protected override void SetBindings()
        {
            base.SetBindings();

            // Signals
            BindSignal<SigLoadInitUI>();
            BindSignal<SigOpenLobby>();
            BindSignal<SigInitDone>();
            
            // Models
            BindModel<GameDataModel>();
        }

        protected override void SetFlow()
        {
            base.SetFlow();
            On<SigLoadInitUI>().Do<CmdLoadStartUpUI>();
            On<SigOnFlutterMessage>().Do<CmdOnFlutterMessage>();
        }
        
        protected override void OnBindingsComplete()
        {
            base.OnBindingsComplete();
            AddRoot();
        }
        
        protected override void OnDestroy()
        {
            On<SigOnFlutterMessage>().Undo<CmdOnFlutterMessage>();
            base.OnDestroy();
        }
    }
}