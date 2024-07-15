using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.IOCDemo
{
    public class CmdTestSequence2 : Command
    {
        [InjectParameter] private int mInt = 0;

        public override void Execute()
        {
            Debug.LogFormat("{0} Execute called {1}", this, mInt);
            if (Random.Range(0, 100) > 50)
                Abort();
            else
                Release();
        }
    }
}
