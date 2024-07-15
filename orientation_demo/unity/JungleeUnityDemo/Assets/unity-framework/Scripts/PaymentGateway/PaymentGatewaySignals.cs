using XcelerateGames.IOC;

namespace XcelerateGames.PaymentGateway
{
    public class SigStartPayment : Signal<(int /*amount*/, string /*mobile num*/, string /*email*/, string/*description*/, string /*order_id*/)> { }
    public class SigPaymentCompleted : Signal<string/*payment id*/> { }
    public class SigVerifyPayment : Signal<RazorpayPaymentSuccess> { }
    public class SigPaymentFailed : Signal<string> { }
    //public class SigPaymentCancelled : Signal<string> { }
}
