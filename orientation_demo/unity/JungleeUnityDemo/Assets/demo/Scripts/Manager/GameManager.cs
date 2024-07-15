
using UnityEngine;
using XcelerateGames;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace JungleeGames.UnityDemo
{
    public class GameManager : BaseBehaviour
    {
        [InjectSignal] private SigLoadInitUI mSigLoadInitUI = null;
        [InjectSignal] private SigSendMessageToFlutter mSigSendMessageToFlutter = null;
        [InjectSignal] private SigLoadAssetFromBundle mSigLoadAssetFromBundle = null;
        [InjectSignal] private SigOpenLobby mSigOpenLobby = null;
        
        [SerializeField] private GameObject _Table = null;
        
        void Start()
        {
            mSigLoadInitUI.Dispatch();
            mSigLoadAssetFromBundle.Dispatch("uicard", null, false, 1, OnCardPrefabLoaded);
        }

        private void OnCardPrefabLoaded(GameObject obj)
        {
            Instantiate(obj, _Table.transform);
        }
        
        public void LeaveGameTable()
        {
            GameUtilities.SetGameOrientation(ScreenOrientation.Portrait);
            mSigSendMessageToFlutter.Dispatch(new FlutterMessage() { type = MessageType.GameEnd });
            mSigOpenLobby.Dispatch();
        }
    }
}
