#if UNITY_EDITOR || LOBBY_SIMULATOR
using System.Linq;
using JungleeGames;
using UnityEngine;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Lobby
{
    public class UiLobby : UiBase
    {
        protected const string ActiveLocalUserKey = "ActiveUserIndex";
        #region Properties
        [SerializeField] private UserLobbyData[] _Users = null;
        [SerializeField] private TMPro.TMP_Dropdown _UserDropdown = null;

        [InjectSignal] protected SigOnFlutterMessage mSigOnFlutterMessage = null;
        [InjectSignal] protected SigSendMessageToFlutter mSigSendMessageToFlutter = null;


        protected UserLobbyData pActiveUserData => _Users.Length > 0 ? _Users[Mathf.Clamp(PlayerPrefs.GetInt(GetActiveLocalUserKey(), 0), 0, _Users.Length - 1)] : null;
        #endregion


        #region Ui Callbacks
        /// <summary>
        /// Called & used only in umbrella project to send user back to game selection
        /// </summary>
        public virtual void OnClickBackToHome()
        {
#if UMBRELLA
            mSigSendMessageToFlutter.Dispatch(new FlutterMessage() { type = FlutterMessageType.GameEnd });
#endif
            Hide();
        }
        #endregion

        #region Protected Methods
        protected virtual string GetActiveLocalUserKey() => ActiveLocalUserKey;

        protected override void Start()
        {
            base.Start();
            LoadUserDetails();
        }

        protected override void OnDestroy()
        {
            if (_Users.Length > 0)
                _UserDropdown.onValueChanged.RemoveAllListeners();
            base.OnDestroy();
        }

        private void LoadUserDetails()
        {
            _Users = _Users.Where(x => x.environment == PlatformUtilities.GetEnvironment()).ToArray();
            if (_Users.Length == 0)
            {
                _UserDropdown.gameObject.SetActive(false);
                XDebug.LogWarning($"No user lobby data found in lobby for {PlatformUtilities.GetEnvironment()} environment");
                return;
            }
            _UserDropdown.ClearOptions();
            _UserDropdown.SetActive(_Users.Length > 0);
            if (_Users.Length > 0)
            {
                _UserDropdown.AddOptions(_Users.Select(x => x.name).ToList());
                _UserDropdown.value = -1;
                int selectedUserIndex = Mathf.Clamp(PlayerPrefs.GetInt(GetActiveLocalUserKey(), 0), 0, _Users.Length - 1);
                _UserDropdown.value = selectedUserIndex;
                _UserDropdown.onValueChanged.AddListener(OnChangeUserData);
                OnChangeUserData(selectedUserIndex);
            }
        }

        protected virtual void OnChangeUserData(int index)
        {
            PlayerPrefs.SetInt(GetActiveLocalUserKey(), index);
            PlayerPrefs.Save();
        }
        #endregion
    }
}
#endif // UNITY_EDITOR || LOBBY_SIMULATOR