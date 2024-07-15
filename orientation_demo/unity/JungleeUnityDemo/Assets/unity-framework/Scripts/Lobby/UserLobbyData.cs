using static XcelerateGames.PlatformUtilities;

namespace XcelerateGames.Lobby
{
    [System.Serializable]
    public class UserLobbyData
    {
        public Environment environment; 
        public string name;
        public long id;
        public string lat;
        public string lng;
        public int avatarId;
        [UnityEngine.TextArea(2, 3)] public string cookie;
    }
}