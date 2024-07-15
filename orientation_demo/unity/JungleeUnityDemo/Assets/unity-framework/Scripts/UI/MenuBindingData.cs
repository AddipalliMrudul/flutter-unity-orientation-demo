namespace XcelerateGames.UI
{
    [System.Serializable]
    public enum BindingType
    {
        None,
        Int,
        Float,
        String,
        LocalizedString,
        Texture,
        Sprite,
    }

    [System.Serializable]
    public class BindingData
    {
        public string _Name = null;
        public BindingType _Type = BindingType.None;
        public UiItem _Item = null;
    }
}
