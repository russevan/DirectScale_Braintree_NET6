using DirectScaleBraintree;
using DirectScaleBraintree.Models;
using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using System;

namespace BraintreeDirectScale.Api
{
    public class GetClientToken : IApiEndpoint
    {
        private readonly ICurrencyService _currencyService;
        private readonly IAssociateService _associateService;
        private readonly IRequestParsingService _requestParsing;
        private readonly ILoggingService _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;

        public GetClientToken(IRequestParsingService requestParsing, ICurrencyService currencyService, IAssociateService associateService, ILoggingService loggingService, ISettingsService settingsService, IDataService dataService)
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
                Route = "braintree/getClientToken",
                Authentication = AuthenticationType.None
            };
        }

        public IApiResponse Post(ApiRequest request)
        {
            var paymentIn = _requestParsing.ParseBody<TokenRequest>(request);
            if (paymentIn == null)
            {
                _logger.LogInformation($"Could not parse token request. Got body: {request.Body}");
                throw new Exception("The inbound token request could not be parsed. It must include CustomerId and DiscoMerchantId.");
            }
            var moneyIn = Initializer.GetMoneyInInstanceForMerchantId(paymentIn.DiscoMerchantId, _currencyService, _associateService, _logger, _settingsService, _dataService);
            return new Ok(moneyIn.GetClientToken(paymentIn.CustomerId));
        }
    }
}
