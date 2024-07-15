namespace JungleeGames
{
    /// <summary>
    /// Product ID of all games under JungleeGames
    /// </summary>
    public enum Product
    {
        None = 0,
        Howzat = 1,
        SolitaireGold = 2,
        Rummy = 3,
        Carrom = 4,
        Poker = 5,
        Ludo = 6,
        Teenpatti = 7,
        End
    }

    /// <summary>
    /// Channel ID`s of our various games
    /// </summary>
    public enum ChannelId
    {
        None = 0,

        Howzat_Cash_Android = 10,
        Howzat_PS_Android = 20,
        Howzat_iOS = 13,

        Poker_Cash_Android = 401,
        Poker_PS_Android = 405,
        Poker_iOS = 402,

        SolitaireGold_Cash_Android = 10,
        SolitaireGold_PS_Android = 10,
        SolitaireGold_iOS = 101,

        Carrom_Cash_Android = 301,
        Carrom_PS_Android = 305,
        Carrom_iOS = 302,

        Ludo_Cash_Android = 601,
        Ludo_PS_Android = 605,
        Ludo_iOS = 602,

        Teenpatti_Cash_Android = 701,
        Teenpatti_PS_Android = 705,
        Teenpatti_iOS = 702,
    }

    public enum GameType
    {
        None,
        AndroidFree,
        AndroidCash,
        iOS,
    }

    [System.Serializable]
    public class ChannelIDData
    {
        public GameType _GameType = GameType.None;
        public ChannelId _ChannelId = ChannelId.None;
    }
}
