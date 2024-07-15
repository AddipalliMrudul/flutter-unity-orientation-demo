using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// Command of SigVibrate
    /// Make phone vibrate in pattern
    /// mPattern -> Input vibration pattern
    /// mCancel -> To stop the running vibration
    /// </summary>
    public class CmdVibratePattern : Command
    {
        [InjectParameter] private long[] mPattern;
        [InjectParameter] private bool mCancel;

        public override void Execute()
        {
            Vibration.Vibrate(mPattern, mCancel);
            base.Execute();
        }
    }
}
