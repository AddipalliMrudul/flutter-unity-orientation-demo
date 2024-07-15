﻿using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.IOCDemo
{
    public class CmdTest2Arg : Command
    {
        [InjectParameter] private int mInt = 0;
        [InjectParameter] private bool mBool = false;

        public override void Execute()
        {
            base.Execute();
            Debug.LogErrorFormat("CmdTest2Arg Execute called {0}, {1}", mInt, mBool);
        }
    }
}
