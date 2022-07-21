using DirectScaleBraintree.Interfaces;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;

namespace DirectScaleBraintree
{
    public class BraintreeMoneyInAud : BraintreeMoneyIn
    {
        public BraintreeMoneyInAud(
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            IDataService dataService
            ) : base(currencyService, associateService, loggingService, settingsService, dataService,
                 new MerchantInfo
                 {
                     Currency = "AUD",
                     DisplayName = "Credit Card/Other Methods",
                     Id = BraintreeSettings.BRAINTREE_AUD_DISCO_MERCHANTID,
                     MerchantName = "Braintree (AUD)"
                 }
             )
        {
        }
    }
}
