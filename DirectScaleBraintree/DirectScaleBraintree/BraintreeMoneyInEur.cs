using DirectScaleBraintree.Interfaces;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;

namespace DirectScaleBraintree
{
    public class BraintreeMoneyInEur : BraintreeMoneyIn
    {
        public BraintreeMoneyInEur(
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            IDataService dataService
            ) : base(currencyService, associateService, loggingService, settingsService, dataService,
                 new MerchantInfo
                 {
                     Currency = "EUR",
                     DisplayName = "Credit Card/Other Methods",
                     Id = BraintreeSettings.BRAINTREE_EUR_DISCO_MERCHANTID,
                     MerchantName = "Braintree (EUR)"
                 }
             )
        {
        }
    }
}
