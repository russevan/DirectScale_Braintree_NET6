using System.Collections.Generic;

namespace DirectScaleBraintree.Models
{
    public class ChargeResponse
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; }
        public string ProcessorResponseCode { get; set; }
        public string ProcessorResponseMessage { get; set; }
        public string ProcessorResponseAdditionalData { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
