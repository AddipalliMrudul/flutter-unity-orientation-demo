#if USE_IAP
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
#if UNITY_2022_3
using UnityEngine.Purchasing.Extension;
#endif
using XcelerateGames.IOC;

namespace XcelerateGames.Purchasing
{
    /// <summary>
    /// https://docs.unity3d.com/Packages/com.unity.purchasing@4.7/manual/index.html
    /// https://developers.google.com/android-publisher/api-ref/rest/v3/monetization.subscriptions
    /// </summary>

#if UNITY_2022_3
    public class PurchaseManager : BaseBehaviour, IDetailedStoreListener
#else
    public class PurchaseManager : BaseBehaviour, IStoreListener
#endif
    {
        [SerializeField] bool _SkipIAPVerification = false;

        //[Inject] SigRemoteRequest mSigRemoteRequest = null;
        [InjectSignal] SigPurchaseItem mSigPurchaseItem = null;
        [InjectSignal] SigPurchaseStarted mSigPurchaseStarted = null;
        [InjectSignal] SigPurchaseCompleted mSigPurchaseCompleted = null;
        [InjectSignal] SigPurchaseFailed mSigPurchaseFailed = null;
        [InjectSignal] SigPurchaseCancelled mSigPurchaseCancelled = null;
        [InjectSignal] SigRestorePurchases mSigRestorePurchases = null;
        [InjectSignal] SigPurchaseInitFailed mSigPurchaseInitFailed = null;
        [InjectSignal] SigIAPSubscriptionInfo mSigIAPSubscriptionInfo = null;
        [InjectSignal] SigPurchaseManagerReady mSigPurchaseManagerReady = null;
        [InjectSignal] SigInitPurchaseManager mSigInitPurchaseManager = null;
        [InjectSignal] SigCancelSubscription mSigCancelSubscription = null;
        [InjectSignal] SigDoPurchaseVerification mSigDoPurchaseVerification = null;
        [InjectSignal] SigSetPurchaseVerification mSigSetPurchaseVerification = null;

        [InjectModel] private IAPStoreModel mIAPStoreModel = null;

        private IStoreController mStoreController;          // The Unity Purchasing system.
        private IExtensionProvider mStoreExtensionProvider; // The store-specific Purchasing subsystems.
        //Reference to the pack we are trying to purchase
        private string mCurrentItem = null;
        //private bool mShowRewards = true;
        private Func<string, bool> mReceiptValidator = null;

#if UNITY_IOS
        [InjectSignal] SigPurchaseRestored mSigPurchaseRestored = null;
        private IAppleExtensions mAppleExtensions;
#endif

        protected void Start()
        {
            XDebug.Log($"PurchaseManager created: _SkipIAPVerification:{_SkipIAPVerification}", XDebug.Mask.Purchasing);
            mSigInitPurchaseManager.AddListener(InitializePurchasing);
            mSigPurchaseItem.AddListener(Purchase);
            mSigRestorePurchases.AddListener(OnRequestRestorePurchases);
            mSigCancelSubscription.AddListener(OnRequestCancelSubscription);
            mSigSetPurchaseVerification.AddListener(SetPurchaseVerifier);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            mSigPurchaseItem.RemoveListener(Purchase);
            mSigInitPurchaseManager.RemoveListener(InitializePurchasing);
            mSigRestorePurchases.RemoveListener(OnRequestRestorePurchases);
            mSigCancelSubscription.RemoveListener(OnRequestCancelSubscription);
            mSigSetPurchaseVerification.RemoveListener(SetPurchaseVerifier);
        }

        private void SetPurchaseVerifier(Func<string, bool> receiptValidator)
        {
            mReceiptValidator = receiptValidator;
        }
        private void Verify(Product product)
        {
            // Function made local, to not have to cache Product details.
            //if success is true, verification sucessfull else failed
            Action<bool> OnVerificationComplete = (success) =>
            {
                XDebug.Log($"Purchase Verification complete: {success}", XDebug.Mask.Purchasing);
                if (success)
                {
                    // Update store that the product is consumed.
                    mStoreController.ConfirmPendingPurchase(product);
                    mSigPurchaseCompleted.Dispatch(mCurrentItem);
                }
                else
                    mSigPurchaseFailed.Dispatch(mCurrentItem);
            };

            if (_SkipIAPVerification || Application.isEditor)
            {
                XDebug.Log("Skipping IAP verification", XDebug.Mask.Purchasing);
                OnVerificationComplete(true);
            }
            else
            {
                try
                {
                    if (XDebug.CanLog(XDebug.Mask.Purchasing))
                    {
                        XDebug.Log("Performing IAP verification", XDebug.Mask.Purchasing);
                        XDebug.Log(product.receipt, XDebug.Mask.Purchasing);
                    }
                    mSigDoPurchaseVerification.Dispatch(product, OnVerificationComplete);
                }
                catch (Exception e)
                {
                    XDebug.LogException("Exception while loading receipt : " + product.receipt + "\n" + e.Message);
                    OnVerificationFailed(null);
                }
            }
        }

        private void OnVerificationFailed(object obj)
        {
            XDebug.LogException("Purchase verification failed.");
            mSigPurchaseFailed.Dispatch(mCurrentItem);
            mCurrentItem = null;
        }

        private void OnRequestRestorePurchases()
        {
#if UNITY_IOS
            mAppleExtensions.RestoreTransactions(result =>
            {
                if (result)
                {
                    XDebug.Log("Purchases restore successfully", XDebug.Mask.Purchasing);
                }
                else
                {
                    XDebug.LogError("Failed to restore purchase", XDebug.Mask.Purchasing);
                }
                mSigPurchaseRestored.Dispatch(result);
            });
#endif
        }

        void InitializePurchasing(List<StoreItem> storeProductIds)
        {
            if (IsInited())
                return;
            if (XDebug.CanLog(XDebug.Mask.Purchasing))
                XDebug.Log($"Initialize Purchasing: \n{storeProductIds.Printable('\n')}", XDebug.Mask.Purchasing);

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (StoreItem product in storeProductIds)
            {
                builder.AddProduct(product.productId, TranslateType(product.productType));
            }

            if (builder.products.Count > 0)
            {
                XDebug.Log("Initializing " + builder.products.Count + " IAP items.", XDebug.Mask.Purchasing);
                UnityPurchasing.Initialize(this, builder);
            }
            else
                XDebug.LogError("No Items to initialize IAP store", XDebug.Mask.Purchasing);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            XDebug.Log("Initilaized IAP items", XDebug.Mask.Purchasing);
            mStoreController = controller;
            mStoreExtensionProvider = extensions;
            UpdateIapData();
#if UNITY_IOS
            mAppleExtensions = extensions.GetExtension<IAppleExtensions>();
            mAppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
#endif
            CheckSubscriptionStatus();
            mSigPurchaseManagerReady.Dispatch(true);
        }

        private void CheckSubscriptionStatus()
        {
            if (mStoreController != null)
            {
                Product[] products = mStoreController.products.all;
                Dictionary<string, string> introductory_info_dict = null;
#if UNITY_IOS
                introductory_info_dict = mAppleExtensions.GetIntroductoryPriceDictionary();
#endif //UNITY_IOS

                //mIAPStoreModel._SubscriptionData = new List<IAPSubscriptionInfo>();
                foreach (Product product in products)
                {
                    if (product.definition.type == UnityEngine.Purchasing.ProductType.Subscription)
                    {
                        if (product.receipt != null)
                        {
                            string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(product.definition.storeSpecificId)) ? null : introductory_info_dict[product.definition.storeSpecificId];
                            SubscriptionManager p = new SubscriptionManager(product, intro_json);
                            SubscriptionInfo info = p.getSubscriptionInfo();

                            IAPSubscriptionInfo subscriptionInfo = mIAPStoreModel._SubscriptionData.Find(x => x.productId == product.definition.id);
                            if (subscriptionInfo != null)
                            {
                                //subscriptionInfo.productId = info.getProductId();
                                subscriptionInfo.purchaseDate = info.getPurchaseDate();
                                subscriptionInfo.expiryDate = info.getExpireDate();
                                subscriptionInfo.isSubscribed = info.isSubscribed() == Result.True;
                                subscriptionInfo.isExpired = info.isExpired() == Result.True;
                                subscriptionInfo.isCancelled = info.isCancelled() == Result.True;
                                subscriptionInfo.remainingTime = info.getRemainingTime();

                                mSigIAPSubscriptionInfo.Dispatch(subscriptionInfo);
                                //mIAPStoreModel._SubscriptionData.Add(subscriptionInfo);
                                if (XDebug.CanLog(XDebug.Mask.Purchasing))
                                {
                                    string message = subscriptionInfo.ToJson();
                                    message += ", product is in free trial peroid? " + info.isFreeTrial();
                                    message += ", product is auto renewing? " + info.isAutoRenewing();
                                    message += ", is this product in introductory price period? " + info.isIntroductoryPricePeriod();
                                    message += ", the product introductory localized price is: " + info.getIntroductoryPrice();
                                    message += ", the product introductory price period is: " + info.getIntroductoryPricePeriod();
                                    message += ", the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles();
                                    XDebug.Log($"Subscription status of {product.definition.id}: {message}", XDebug.Mask.Purchasing);
                                }
                            }
                        }
                        else
                        {
                            XDebug.Log($"The product {product.definition.id} should have a valid receipt", XDebug.Mask.Purchasing);
                        }
                    }
                }
            }
        }

