using DirectScaleBraintree.Interfaces;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;

namespace DirectScaleBraintree
{
    public class BraintreeMoneyInGbp : BraintreeMoneyIn
    {
        public BraintreeMoneyInGbp(
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            IDataService dataService
            ) : base(currencyService, associateService, loggingService, settingsService, dataService,
                 new MerchantInfo
                 {
                     Currency = "GBP",
                     DisplayName = "Credit Card/Other Methods",
                     Id = BraintreeSettings.BRAINTREE_GBP_DISCO_MERCHANTID,
                     MerchantName = "Braintree (GBP)"
                 }
             )
        {
        }
    }
}
