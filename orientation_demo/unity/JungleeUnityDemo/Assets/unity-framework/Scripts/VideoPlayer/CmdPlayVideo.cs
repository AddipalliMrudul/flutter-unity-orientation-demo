#if VIDEO_ENABLED

using UnityEngine;
using UnityEngine.Video;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames.Video
{
    public class CmdPlayVideo : Command
    {
        [InjectParameter] protected string mAssetName = null;
        [InjectParameter] protected (string clipURL, VideoClip videoClip, bool adjustSize) mData = (null, null, false);
        [InjectParameter] protected ScreenOrientation mScreenOrientation = ScreenOrientation.AutoRotation;

        public override void Execute()
        {
            if (IsDataValid())
            {
                UiVideoPlayer videoPlayer = Init();
                if (videoPlayer != null)
                {
                    if (mData.videoClip != null)
                        videoPlayer.Play(mData.videoClip, mScreenOrientation, mData.adjustSize);
                    else
                        videoPlayer.Play(mData.clipURL, mScreenOrientation, 1, mData.adjustSize);
                }
                else
                    XDebug.LogError("videoPlayer is null");
            }
            base.Execute();
        }

        /// <summary>
        /// Checks if the data for playing a video is valid.
        /// </summary>
        /// <returns><c>true</c> if the data is valid; otherwise, <c>false</c>.</returns>
        protected virtual bool IsDataValid()
        {
            bool isValid = false;
            if (mData.videoClip == null && mData.clipURL.IsNullOrEmpty())
                XDebug.LogError("Either Video clip or URL to the video clip must be provided");
            else if (mAssetName.IsNullOrEmpty())
                XDebug.LogError("Video player asset name cannot be null");
            else
                isValid = true;
            return isValid;
        }

        protected virtual UiVideoPlayer Init()
        {
            //Load this from resources.
            GameObject go = ResourceManager.LoadFromResources<GameObject>(mAssetName);
            if (go != null)
            {
                return GameObject.Instantiate(go).GetComponent<UiVideoPlayer>();
            }
            else
                XDebug.LogError($"Failed to load {mAssetName}");
            return null;
        }
    }
}
#endif //VIDEO_ENABLED
