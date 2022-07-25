using DirectScaleBrainTree.Merchants.Ideal;
using DirectScale.Disco.Extension.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using DirectScaleBrainTree.Services.Interfaces;
using DirectScaleBrainTree.Services;

namespace DirectScaleBrainTree.Middleware
{
    public static class DirectScaleBrainTreeExtensionMethods
    {
        public static IServiceCollection AddDirectScaleBrainTree(this IServiceCollection services)
        {
            // Register Classes needed
            services.AddSingleton<IBrainTreeLocalPaymentMethodsService, BrainTreeLocalPaymentMethodsService>();

            return services;
        }
        public static IApplicationBuilder UseDirectScaleBrainTree(this IApplicationBuilder app)
        {
            app.Map("/api/Merchants/BrainTreeLPM/Callback", x => x.UseMiddleware<BrainTreeLPMCallBackMiddleware>());

            return app;
        }
        public static void AddIdealBrainTreeLocalPaymentMethod(this SetupOptions setupOptions, int merchantId)
        {
            setupOptions.AddMerchant<IdealLocalPaymentMethodRedirectMoneyInEur>(merchantId, "BrainTree iDEAL (EUR)", "iDEAL BrainTree Local Payment Method for The Netherlands", "EUR");
        }
    }
}
