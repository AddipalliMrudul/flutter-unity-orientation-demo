namespace XcelerateGames.UI
{
    public class UiItemData
    {
        public UiItem _Item = null;
        public int _Index;

        public UiItemData()
        {
            _Index = -1;
        }

        public UiItemData(int i)
        {
            _Index = i;
        }

        public UiItemData(int i, UiItem inItem)
        {
            _Index = i;
            _Item = inItem;
        }

        public UiItem GetItem()
        {
            return _Item;
        }

        public virtual void Destroy()
        {
        }
    }
}