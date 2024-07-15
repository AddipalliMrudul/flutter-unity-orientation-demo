using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.IOCDemo
{
    public class CmdTestSequence3 : Command
    {
        [InjectParameter] private int mInt = 0;

        public override void Execute()
        {
            Debug.LogFormat("{0} Execute called {1}", this, mInt);
            base.Execute();
        }
    }
}
