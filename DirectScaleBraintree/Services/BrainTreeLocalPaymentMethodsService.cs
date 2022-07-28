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
        protected readonly IBraintreeSettingsService _braintreeSettingsService;
        public BraintreeLocalPaymentMethodsService(IBraintreeSettingsService braintreeSettingsService)
        {
            _braintreeSettingsService = braintreeSettingsService ?? throw new ArgumentNullException(nameof(braintreeSettingsService));
        }

        public Task<Result<Transaction>> CreateTransaction(TransactionRequest transactionRequest)
        {
            var gateway = new BraintreeGateway(
                _braintreeSettingsService.Environment, 
                _braintreeSettingsService.MerchantId, 
                _braintreeSettingsService.PublicKey,
                _braintreeSettingsService.PrivateKey
                );

            return Task.FromResult(gateway.Transaction.Sale(transactionRequest));
        }

        public Task<Result<Transaction>> RefundTransaction(string transactionId, decimal amount)
        {

            var gateway = new BraintreeGateway(
                _braintreeSettingsService.Environment,
                _braintreeSettingsService.MerchantId,
                _braintreeSettingsService.PublicKey,
                _braintreeSettingsService.PrivateKey
                );

            return Task.FromResult(gateway.Transaction.Refund(transactionId, amount));
        }
    }
}
