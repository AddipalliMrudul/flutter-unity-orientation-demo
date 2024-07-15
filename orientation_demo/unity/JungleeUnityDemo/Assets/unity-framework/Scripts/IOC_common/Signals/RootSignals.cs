using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// Bundlename/AssetName, Parent, Show Loading gear?, whether to load asset by name or by id. If id is more than zero then asset is loaded by id, Call back when asset is loaded
    /// </summary>
    public class SigLoadAssetFromBundle : Signal<string, Transform, bool, int, System.Action<GameObject>> { }
    /// <summary>
    /// Dispatched when entire scene is loaded & loading screen is destroyed. Passes loaded scene name as argument
    /// </summary>
    public class SigSceneReady : Signal<string> { }
    /// <summary>
    /// Trigger this signal when core framework components are initialised ex ResourceManager, AudioController etc. The commands & signals excecute only once.
    /// </summary>
    public class SigEngineReady : Signal { }
    /// <summary>
    /// Trigger this signal when core framework components are initialised ex ResourceManager, AudioController etc. The commands & signals excecute everytime this signal is dispatched
    /// </summary>
    public class SigFrameworkInited : Signal { }
    public class SigShowZoomAndPan : Signal<Texture, string> { }
    /// <summary>
    /// Asset URL, Is asset encrypted
    /// </summary>
    public class SigSaveFileToGallery : Signal<string, bool> { }
    public class SigSaveFileToDownloads : Signal<string, ResourceManager.ResourceType, bool/*open after download*/> { }
    public class SigShowToast : Signal<string, bool, ToastLength> { }
    public class SigHideToast : Signal { }
    public class SigShowAudioPlayer : Signal<string> { }
    public class SigRemoteConfigLoaded : Signal<bool> { }

    #region Miscellaneous
    //Every game will have its own way of getting the hash, Each game must create a command or write logic to use this signal
    public class SigGetVersionListHash : Signal<System.Action<bool>> { }
    public class SigShowSettings : Signal { }
    public class SigSettingsHidden : Signal { }
    public class SigCloseGameTable : Signal { }

    public class SigVibrate : Signal<int> { } //passing vibration play time
    public class SigVibratePattern : Signal<long[],bool> { } //passing vibration pattern and cancel


    //Use the command CmdUploadDeviceLogs by deriving from it & set flow only in your game specific bindibfs 
    public class SigUploadDeviceLogs : Signal<Dictionary<string, string>, string, Action<bool>> { }
    /// <summary>
    /// Dispatches everytime app is min-maxed witgh the amount of tiem app was minimised in milliseconds
    /// </summary>
    public class SigAppResumedFromBackground : Signal<double> { }
    public class SigSendFrameworkEvent : Signal<string> { }
    public class SigOnSwipe : Signal<SwipeDirection> { }
    #endregion Miscellaneous

    //#region Notification Related
    public class SigOnGetNotification : Signal<Notification> { } //get and show notification
    //  public class SigClearNotificationList : Signal<List<string>> { } //remove every notification which contains id from the list
    //   public class SigClearNotification : Signal<string> { }  //remove single notification by id
    public class SigClearAllNotfiication : Signal { } //remove all notifications
                                                      //#endregion

    public class SigOnGetPong : Signal { }
    //public class SigServerConnectionClose : Signal { }
    public class SigOnPingCountUpdate : Signal<int> { }
    public class SigOnDisconnection : Signal { }
    //public class SigGameQuit : Signal { }
    public class SigOnReconnect : Signal { }
    public class SigOnPingSend : Signal { }

    #region ZooKeeper
    public class SigFetchZooKeeperConfig : Signal<int/*channelId*/, string/*cookie*/> { }
    public class SigZooKeeperConfigFetched : Signal<string/*config*/> { }
    #endregion ZooKeeper

#if FIREBASE_CRASHLYTICS_ENABLED
    public class SigInitFirebase : Signal { }
#endif //FIREBASE_CRASHLYTICS_ENABLED

    #region Debug
    public class SigSendMailLog : Signal<bool, (string email, string subject, string message)> { }
    #endregion

#if BACKTRACE_ENABLED
    public class SigAddBacktraceAttribute : Signal<string, string> { }
#endif
    public class SigSendBreadcrumbEvent : Signal<string, Dictionary<string, object>> { }
    public class SigAssetBundleDownloaderState:Signal<AssetDownloadData>{}
}
