using System;
using XcelerateGames.UI;

namespace XcelerateGames
{
    [Serializable]
    public class InteractiveState
    {
        public UiItem _Object = null;
        public bool _IsInteractive = true;

        public void Apply()
        {
            if (_Object != null)
                _Object.SetInteractive(_IsInteractive);
        }
    }
}
