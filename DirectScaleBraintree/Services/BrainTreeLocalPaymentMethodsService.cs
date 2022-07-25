using Braintree;
using DirectScaleBraintree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Services
{
    public class BraintreeLocalPaymentMethodsService : IBraintreeLocalPaymentMethodsService
    {
        // https://docs-prod-us-east-2.production.braintree-api.com/reference/request/transaction/refund/dotnet
        public async Task Refund(string transactionId, decimal? amount = null)
        {
//            Result<Transaction> result = gateway.Transaction.Refund(transactionId, amount
//);

//            result.IsSuccess()
//// true
//Transaction refund = result.Target;
//            refund.Type;
//            // TransactionType.CREDIT
//            refund.Amount;
//            // 50.00

//            Result<Transaction> result = gateway.Transaction.Refund(
//                "a_transaction_id",
//                10.00M
//            );

//            result.IsSuccess()
//// true
//Transaction refund = result.Target;
//            refund.Type;
//            // TransactionType.CREDIT
//            refund.Amount;
//            // 10.00
            throw new NotImplementedException();
        }
    }
}
