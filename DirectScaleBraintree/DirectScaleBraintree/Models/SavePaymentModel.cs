using Newtonsoft.Json;

namespace DirectScaleBraintree.Models
{
    public class SavePaymentModel
    {
        public SavePaymentModel Clone()
        {
            return (SavePaymentModel)MemberwiseClone();
        }
        [JsonProperty("DiscoMerchantId")]
        public int DiscoMerchantId { get; set; }

        [JsonProperty("PayerId")]
        public string PayerId { get; set; }

        [JsonProperty("PaymentNonce")] // One-time-use token, will be converted to vault token
        public string PaymentNonce { get; set; }

        [JsonProperty("ExpireMonth")]
        public string ExpireMonth { get; set; }

        [JsonProperty("ExpireYear")]
        public string ExpireYear { get; set; }

        [JsonProperty("PaymentType")] // PayPalAccount, CreditCard, VenmoAccount, others?
        public string PaymentType { get; set; }

        [JsonProperty("CardType")]
        public string CardType { get; set; }

        [JsonProperty("CardHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty("LastFour")]
        public string LastFour { get; set; }

        [JsonProperty("DeviceData")]
        public string DeviceData { get; set; }
        
        [JsonProperty("PaymentToken")]
        public string PaymentToken { get; set; }


    }
}
