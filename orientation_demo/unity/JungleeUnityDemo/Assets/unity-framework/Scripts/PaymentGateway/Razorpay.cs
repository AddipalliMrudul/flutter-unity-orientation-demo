namespace XcelerateGames.PaymentGateway
{
    public class RazorpayPaymentSuccess
    {
        public string razorpay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }
    }

    public class RazorpayPaymentFailed
    {
        public string code { get; private set; }
        public string description { get; private set; }
        public RazorpayPaymentFailedMetaData metadata { get; private set; }
    }

    public class RazorpayPaymentFailedMetaData
    {
        public string order_id { get; private set; }
    }
}
