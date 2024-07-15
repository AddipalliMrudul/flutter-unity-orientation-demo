namespace JungleeGames
{
    public static class FlutterMessageType
    {
        //Used in Umbrella project
        //Will be sent from flutter to inform Unity which minigame(Rummy, GoldRush, Carrom etc) to be loaded. 
        public const string InitMinigame = "InitMinigame";
        //Sent from Unity to flutter to notify that unity has loaded the game specific scene & is ready to start
        public const string MinigameReady = "MinigameReady";

        //Sent from each game that the minigame is complete. THis is further used by Umbrella logic to control scene transitions
        public const string MinigameEnd = "MinigameEnd";
        /// <summary>
        /// Destroy all objects from the scene,when this received from then flutter.
        /// After this call goto lobby
        /// </summary>
        public const string CloseGameTable = "CloseGameTable";

        //Sent from each game that the minigame is complete. THis is further used by Umbrella logic to control scene transitions
        public const string BackToLobby = "BackToLobby";
        public const string UnityProcessIsReady = "UnityProcessIsReady";

        public const string AnalyticsEvent = "AnalyticsEvent";

        //GR
        public const string GameTableFTUEComplete = "GameTableFTUEComplete";
        public const string GameTableFTUERewardFailed = "GameTableFTUERewardFailed";
        public const string GameTableFTUESkipped = "GameTableFTUESkipped";
        public const string PlayAgain = "PlayAgain";

        //Rummy
        public const string FillDetails = "FillDetails";
        public const string ShowUI = "ShowUI";
        public const string TableQuited = "TableQuited";

        //Common for all games
        public const string Init = "Init";
        public const string ShowReportProblem = "ShowReportProblem";

        //Sent from Unity inform flutter that the game is complete
        public const string GameStart = "GameStart";
        public const string GameEnd = "GameEnd";
        public const string IsUnityReady = "IsUnityReady";
        public const string UnityReady = "UnityReady";
        public const string KeyDown = "KeyDown";
        public const string FrameworkEvent = "FrameworkEvent";
        public const string ShowUiOverlay = "ShowUiOverlay";
        /// <summary>
        /// This message is being send from flutter end,
        /// On this message, on going game-table will close
        /// </summary>
        public const string EndMiniGameRequest = "EndMiniGameRequest";

        //Poker : remove this later
        public const string ReBuyAmount = "ReBuyAmount";
        /// <summary>
        /// will use for genral communication to flutter
        /// </summary>
        public const string GenericMessage = "GenericMessage";
    }
}
