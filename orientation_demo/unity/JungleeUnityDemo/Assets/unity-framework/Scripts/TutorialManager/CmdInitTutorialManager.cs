using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames.Tutorials
{
    /// <summary>
    /// Class: CmdInitTutorialManager
    /// </summary>
    public class CmdInitTutorialManager : Command
    {
        public override void Execute()
        {
            ResourceManager.LoadById("tutorialMgr", OnTutorialManagerLoaded, ResourceManager.ResourceType.Object);
        }
    
        private void OnTutorialManagerLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                TutorialManager manager = Utilities.Instantiate<TutorialManager>(inObject as GameObject, "PfUiTutorialManager");
                GameObject.DontDestroyOnLoad(manager.gameObject);
                Release();
            }
            else if (inEvent == ResourceEvent.ERROR)
            {
                Release();
                XDebug.LogError("Failed to load asset : " + inURL);
            }
        }
}
}
