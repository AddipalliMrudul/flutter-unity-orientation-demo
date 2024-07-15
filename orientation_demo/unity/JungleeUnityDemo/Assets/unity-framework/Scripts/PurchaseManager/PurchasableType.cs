namespace XcelerateGames.Purchasing
{
    public enum ProductType
    {
        None = -1,
        Consumable = 0,
        NonConsumable,
        Subscription
    }

    public enum IAPInitializationFailureReason
    {
        None,
        AppNotKnown,
        PurchasingUnavailable,
        NoProductsAvailable
    }
}
