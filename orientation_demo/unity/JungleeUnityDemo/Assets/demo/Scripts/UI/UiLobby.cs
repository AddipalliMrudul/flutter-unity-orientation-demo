using System;
using Unity.VisualScripting;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace JungleeGames.UnityDemo
{    
    public class UiLobby : UiMenu
    {
#if UNITY_EDITOR
        [InjectSignal] private SigOpenLobby mSigOpenLobby = null;
#endif
        
        protected override void Start()
        {
#if UNITY_EDITOR
            Show();
            mSigOpenLobby.AddListener(OnOpenLobby);
            
            base.Start();
#else
            Hide();
#endif
        }

        private void OnOpenLobby()
        {
            Show();
        }

        public void OnPlayButtonSelected()
        {
            Hide();
        }
    }
}