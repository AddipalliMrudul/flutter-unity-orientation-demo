using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XcelerateGames.Purchasing
{
    public class ReceiptData
    {
        public Payload Payload { get; set; }
        public string Store { get; set; }
        public string TransactionID { get; set; }

        // [JsonExtensionData]
        // private IDictionary<string, JToken> _additionalData;

        // [OnDeserialized]
        // private void OnDeserialized(StreamingContext context)
        // {
        //     XDebug.LogError("HERE " + _additionalData["json"]);
        // }
    }

    public class Json
    {
        public string orderId { get; set; }
        public string packageName { get; set; }
        public string productId { get; set; }
        public long purchaseTime { get; set; }
        public int purchaseState { get; set; }
        public string purchaseToken { get; set; }
        public int quantity { get; set; }
        public bool acknowledged { get; set; }
    }

    public class Payload
    {
        public Json json { get; set; }
        public string signature { get; set; }
        public List<SkuDetail> skuDetails { get; set; }
    }

    public class SkuDetail
    {
        public string productId { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string name { get; set; }
        public string iconUrl { get; set; }
        public string description { get; set; }
        public string price { get; set; }
        public int price_amount_micros { get; set; }
        public string price_currency_code { get; set; }
        public string skuDetailsToken { get; set; }
    }
}
