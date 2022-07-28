using DirectScaleBraintree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Services
{
    public class BraintreeSettingsService : IBraintreeSettingsService
    {
        public string Environment { get { return System.Environment.GetEnvironmentVariable("BraintreeEnvironment") ?? String.Empty; } }
        public string MerchantId { get { return System.Environment.GetEnvironmentVariable("BraintreeMerchantId") ?? String.Empty; } }
        public string PublicKey { get { return System.Environment.GetEnvironmentVariable("BraintreePublicKey") ?? String.Empty; } }
        public string PrivateKey { get { return System.Environment.GetEnvironmentVariable("BraintreePrivateKey") ?? String.Empty; } }
        public string TokenizationKey { get { return System.Environment.GetEnvironmentVariable("BraintreeTokenizationKey") ?? String.Empty; } }

        public string MerchantAccountId(string currencyCode)
        {
            return System.Environment.GetEnvironmentVariable($"Braintree_MerchantAccountId_{currencyCode.ToUpper()}") ?? String.Empty;
        }
    }
}
