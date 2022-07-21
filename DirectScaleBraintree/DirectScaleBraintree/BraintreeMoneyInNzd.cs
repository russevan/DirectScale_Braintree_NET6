using DirectScaleBraintree.Interfaces;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;

namespace DirectScaleBraintree
{
    public class BraintreeMoneyInNzd : BraintreeMoneyIn
    {
        public BraintreeMoneyInNzd(
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            IDataService dataService
            ) : base(currencyService, associateService, loggingService, settingsService, dataService,
                 new MerchantInfo
                 {
                     Currency = "NZD",
                     DisplayName = "Credit Card/Other Methods",
                     Id = BraintreeSettings.BRAINTREE_NZD_DISCO_MERCHANTID,
                     MerchantName = "Braintree (NZD)"
                 }
             )
        {
        }
    }
}
