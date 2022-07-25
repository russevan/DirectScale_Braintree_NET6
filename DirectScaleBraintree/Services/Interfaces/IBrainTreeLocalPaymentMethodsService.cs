using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Services.Interfaces
{
    public interface IBraintreeLocalPaymentMethodsService
    {
        Task Refund(string transactionId, decimal? amount = null);
    }
}