        private void UpdateIapData()
        {
            mIAPStoreModel._StoreData = new List<IAPStoreData>();
            mIAPStoreModel._SubscriptionData = new List<IAPSubscriptionInfo>();
            if (mStoreController != null)
            {
                Product[] products = mStoreController.products.all;
                foreach (Product product in products)
                {
                    if (!product.availableToPurchase)
                    {
                        XDebug.LogWarning($"Product : {product.definition.id} is not available for purchase, skipping it", XDebug.Mask.Purchasing);
                        continue;
                    }
                    if (product.definition.type == UnityEngine.Purchasing.ProductType.Consumable || product.definition.type == UnityEngine.Purchasing.ProductType.NonConsumable)
                    {
                        IAPStoreData storeData = new IAPStoreData()
                        {
                            productId = product.definition.id,
                            localizedPriceString = product.metadata.localizedPriceString,
                            localizedTitle = product.metadata.localizedPriceString,
                            isoCurrencyCode = product.metadata.isoCurrencyCode,
                            localizedDescription = product.metadata.localizedDescription,
                            localizedPrice = product.metadata.localizedPrice
                        };
                        if (XDebug.CanLog(XDebug.Mask.Purchasing))
                            XDebug.Log(storeData.ToString(), XDebug.Mask.Purchasing);
                        mIAPStoreModel._StoreData.Add(storeData);
                    }
                    else if (product.definition.type == UnityEngine.Purchasing.ProductType.Subscription)
                    {
                        IAPSubscriptionInfo subscriptionInfo = new IAPSubscriptionInfo()
                        {
                            productId = product.definition.id,
                            localizedPriceString = product.metadata.localizedPriceString,
                            localizedTitle = product.metadata.localizedPriceString,
                            isoCurrencyCode = product.metadata.isoCurrencyCode,
                            localizedDescription = product.metadata.localizedDescription,
                            localizedPrice = product.metadata.localizedPrice
                        };
                        mIAPStoreModel._SubscriptionData.Add(subscriptionInfo);
                    }
                }
            }
        }

