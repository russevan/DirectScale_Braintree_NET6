using DirectScale.Disco.Extension.Api;
using Microsoft.Extensions.DependencyInjection;
using DirectScaleBraintree.Interfaces;
using BraintreeDirectScale;
using DirectScale.Disco.Extension;
using BraintreeDirectScale.Api;
using System;
using DirectScale.Disco.Extension.Services;
using DirectScale.Braintree.Repositories;

namespace DirectScaleBraintree
{
    public static class Initializer
    {
        public static void UseBraintree(this IServiceCollection services, int discoMerchId)
        {
            UseBraintree(services);
        }

        public static void UseBraintree(this IServiceCollection services, int[] discoMerchIds)
        {
            UseBraintree(services);
        }

        public static void UseBraintree(this IServiceCollection services)
        {
            services.AddSingleton<IMoneyInMerchant, BraintreeMoneyInCad>();
            services.AddSingleton<IMoneyInMerchant, BraintreeMoneyInUsd>();
            services.AddSingleton<IMoneyInMerchant, BraintreeMoneyInAud>();
            services.AddSingleton<IMoneyInMerchant, BraintreeMoneyInGbp>();
            services.AddSingleton<IMoneyInMerchant, BraintreeMoneyInNzd>();
            services.AddSingleton<IMoneyInMerchant, BraintreeMoneyInEur>();

            services.AddSingleton<IApiEndpoint, GetClientToken>();
            services.AddSingleton<IApiEndpoint, GetClientToken>();
            services.AddSingleton<IApiEndpoint, SavePayment>();

            // * Types of services that can be added: *
            // Transient: Create a new one every time.
            // Singleton: Once in life of service. Cleared when IIS restarts.
            // Scoped: Once per HTTPContext request
        }

        public static BraintreeMoneyIn GetMoneyInInstanceForMerchantId(int mId, ICurrencyService _currencyService, IAssociateService _associateService, ILoggingService _logger, ISettingsService _settingsService, IDataService _dataService)
        {
            switch (mId)
            {
                case BraintreeSettings.BRAINTREE_AUD_DISCO_MERCHANTID:
                    return new BraintreeMoneyInAud(_currencyService, _associateService, _logger, _settingsService, _dataService) ?? throw new Exception("The payment processor is incorrectly configured.");
                case BraintreeSettings.BRAINTREE_CAD_DISCO_MERCHANTID:
                    return new BraintreeMoneyInCad(_currencyService, _associateService, _logger, _settingsService, _dataService) ?? throw new Exception("The payment processor is incorrectly configured.");
                case BraintreeSettings.BRAINTREE_EUR_DISCO_MERCHANTID:
                    return new BraintreeMoneyInEur(_currencyService, _associateService, _logger, _settingsService, _dataService) ?? throw new Exception("The payment processor is incorrectly configured.");
                case BraintreeSettings.BRAINTREE_GBP_DISCO_MERCHANTID:
                    return new BraintreeMoneyInGbp(_currencyService, _associateService, _logger, _settingsService, _dataService) ?? throw new Exception("The payment processor is incorrectly configured.");
                case BraintreeSettings.BRAINTREE_NZD_DISCO_MERCHANTID:
                    return new BraintreeMoneyInNzd(_currencyService, _associateService, _logger, _settingsService, _dataService) ?? throw new Exception("The payment processor is incorrectly configured.");
                case BraintreeSettings.BRAINTREE_USD_DISCO_MERCHANTID:
                    return new BraintreeMoneyInUsd(_currencyService, _associateService, _logger, _settingsService, _dataService) ?? throw new Exception("The payment processor is incorrectly configured.");
                default:
                    _logger.LogError(new Exception(), $"Braintree Error: Cannot get token. Merchant ID {mId} is not valid.");
                    throw new Exception($"Braintree Error: Cannot get token. Merchant ID {mId} is not valid.");
            }
        }
    }
}
