using UnityEngine;
using XcelerateGames.IOC;

//Demo1
namespace XcelerateGames.IOCDemo
{
    public class CmdTest0Arg : Command
    {
        public override void Execute()
        {
            Debug.LogError("CmdTest0Arg Execute called");
            base.Execute();
        }
    }
}
