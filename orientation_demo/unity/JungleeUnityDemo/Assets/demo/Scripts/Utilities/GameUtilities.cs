using UnityEngine;
using XcelerateGames;

namespace JungleeGames.UnityDemo
{
    public static class GameUtilities
    {
        public static void ToggleFullScreen(bool _isfullscreen)
        {
            if (Screen.fullScreen != _isfullscreen)
            {
                Screen.fullScreen = _isfullscreen;
                if (XDebug.CanLog(XDebug.Mask.Game))
                    XDebug.Log($"Setting full screen from Game Table currently isfullscreen {Screen.fullScreen} change to isfullscreen {_isfullscreen}", XDebug.Mask.Game);
            }
        }
        
        public static void SetGameOrientation(ScreenOrientation _screenOrientation)
        {
            if(Screen.orientation != _screenOrientation)
            {
                if (XDebug.CanLog(XDebug.Mask.Game))
                    Debug.Log($"Changing orientation from Game Table current Orientation {Screen.orientation} change to {_screenOrientation}, deviceOrientation: {Input.deviceOrientation}");
                Screen.orientation = _screenOrientation;
            }
        }
    }
}