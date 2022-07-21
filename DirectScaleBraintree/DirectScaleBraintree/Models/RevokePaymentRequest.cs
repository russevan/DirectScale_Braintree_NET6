namespace DirectScaleBraintree.Models
{
    public class RevokePaymentRequest
    {
        public dynamic bt_signature { get; set; }
        public dynamic bt_payload { get; set; }
    }
}
