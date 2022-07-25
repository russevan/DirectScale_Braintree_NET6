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

            return services;
        }
        public static IApplicationBuilder UseDirectScaleBraintree(this IApplicationBuilder app)
        {
            app.Map("/api/Merchants/BrainTreeLPM/Callback", x => x.UseMiddleware<BraintreeLPMCallBackMiddleware>());
            app.Map("/api/Merchants/BrainTreeLPM/Redirect", x => x.UseMiddleware<BraintreeLPMRedirectPageMiddleware>());

            return app;
        }
        public static void AddIdealBraintreeLocalPaymentMethod(this SetupOptions setupOptions, int merchantId)
        {
            setupOptions.AddMerchant<IdealLocalPaymentMethodRedirectMoneyInEur>(merchantId, "BrainTree iDEAL (EUR)", "iDEAL BrainTree Local Payment Method for The Netherlands", "EUR");
        }
    }
}
