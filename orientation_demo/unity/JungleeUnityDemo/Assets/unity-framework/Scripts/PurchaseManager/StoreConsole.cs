#if USE_IAP
using XcelerateGames.IOC;
using XcelerateGames.Purchasing;

namespace XcelerateGames
{
    public class StoreConsole : BaseBehaviour, IConsole
    {
        [InjectSignal] SigPurchaseItem mSigPurchaseItem = null;

        private void Start()
        {
            UiConsole.Register("store", this);
        }

        public bool OnExecute(string[] args, string baseCommand)
        {
            if (Utilities.Equals(args[0], "buy"))
            {
                if (args.Length == 2)
                {
                    mSigPurchaseItem.Dispatch(args[1]);
                }
                else
                    UiConsole.WriteLine("Invalid no of args, usage store buy productId");
            }
            return true;
        }

        public void OnHelp()
        {
        }
    }
}
#endif //USE_IAP