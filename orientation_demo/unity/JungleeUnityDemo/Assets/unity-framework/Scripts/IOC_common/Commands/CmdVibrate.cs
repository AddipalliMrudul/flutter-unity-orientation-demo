using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// Command of SigVibrate
    /// Make phone vibrate
    /// </summary>
    public class CmdVibrate : Command
    {
        [InjectParameter] private int mVibrationTime;

        public override void Execute()
        {
            Vibration.Vibrate(mVibrationTime);
            base.Execute();
        }
    }
}
