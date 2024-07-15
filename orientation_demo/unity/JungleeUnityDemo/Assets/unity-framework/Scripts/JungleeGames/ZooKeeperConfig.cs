using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;
using XcelerateGames;
using JungleeGames.Analytics;
using XcelerateGames.Webrequets;
using System.Net;
using UnityEngine.Networking;

namespace JungleeGames
{
    /// <summary>
    /// A class to fetch Zoo Keeper config.
    /// Note: default channel ID is 10 which is Howzat cash app.
    /// QA: https://api-hzt-inc-qa1.howzat.com/api/gr/app/initdata
    /// Live: https://mercury.howzat.com/api/gr/app/initdata
    /// </summary>
    public class ZooKeeperConfig : BaseBehaviour
    {
        #region Properties
        [SerializeField] protected bool _RunOnEditorOnly = true;
        [SerializeField] protected EnvironmentPath _APIPath = null;
        #endregion //Properties

        #region Signals
        [InjectSignal] private SigFetchZooKeeperConfig mSigFetchZooKeeperConfig = null;
        [InjectSignal] private SigZooKeeperConfigFetched mSigZooKeeperConfigFetched = null;
        #endregion //Signals

        #region UI Callbacks
        #endregion //UI Callbacks

        #region Private Methods
        protected override void Awake()
        {
            if (_RunOnEditorOnly && !Application.isEditor)
                Destroy(gameObject);
            else
            {
                base.Awake();
                mSigFetchZooKeeperConfig.AddListener(FetchConfig);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mSigFetchZooKeeperConfig != null)
            {
                mSigFetchZooKeeperConfig.RemoveListener(FetchConfig);
            }
        }

        protected virtual void FetchConfig(int channelId, string cookie)
        {
            StartCoroutine(GetConfig(channelId, cookie));
        }

        private IEnumerator GetConfig(int channelId, string cookie)
        {
            WWWForm form = new WWWForm();

            using (UnityWebRequest www = UnityWebRequest.Post(_APIPath.Path, form))
            {
                www.SetRequestHeader("channelId", channelId.ToString());
                www.SetRequestHeader("Cookie", cookie);
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    XDebug.LogError($"Failed to fetch ZooKeeper config: {www.error}");
                    mSigZooKeeperConfigFetched.Dispatch(null);
                }
                else
                {
                    Debug.Log("ZookerConfig fetched successfully!");
                    string data = www.downloadHandler.text.GetJsonNode("data");
                    mSigZooKeeperConfigFetched.Dispatch(data);
                    //System.IO.File.WriteAllText("ZooKeeper.txt", www.downloadHandler.text);
                }
            }
        }
        #endregion //Private Methods

        #region Editor
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (_APIPath == null)
                _APIPath = new EnvironmentPath();
            if (_APIPath.IsEmpty())
            {
                _APIPath = new EnvironmentPath();
                _APIPath._Paths = new EnvironmentPathData[2]
                {
                    new EnvironmentPathData() { _Environment = PlatformUtilities.Environment.qa, _Path = "https://api-hzt-inc-qa1.howzat.com/api/gr/app/initdata" },
                    new EnvironmentPathData() { _Environment = PlatformUtilities.Environment.live, _Path = "https://mercury.howzat.com/api/gr/app/initdata" }
                };
            }
        }
#endif //UNITY_EDITOR

        #endregion Editor
    }
}
