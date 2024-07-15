using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.IOCDemo
{
    public class CmdTestFinal : Command
    {
        public override void Execute()
        {
            Debug.LogErrorFormat("CmdTestFinal called {0} {1}", this, Time.frameCount);
            base.Execute();
        }
    }
}
