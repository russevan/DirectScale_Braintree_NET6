using DirectScaleBraintree;
using DirectScaleBraintree.Interfaces;
using DirectScaleBraintree.Models;
using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using System;

namespace BraintreeDirectScale.Api
{
    public class SavePayment : IApiEndpoint
    {
        private readonly ICurrencyService _currencyService;
        private readonly IAssociateService _associateService;
        private readonly IRequestParsingService _requestParsing;
        private readonly ILoggingService _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;


        public SavePayment(IRequestParsingService requestParsing, ICurrencyService currencyService, IAssociateService associateService, ILoggingService loggingService, ISettingsService settingsService, IDataService dataService)
        {
            _requestParsing = requestParsing;
            _currencyService = currencyService;
            _associateService = associateService;
            _logger = loggingService;
            _settingsService = settingsService;
            _dataService = dataService;
        }

        public ApiDefinition GetDefinition()
        {
            return new ApiDefinition
            {
                Route = "braintree/savePayment",
                Authentication = AuthenticationType.None
            };
        }

        public IApiResponse Post(ApiRequest request)
        {
            _logger.LogInformation($"Called SavePayment for customer with this payload: {request.Body}");
            var paymentIn = _requestParsing.ParseBody<SavePaymentModel>(request);
            if (paymentIn == null) { throw new Exception("The inbound payment request could not be parsed."); }
            var moneyIn = Initializer.GetMoneyInInstanceForMerchantId(paymentIn.DiscoMerchantId, _currencyService, _associateService, _logger, _settingsService, _dataService);
            try
            {
                return new Ok(moneyIn.SavePayment(paymentIn));
            }
            catch (Exception e)
            {
                return new Ok(new { ErrorMessage = e.Message, IsError = true });
            }
        }
    }
}
