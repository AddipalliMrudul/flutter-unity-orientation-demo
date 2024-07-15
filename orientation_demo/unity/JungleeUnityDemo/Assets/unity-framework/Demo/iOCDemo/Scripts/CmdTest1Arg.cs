﻿using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.IOCDemo
{
    public class CmdTest1Arg : Command
    {
        [InjectParameter] private int mInt = 0;

        public override void Execute()
        {
            base.Execute();
            Debug.LogErrorFormat("CmdTest1Arg Execute called {0}", mInt);
        }
    }
}
