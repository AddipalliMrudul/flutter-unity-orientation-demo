namespace XcelerateGames.IOC
{
    [System.Serializable]
    public enum ArgumentType
    {
        None,
        Int,
        Float,
        String,
        Bool,
    }

    [System.Serializable]
    public class ArgumentData
    {
        public string _Data = null;
        public ArgumentType _Type = ArgumentType.None;
    }
}
