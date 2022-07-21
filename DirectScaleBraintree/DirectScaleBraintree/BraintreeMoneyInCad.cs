using DirectScaleBraintree;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;
using DirectScaleBraintree.Interfaces;

namespace DirectScaleBraintree
{
    public class BraintreeMoneyInCad : BraintreeMoneyIn
    {
        public BraintreeMoneyInCad(
            //bool useDSHardcodedCreds,
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            IDataService dataService
            ) : base(currencyService, associateService, loggingService, settingsService, dataService,
                 new MerchantInfo
                 {
                     Currency = "CAD",
                     DisplayName = "Credit Card/Other Methods",
                     Id = BraintreeSettings.BRAINTREE_CAD_DISCO_MERCHANTID,
                     MerchantName = "Braintree (CAD)"
                 }
             )
        {
        }
    }
}
