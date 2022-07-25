using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBrainTree.Services.Interfaces
{
    public interface IBrainTreeLocalPaymentMethodsService
    {
        Task RefundPayment(string transactionId, decimal? amount = null);
    }
}
