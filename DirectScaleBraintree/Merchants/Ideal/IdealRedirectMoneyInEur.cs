using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.MoneyIn.Custom.Models;
using DirectScaleBraintree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Merchants.Ideal
{
    public class IdealLocalPaymentMethodRedirectMoneyInEur : RedirectMoneyInMerchant
    {
        private IBraintreeLocalPaymentMethodsService _brainTreeLocalPaymentMethodsService;
        public IdealLocalPaymentMethodRedirectMoneyInEur(IBraintreeLocalPaymentMethodsService brainTreeLocalPaymentMethodsService)
        {
            _brainTreeLocalPaymentMethodsService = brainTreeLocalPaymentMethodsService ?? throw new ArgumentNullException(nameof(brainTreeLocalPaymentMethodsService));
        }

        public async override Task<PaymentRedirectResult> RedirectPayment(int associateId, int orderNumber, Address billingAddress, double amount, string currencyCode, string returnUrl)
        {
            var result = new PaymentRedirectResult()
            {
                AuthorizationCode = orderNumber + "_AuthCode",
                ReferenceNumber = orderNumber + "_ReferenceNumber",
                TransactionNumber = orderNumber + "_TransactionNumber",
                RedirectUrl = "https://5826-2601-680-8100-fff0-4b7-62db-9d07-d8a7.ngrok.io" + "/Merchants/BrainTreeLPM/Redirect" // TODO: UPDATE THIS TO BE DYNAMIC
            };

            return await Task.FromResult(result);
        }

        public async override Task<ExtendedPaymentResult> RefundPayment(string payerId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string referenceNumber, string transactionNumber, string authorizationCode)
        {
            await _brainTreeLocalPaymentMethodsService.Refund(referenceNumber, (decimal)refundAmount);
            throw new NotImplementedException();
        }

        //public override Task<string> GetNewPayerId(int associateId)
        //{
        //    return base.GetNewPayerId(associateId);
        //}

        //public override Task<ValidationResult> ValidateCurrency(string currencyCode)
        //{
        //    return base.ValidateCurrency(currencyCode);
        //}

        //public override Task<ValidationResult> ValidatePayment(string payerId, NewPayment payment)
        //{
        //    return base.ValidatePayment(payerId, payment);
        //}
    }
}