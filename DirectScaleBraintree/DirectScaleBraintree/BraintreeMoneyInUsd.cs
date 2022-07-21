using DirectScaleBraintree.Interfaces;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;

namespace DirectScaleBraintree
{
    public class BraintreeMoneyInUsd : BraintreeMoneyIn
    {
        public BraintreeMoneyInUsd(
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            IDataService dataService
            ) : base(currencyService, associateService, loggingService, settingsService, dataService,
                 new MerchantInfo
                 {
                     Currency = "USD",
                     DisplayName = "Credit Card/Other Methods",
                     Id = BraintreeSettings.BRAINTREE_USD_DISCO_MERCHANTID,
                     MerchantName = "Braintree (USD)"
                 }
             )
        {
        }
    }
}
