using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Services.Interfaces
{
    public interface IBraintreeSettingsService
    {

        string Environment { get; }
        string MerchantId { get; }
        string PublicKey { get; }
        string PrivateKey { get;  }
        string TokenizationKey { get; }
        string MerchantAccountId(string currencyCode);
    }
}
