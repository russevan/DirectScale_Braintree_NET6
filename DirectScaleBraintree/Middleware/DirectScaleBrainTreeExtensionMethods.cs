using DirectScaleBraintree.Merchants.Ideal;
using DirectScale.Disco.Extension.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using DirectScaleBraintree.Services.Interfaces;
using DirectScaleBraintree.Services;
using DirectScaleBraintree.Middleware;

namespace DirectScaleBraintree.Middleware
{
    public static class DirectScaleBraintreeExtensionMethods
    {
        public static IServiceCollection AddDirectScaleBraintree(this IServiceCollection services)
        {
            // Register Classes needed
            services.AddSingleton<IBraintreeLocalPaymentMethodsService, BraintreeLocalPaymentMethodsService>();
            services.AddSingleton<IBraintreeSettingsService, BraintreeSettingsService>();

            return services;
        }
        public static IApplicationBuilder UseDirectScaleBraintree(this IApplicationBuilder app)
        {
            app.Map("/api/Merchants/BrainTreeLPM/Callback", x => x.UseMiddleware<BraintreeLPMCallBackMiddleware>());
            app.Map("/Merchants/BrainTreeLPM/Redirect", x => x.UseMiddleware<BraintreeLPMRedirectPageMiddleware>());
            app.Map("/Merchants/BrainTreeLPM/SaveAuthorizationNumber", x => x.UseMiddleware<BraintreeLPMSaveAuthorizationNumberMiddleware>());
            app.Map("/Merchants/BrainTreeLPM/CreateTransaction", x => x.UseMiddleware<BraintreeLPMCreateTransactionMiddleware>());

            return app;
        }
        public static void AddBraintreeLocalPaymentMethodMerchant(this SetupOptions setupOptions, int merchantId, string name, string description, string currencyCode)
        {
            setupOptions.AddMerchant<BraintreeLocalPaymentMethodRedirectMoneyIn>(merchantId, name, description, currencyCode);
        }
    }
}
