using Newtonsoft.Json;

namespace DirectScaleBraintree.Models
{
    public class TokenRequest
    {
        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }
        
        [JsonProperty("DiscoMerchantId")]
        public int DiscoMerchantId { get; set; }
    }
}
