using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Services.Interfaces
{
    public interface IBraintreeLocalPaymentMethodsService
    {
        Task <Result<Transaction>> RefundTransaction(string transactionId, decimal amount);
        Task<Result<Transaction>> CreateTransaction(TransactionRequest transactionRequest);
    }
}
