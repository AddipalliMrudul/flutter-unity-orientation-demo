using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.IOCDemo
{
    public class CmdTestAbort : Command
    {
        public override void Execute()
        {
            Debug.LogErrorFormat("CmdTestAbort called {0} {1}", this, Time.frameCount);
            base.Execute();
        }
    }
}
