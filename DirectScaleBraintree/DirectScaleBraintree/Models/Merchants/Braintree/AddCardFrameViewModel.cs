namespace DirectScaleBraintree.Models.Merchants.Braintree
{
    public class AddCardFrameViewModel
    {
        public string? BraintreeTokenizationKey { get; set; }
        public string? BaseUrl { get; set; }
        public string? BraintreeMerchantAccountId { get; set; }
        public int DiscoMerchantId { get; set; }
        public bool EnableDropInPayPal { get; set; }
        public bool EnableDropInPayPalCredit { get; set; }
        public bool EnableDropInVenmo { get; set; }
        public bool EnableDropInApplePay { get; set; }
        public bool EnableDropInGooglePay { get; set; }
        public string? GooglePayMerchantId { get; set; }
        public bool IsLive { get; set; }
        public string? PayorId { get; set; }
    }
}
