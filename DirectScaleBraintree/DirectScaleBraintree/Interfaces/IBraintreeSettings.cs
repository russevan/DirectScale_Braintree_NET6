namespace DirectScaleBraintree.Interfaces
{
    public interface IBraintreeSettings
    {
        bool EnableDBSettings { get; set; }
        int DiscoMerchantId { get; set; }
        string BraintreeEnvironment { get; set; }
        string BraintreeMerchantId { get; set; }
        string BraintreeMerchantAccountId { get; set; }
        string ChannelPartnerId { get; }
        string PublicKey { get; set; }
        string PrivateKey { get; set; }
        string TokenizationKey { get; set; }
        string PayPalClientId { get; set; }
        string PayPalSecret { get; set; }
        string IFrameDimensions { get; set; }
        string DropInUiMethods { get; set; }
        bool UseDirectScaleHardCodedCreds { get; set; }
        int IFrameWidth { get; }
        int IFrameHeight { get; }
        bool EnableDropInPayPal { get; }
        bool EnableDropInPayPalCredit { get; }
        bool EnableDropInVenmo { get; }
        bool EnableDropInApplePay { get; }
        bool EnableDropInGooglePay { get; }
        string GooglePayMerchantId { get; set; }
        decimal MinimumCurrencyAmount { get; set; }
        bool UseDatabaseSettings { get; set; }
        bool IsLive { get; }
        bool HasLoadedDBValues { get; set; }
        void ClearSettings();
    }
}
