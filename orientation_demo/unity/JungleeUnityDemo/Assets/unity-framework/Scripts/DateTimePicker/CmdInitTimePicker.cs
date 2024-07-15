using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.UI
{
    public class CmdInitTimePicker : Command
    {
        public override void Execute()
        {
            base.Execute();
            Debug.LogError("CmdInitTimePicker Execute called");
        }
    }
}
