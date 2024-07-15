using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames.PaymentGateway
{
    public class PaymentGatewayManager : BaseBehaviour
    {
#if UNITY_EDITOR
        public string _Order_id, _Payment_id, _Signature;
#endif
        static private AndroidJavaObject mCurrentActivity = null;

        [InjectSignal] private SigStartPayment mSigStartPayment = null;
        [InjectSignal] private SigVerifyPayment mSigVerifyPayment = null;
        [InjectSignal] private SigPaymentFailed mSigPaymentFailed = null;

        protected override void Awake()
        {
            base.Awake();
            //Do not change the name
            gameObject.name = "PaymentGatewayManager";
            DontDestroyOnLoad(gameObject);
            Init();
        }

        private void Start()
        {
#if !LIVE_BUILD && !BETA_BUILD
            XDebug.AddMask(XDebug.Mask.Purchasing);
#endif
            mSigStartPayment.AddListener(StartPayment);
        }

        protected override void OnDestroy()
        {
            mSigStartPayment.RemoveListener(StartPayment);
            base.OnDestroy();
        }

        private void Init()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (jc != null)
            {
                mCurrentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                if (mCurrentActivity == null)
                    XDebug.LogException("Could not find Current Activity! This is bad");
                else
                {
                    Debug.Log("Initialized PaymentGatewayManager.");
                }
            }
            else
                XDebug.LogException("Could not find JavaClass : com.unity3d.player.UnityPlayer");
        }

        private void StartPayment((int amount, string mobileNumber, string email, string description, string orderId) data)
        {
#if UNITY_EDITOR
            mSigVerifyPayment.Dispatch(new RazorpayPaymentSuccess()
            {
                razorpay_order_id = _Order_id,
                razorpay_payment_id = _Payment_id,
                razorpay_signature = _Signature
            });
#elif UNITY_ANDROID
            if (mCurrentActivity == null)
            {
                XDebug.LogException($"StartPayment:: mCurrentActivity is null");
                return;
            }
            bool result = mCurrentActivity.Call<bool>("StartPayment", data.amount, data.mobileNumber, data.email, data.description, data.orderId, PlatformUtilities.GetEnvironment().ToString());
            //If there is any exception while invoking function in Java, it returns false, only in that case we trigger failed event
            if(!result)
            {
                mSigPaymentFailed.Dispatch("");
            }
#endif
        }

#region Callback from native OS
        private void OnPaymentSuccessRazorPay(string jsonData)
        {
            if(XDebug.CanLog(XDebug.Mask.Purchasing))
                XDebug.Log($"OnPaymentSuccessRazorPay: {jsonData}", XDebug.Mask.Purchasing);
            RazorpayPaymentSuccess response = jsonData.FromJson<RazorpayPaymentSuccess>();
            mSigVerifyPayment.Dispatch(response);
        }

        private void OnPaymentErrorRazorPay(string jsonData)
        {
            if(XDebug.CanLog(XDebug.Mask.Purchasing))
                XDebug.Log($"OnPaymentErrorRazorPay: {jsonData}", XDebug.Mask.Purchasing);
            mSigPaymentFailed.Dispatch("");
        }
#endregion
    }
}
