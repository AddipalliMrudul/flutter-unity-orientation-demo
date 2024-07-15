using UnityEngine.UI;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.Keyboard
{
    public class DoneKey : UiItem
    {
        #region Signals
        [InjectSignal] private SigHideKeyboard mSigHideKeyboard = null;
        #endregion //Signals

       public override void OnClicked()
        {
            base.OnClicked();
            mSigHideKeyboard.Dispatch();
        }
    }
}
