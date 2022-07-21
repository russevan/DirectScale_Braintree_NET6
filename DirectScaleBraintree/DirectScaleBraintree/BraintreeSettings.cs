namespace DirectScaleBraintree 
{
    /// <summary>
    /// There are three possible setting setups here.
    /// 1. By default, Braintree will use the DirectScale hard-coded settings. This is a DS sandbox that crosses all clients, so expect data to already be there when you're using common IDs (like 2)
    /// 2. When the instance is made with settings, it will use whatever's there. We don't want mix-and-matching, though. So, what's hard-coded in the Client Extension should be Live creds.
    /// 3. If the user wants to specify and control credentials without deployment, that can be done in the Client DB schema (and ultimately on a Settings page, we'll see)
    /// </summary>
    public class BraintreeSettings //: IBraintreeSettings
    {
        public const int BRAINTREE_USD_DISCO_MERCHANTID = 9100;
        public const int BRAINTREE_CAD_DISCO_MERCHANTID = 9101;
        public const int BRAINTREE_AUD_DISCO_MERCHANTID = 9102;
        public const int BRAINTREE_GBP_DISCO_MERCHANTID = 9103;
        public const int BRAINTREE_NZD_DISCO_MERCHANTID = 9104;
        public const int BRAINTREE_EUR_DISCO_MERCHANTID = 9105;

        public const string BRAINTREE_DEFAULT_IFRAMEDIMENSIONS = "550x450";
        public const string BRAINTREE_CHANNEL_PARTNER_ID = "DirectScale_SP";

        private string _braintreeEnvironment;
        private string _braintreeMerchantId;
        private string _braintreePublicKey;
        private string _braintreePrivateKey;
        private string _braintreeTokenizationKey;
        private string _braintreePayPalClientId;
        private string _braintreePayPalSecret;
        private string _braintreeGooglePayMerchantId;
        private string _braintreeMerchantAccountId;
        private string _braintreeDropInUiMethods;
        private string _braintreeIFrameDimensions;

        private DateTime InstantiationTime { get; set; }

        public BraintreeSettings(int discoMerchId, bool useDSHardcodedCreds = true, bool enableDBSettings = true)
        {
            InstantiationTime = DateTime.Now;
            DiscoMerchantId = discoMerchId;
            UseDirectScaleHardCodedCreds = useDSHardcodedCreds;
            EnableDBSettings = enableDBSettings;
        }

        /// <summary>
        /// This is the partner ID, hard-coded to DirectScale's value.
        /// </summary>
        public string ChannelPartnerId => BRAINTREE_CHANNEL_PARTNER_ID;
        public int DiscoMerchantId { get; set; } 
        public bool EnableDBSettings { get; set; } // The program will read the DB for settings
        public bool HasLoadedDBValues { get; set; } = false;
        public double MinimumCurrencyAmount { get; set; } = 0.00;
        public bool IsLive => "production".Equals(BraintreeEnvironment, StringComparison.CurrentCultureIgnoreCase);
        public bool UseDatabaseSettings { get; set; } = false; // The program will use the settings it finds in the DB
        public bool UseDirectScaleHardCodedCreds { get; set; } = false;

        public double GetSettingsAgeInSeconds()
        {
            return (DateTime.Now - InstantiationTime).TotalSeconds;
        }

        public void ClearSettings()
        {
            //UseDatabaseSettings = false;
            UseDirectScaleHardCodedCreds = false;
            _braintreeEnvironment = string.Empty;
            _braintreeMerchantId = string.Empty;
            _braintreePublicKey = string.Empty;
            _braintreePrivateKey = string.Empty;
            _braintreeTokenizationKey = string.Empty;
            _braintreePayPalClientId = string.Empty;
            _braintreePayPalSecret = string.Empty;
            _braintreeGooglePayMerchantId = string.Empty;
            _braintreeMerchantAccountId = string.Empty;
            _braintreeDropInUiMethods = string.Empty;
            _braintreeIFrameDimensions = string.Empty;
        }

        public string BraintreeEnvironment
        {
            get
            {
                var braintreeEnvironment = UseDirectScaleHardCodedCreds ? "sandbox" : _braintreeEnvironment;
                if (string.IsNullOrEmpty(braintreeEnvironment) || !"production sandbox".Contains(braintreeEnvironment))
                {
                    throw new Exception($"The Braintree Environment setting has not been set, or has been set incorrectly. Correct values are 'production' or 'sandbox'. Actual value: {braintreeEnvironment}");
                }
                return braintreeEnvironment;
            }
            set => _braintreeEnvironment = value;
        }


        // This is the identifier of individual, currency-specific merchant accounts. There may be 
        // many of these per client relationship with Braintree.
        public string BraintreeMerchantAccountId
        {
            get
            {
                switch (DiscoMerchantId)
                {
                    case BRAINTREE_USD_DISCO_MERCHANTID:
                        return UseDirectScaleHardCodedCreds ? "directscale" : _braintreeMerchantAccountId;
                    case BRAINTREE_CAD_DISCO_MERCHANTID:
                        return UseDirectScaleHardCodedCreds ? "directscale_CAD" : _braintreeMerchantAccountId;
                    case BRAINTREE_AUD_DISCO_MERCHANTID:
                        return UseDirectScaleHardCodedCreds ? "directscale_AUD" : _braintreeMerchantAccountId;
                    case BRAINTREE_GBP_DISCO_MERCHANTID:
                        return UseDirectScaleHardCodedCreds ? "directscale_GBP" : _braintreeMerchantAccountId;
                    case BRAINTREE_NZD_DISCO_MERCHANTID:
                        return UseDirectScaleHardCodedCreds ? "directscale_NZD" : _braintreeMerchantAccountId;
                    case BRAINTREE_EUR_DISCO_MERCHANTID:
                        return UseDirectScaleHardCodedCreds ? "directscale_EUR" : _braintreeMerchantAccountId;
                }
                return "";
            }
            set => _braintreeMerchantAccountId = value;
        }

        // This is the unique identifier for the overall Merchant relationship with Braintree.
        // There is one of these per client. It is a single relationship between Braintree and the DS client.
        public string BraintreeMerchantId
        {
            get => UseDirectScaleHardCodedCreds ? "s8x92wg4v8qvq45j" : _braintreeMerchantId;
            set => _braintreeMerchantId = value;
        }
        
        public string PayPalClientId
        {
            get => UseDirectScaleHardCodedCreds ? "AVeR5GW2uCj2nbF-JJXUTGh3tnHMeatsti_ei_WCletXP75iQLPoQnEJhOO3JT9c82B-n1PcqzO5Hqtp" : _braintreePayPalClientId ?? throw new Exception("The Braintree ClientId setting has not been set.");
            set => _braintreePayPalClientId = value;
        }
        public string PayPalSecret
        {
            get => UseDirectScaleHardCodedCreds ? "EP5Mjrtg8Kl82HHiQFWidWMJIgIwrWmESF0a8-c9KjfjWTYOLb0VwFCLZJzc68QF9Chvg8GuJCU5JkFY" : _braintreePayPalSecret ?? throw new Exception("The Braintree 'Secret' setting has not been set.");
            set => _braintreePayPalSecret = value;
        }
        public string PrivateKey
        {
            get => UseDirectScaleHardCodedCreds ? "b6e3e6c098963479ad754e56fb7c52da" : _braintreePrivateKey ?? throw new Exception("The Braintree Private Key setting has not been set.");
            set => _braintreePrivateKey = value;
        }
        public string PublicKey
        {
            get => UseDirectScaleHardCodedCreds ? "8rfxmf6fh7bhkrqt" : _braintreePublicKey ?? throw new Exception("The Braintree Public Key setting has not been set.");
            set => _braintreePublicKey = value;
        }

        public string TokenizationKey
        {
            get => UseDirectScaleHardCodedCreds ? "sandbox_ktv3dxf7_s8x92wg4v8qvq45j" : _braintreeTokenizationKey ?? throw new Exception("The Braintree Tokenization Key setting has not been set.");
            set => _braintreeTokenizationKey = value;
        }
        public int IFrameWidth => GetIFrameDimensions("Width");
        public int IFrameHeight => GetIFrameDimensions("Height");
        public bool EnableDropInPayPal => IsDropInEnabledForProviderString("PAYPAL");
        public bool EnableDropInPayPalCredit => IsDropInEnabledForProviderString("PPCREDIT");
        public bool EnableDropInVenmo => IsDropInEnabledForProviderString("VENMO");
        public bool EnableDropInApplePay => IsDropInEnabledForProviderString("APPLEPAY");
        public bool EnableDropInGooglePay => IsDropInEnabledForProviderString("GOOGLEPAY");
        public string GooglePayMerchantId
        {
            get => UseDirectScaleHardCodedCreds ? "merchant-id-from-google" : _braintreeGooglePayMerchantId;
            set => _braintreeGooglePayMerchantId = value;
        }

        public string DropInUiMethods
        {
            get => UseDirectScaleHardCodedCreds ? "PAYPAL,PPCREDIT,VENMO,APPLEPAY,GOOGLEPAY" : _braintreeDropInUiMethods;
            set => _braintreeDropInUiMethods = value.ToUpperInvariant();
        }

        public string IFrameDimensions {
            get { return _braintreeIFrameDimensions ?? BRAINTREE_DEFAULT_IFRAMEDIMENSIONS; }
            set { _braintreeIFrameDimensions = value; }
        }

        // If the DropIn UI setting is blank, then Braintree will only render Credit Card fields in the drop-in UI, but it's not an error.
        protected bool IsDropInEnabledForProviderString(string payProviderToFetch)
        {
            return DropInUiMethods.Contains(payProviderToFetch);
        }

        private int GetIFrameDimensions(string dimensionToGet)
        {
            var dimensionParts = IFrameDimensions.Split('x');
            if (dimensionParts.Length != 2)
            {
                throw new Exception($"Braintree IFrame Dimension Setting '{IFrameDimensions}' must be formatted WxH");
            }

            try
            {
                switch (dimensionToGet)
                {
                    case "Width":
                        return Convert.ToInt32(dimensionParts[0]);
                    case "Height":
                        return Convert.ToInt32(dimensionParts[1]);
                    default:
                        return 0;
                }
            }
            catch (Exception)
            {
                throw new Exception("Braintree IFrame Dimension Setting 'Braintree Iframe Dimensions' must be formatted WxH");
            }
        }
    }
}