        private UnityEngine.Purchasing.ProductType TranslateType(ProductType purchasableType)
        {
            if (purchasableType == ProductType.Consumable)
                return UnityEngine.Purchasing.ProductType.Consumable;
            if (purchasableType == ProductType.NonConsumable)
                return UnityEngine.Purchasing.ProductType.NonConsumable;
            if (purchasableType == ProductType.Subscription)
                return UnityEngine.Purchasing.ProductType.Subscription;
            XDebug.LogException($"Failed to translate purchasable type: {purchasableType}");
            return UnityEngine.Purchasing.ProductType.Consumable;
        }

        public void ProcessPendingTransactions()
        {
#if UNITY_IOS
            mAppleExtensions.RestoreTransactions((result) =>
            {
                if (!result)
                {
                    mSigRemoteRequest.Dispatch(BAWebRequestKeys.IAP_CLEAR_PENDING_TRANSACTIONS);
                }
            });
#endif
        }

        private void OnDeferred(Product item)
        {
            Debug.LogError("Purchase deferred: " + item.definition.id);
        }

        private bool IsInited()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return mStoreController != null && mStoreExtensionProvider != null;
        }

        private bool IsItemAvailable(string productID)
        {
            if (IsInited())
            {
#if UNITY_EDITOR
                return true;
#else
				Product product = mStoreController.products.WithID(productID);
				if (product != null && product.availableToPurchase)
				{
					return true;
				}
#endif
            }
            return false;
        }

        private void Purchase(string productId)
        {
            if (IsInited())
            {
                //    mShowRewards = showPurchasedItems;
                mCurrentItem = productId;
                Product product = mStoreController.products.WithID(productId);
                XDebug.Log(string.Format("Intiating purchase for : '{0}'", productId), XDebug.Mask.Purchasing);
                if (product != null && product.availableToPurchase)
                {
                    mSigPurchaseStarted.Dispatch(mCurrentItem);
                    XDebug.Log($"Purchasing product asychronously: '{product.definition.id}'", XDebug.Mask.Purchasing);
                    if (Application.isEditor)
                    {
                        XDebug.Log("Purchase complete {Faked on IDE}", XDebug.Mask.Purchasing);
                        mSigPurchaseCompleted.Dispatch(mCurrentItem);
                    }
                    else
                        mStoreController.InitiatePurchase(product);
                }
                else
                {
                    XDebug.LogError("BuyProductID: FAILED for " + (product != null ? product.definition.id : productId) + ". Not purchasing product, either is not found or is not available for purchase");
                    mSigPurchaseFailed.Dispatch(mCurrentItem);
                }
            }
            else
            {
                XDebug.LogException("BuyProduct " + productId + " failed. Not initialized.");
                mSigPurchaseFailed.Dispatch(mCurrentItem);
            }
        }

        private void OnInitiateFailed(object obj)
        {
            XDebug.Log("Purchase Initiate failed", XDebug.Mask.Purchasing);
        }

        private void OnInitiateComplete(string obj)
        {
            XDebug.Log("Purchase Initiate complete", XDebug.Mask.Purchasing);
        }

        private void OnRequestCancelSubscription(string productId)
        {
#if UNITY_ANDROID
            string url = $"https://play.google.com/store/account/subscriptions?sku={productId}&package={Application.identifier}";
            XDebug.Log($"Requesting cancel subscription for product id: {productId}, Request url: {url}", XDebug.Mask.Purchasing);
            Application.OpenURL(url);
#elif UNITY_IOS
            Debug.LogError("IMPLEMENT THIS FOR iOS");
#endif
        }

        public Product GetItemsMetadata(string productID)
        {
            if (IsInited())
            {
                Product product = mStoreController.products.WithID(productID);
                return product;
            }
            return null;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            XDebug.LogError($"OnInitializeFailed InitializationFailureReason: {error}", XDebug.Mask.Purchasing);
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    XDebug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                    mIAPStoreModel.InitializationFailure = IAPInitializationFailureReason.AppNotKnown;
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    Debug.Log("Billing disabled!");
                    mIAPStoreModel.InitializationFailure = IAPInitializationFailureReason.PurchasingUnavailable;
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    Debug.Log("No products available for purchase!");
                    mIAPStoreModel.InitializationFailure = IAPInitializationFailureReason.NoProductsAvailable;
                    break;
            }
            mSigPurchaseInitFailed.Dispatch(mIAPStoreModel.InitializationFailure);
            mSigPurchaseManagerReady.Dispatch(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (XDebug.CanLog(XDebug.Mask.Purchasing))
            {
                XDebug.Log($"Purchase successfull : {args.purchasedProduct.ToJson()}", XDebug.Mask.Purchasing);
                XDebug.Log("{RECEIPT} \n" + args.purchasedProduct.receipt, XDebug.Mask.Purchasing);
            }
            Product product = GetItemsMetadata(args.purchasedProduct.definition.id);
            if (product != null)
            {
                bool? isReceiptValid = mReceiptValidator?.Invoke(product.receipt);
                if (isReceiptValid.HasValue && isReceiptValid.Value)
                {
                    Debug.Log("Purchase validated locally, checking with backend now");
                    Verify(product);
                }
                else
                {
                    Debug.Log("Purchase validation failed locally");
                    mSigPurchaseFailed.Dispatch(mCurrentItem);
                }
            }
            else
                XDebug.LogException("Product was null for item being purchased : " + args.purchasedProduct.definition.id);
            // Changed from Complete to Pending.
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            mSigPurchaseCancelled.Dispatch(mCurrentItem);
            mCurrentItem = null;
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing
            // this reason with the user to guide their troubleshooting actions.
            XDebug.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}", XDebug.Mask.Purchasing);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            XDebug.LogError($"OnInitializeFailed:->Error: {error}, message: {message}");
        }

#if UNITY_2022_3
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            XDebug.LogError($"OnPurchaseFailed:->Product: {product.definition.id}, PurchaseFailureDescription: {failureDescription}");
        }
#endif
    }
}
#endif //USE_IAP