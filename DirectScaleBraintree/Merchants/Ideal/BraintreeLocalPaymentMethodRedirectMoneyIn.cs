using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.MoneyIn.Custom.Models;
using DirectScale.Disco.Extension.Services;
using DirectScaleBraintree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Merchants.Ideal
{
    public class BraintreeLocalPaymentMethodRedirectMoneyIn : RedirectMoneyInMerchant
    {
        private IBraintreeLocalPaymentMethodsService _brainTreeLocalPaymentMethodsService;
        public BraintreeLocalPaymentMethodRedirectMoneyIn(IBraintreeLocalPaymentMethodsService brainTreeLocalPaymentMethodsService)
        {
            _brainTreeLocalPaymentMethodsService = brainTreeLocalPaymentMethodsService ?? throw new ArgumentNullException(nameof(brainTreeLocalPaymentMethodsService));
        }

        public async override Task<PaymentRedirectResult> RedirectPayment(int associateId, int orderNumber, Address billingAddress, double amount, string currencyCode, string returnUrl)
        {
            if (returnUrl == null) { returnUrl = String.Empty; }
            var result = new PaymentRedirectResult()
            {
                AuthorizationCode = orderNumber + "_AuthCode",
                ReferenceNumber = orderNumber + "_ReferenceNumber",
                TransactionNumber = orderNumber + "_TransactionNumber",
                RedirectUrl = System.Environment.GetEnvironmentVariable("ExtensionURL")?.ToString() +
                $"/Merchants/BrainTreeLPM/Redirect?associateId={associateId}&orderNumber={orderNumber}&currencyCode={Uri.EscapeDataString(currencyCode)}&returnUrl={Uri.EscapeDataString(returnUrl)}"
            };

            return await Task.FromResult(result);
        }

        public async override Task<ExtendedPaymentResult> RefundPayment(string payerId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string referenceNumber, string transactionNumber, string authorizationCode)
        {
            var result = await _brainTreeLocalPaymentMethodsService.RefundTransaction(transactionNumber, (decimal)refundAmount);
            
            var extendedPaymentResult = new ExtendedPaymentResult
            {
                Amount = (double)(result?.Target.Amount ?? 0) * -1.0,
                AuthorizationCode = authorizationCode,
                Currency = result?.Target.CurrencyIsoCode,
                ResponseId = "",
                Response = "",
                TransactionNumber = "",
                Status = PaymentStatus.Pending
            };

            if (result != null)
            {
                var refundTransactionId = result?.Target?.Id;
                string response = null;


                if (result?.IsSuccess() ?? false)
                {
                    response = result.Message;
                    extendedPaymentResult.Response = response;
                    extendedPaymentResult.TransactionNumber = refundTransactionId;

                    if (result?.Target?.Status == Braintree.TransactionStatus.SETTLING) // TODO: Does this mean refunded? are there more "refunded" statuses.
                    {
                        extendedPaymentResult.Status = PaymentStatus.Accepted;
                    }
                    else if (result?.Target?.Status == Braintree.TransactionStatus.PROCESSOR_DECLINED) // Does this mean the refund failed? what other statuses?
                    {
                        extendedPaymentResult.Status = PaymentStatus.Rejected;
                    }
                    else
                    {
                        // TODO: Log
                    }
                }
                else
                {
                    extendedPaymentResult.Response = result.Errors.ToString(); // TODO: is ToString() Overloaded?
                    List<Braintree.ValidationError> errors = result.Errors.DeepAll();
                    // TODO: Log
                }

            }
            return await Task.FromResult(extendedPaymentResult);
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