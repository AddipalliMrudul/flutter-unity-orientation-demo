#if VIDEO_ENABLED

using UnityEngine;
using UnityEngine.Video;
using XcelerateGames.IOC;

namespace XcelerateGames.Video
{
    public class SigPlayVideo : Signal<string, (string clipURL, VideoClip videoClip, bool adjustSize), ScreenOrientation> { }
    public class SigVideoPlayerCreated : Signal<UiVideoPlayer> { }
    public class SigVideoPlayerClosed : Signal { }
    public class SigVideoPlayerComplete : Signal { }
    public class SigVideoPlaybackStarted : Signal { }
}
#endif //VIDEO_ENABLED
