#if USE_IAP
using System;
using System.Collections.Generic;
using XcelerateGames;
using XcelerateGames.IOC;

namespace XcelerateGames.Purchasing
{
    public class IAPStoreModel : XGModel
    {
        public List<IAPStoreData> _StoreData = null;
        public List<IAPSubscriptionInfo> _SubscriptionData = null;
        public IAPInitializationFailureReason InitializationFailure = IAPInitializationFailureReason.None;

        public bool IsReady => _StoreData != null;

        public IAPStoreData GetStoreData(string productid)
        {
            IAPStoreData storeData = _StoreData.Find(product => product.productId.Equals(productid));
            if (storeData == null)
                XDebug.LogError($"Could not find IAPStoreData for {productid}", XDebug.Mask.Prefetching);
            return storeData;
        }

        public IAPSubscriptionInfo GetSubscriptionInfo(string productid)
        {
            IAPSubscriptionInfo subscriptionData = _SubscriptionData.Find(product => product.productId.Equals(productid));
            if (subscriptionData == null)
                XDebug.LogException($"Could not find IAPSubscriptionInfo for {productid}");
            return subscriptionData;
        }
    }

    public class IAPStoreData
    {
        public string productId;
        public string localizedPriceString;
        public string localizedTitle;
        public string isoCurrencyCode;
        public string localizedDescription;
        public decimal localizedPrice;

        public override string ToString()
        {
            return this.ToJson();
        }
    }

    public class IAPSubscriptionInfo : IAPStoreData
    {
        public DateTime purchaseDate;
        public DateTime expiryDate;
        public TimeSpan remainingTime;
        public bool isSubscribed;
        public bool isExpired;
        public bool isCancelled;
    }

    public class StoreItem
    {
        public string productId = null;
        public ProductType productType = ProductType.None;

        public override string ToString()
        {
            return $"{productId} : {productType}";
        }
    }
}
#endif //USE_IAP
