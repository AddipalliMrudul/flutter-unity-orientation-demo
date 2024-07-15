#if USE_IAP
using System;
using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames.Purchasing
{
    public class SigPurchaseItem : Signal<string> { }
    public class SigInitPurchaseManager : Signal<List<StoreItem>> { } //Store bundle Ids
    public class SigPurchaseStarted : Signal<string> { }
    public class SigPurchaseCompleted : Signal<string> { }
    public class SigPurchaseFailed : Signal<string> { }
    public class SigPurchaseCancelled : Signal<string> { }
    public class SigRestorePurchases : Signal { }
    public class SigPurchaseInitFailed : Signal<IAPInitializationFailureReason> { }
    public class SigIAPSubscriptionInfo : Signal<IAPSubscriptionInfo> { }
    public class SigPurchaseManagerReady : Signal<bool> { }
    //Triggers cancel subscription process in Android. iOS to be tested.
    public class SigCancelSubscription : Signal<string> { }
    public class SigDoPurchaseVerification : Signal<UnityEngine.Purchasing.Product, Action<bool>> { }
    public class SigSetPurchaseVerification : Signal<Func<string, bool>> { }
}
#endif //USE_IAP