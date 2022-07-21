namespace DirectScaleBraintree.Models
{
    public class ChargeData
    {
        public string PaymentMethodToken { get; set; }
        public decimal Amount { get; set; }
        public string OrderNumber { get; set; }
        public string ChannelPartnerId { get; set; }
    }
}
