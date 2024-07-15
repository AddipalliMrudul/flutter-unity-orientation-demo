using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JungleeGames
{
    public enum Game
    {
        None,
        Rummy,
        GoldRush,
        Carrom,
        Poker,
        Ludo,
        Teenpatti,
        End
    }

    public class InitMinigame
    {
        //[JsonProperty("game")] [JsonConverter(typeof(StringEnumConverter))] public Game game = Game.None;
        [JsonProperty("game")] public Game game = Game.None;
        [JsonProperty("preload")] public bool preload = false;
    }

    public class MinigameReady
    {
        //[JsonProperty("game")] [JsonConverter(typeof(StringEnumConverter))] public Game game = Game.None;
        [JsonProperty("game")] public Game game = Game.None;
        [JsonProperty("scene")] public string scene;
    }
}