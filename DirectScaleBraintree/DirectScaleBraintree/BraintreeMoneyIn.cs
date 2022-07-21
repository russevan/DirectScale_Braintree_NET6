using DirectScaleBraintree.Models;
using DirectScale.Braintree.Repositories;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;
using System;
using System.Linq;
using DirectScale.Disco.Extension.MoneyIn.Custom.Models;

namespace DirectScaleBraintree
{
    public abstract class BraintreeMoneyIn : SavedPaymentMoneyInMerchant
    {
        private BraintreeService _braintreeService;
        private readonly BraintreeSettings _braintreeSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IAssociateService _associateService;
        private readonly ILogger<BraintreeMoneyIn> _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;

        public BraintreeMoneyIn(
            //bool useDSHardcodedCreds,
            ICurrencyService currencyService,
            IAssociateService associateService,
            ILogger<BraintreeMoneyIn> loggingService,
            ISettingsService settingsService,
            IDataService dataService,
            MerchantInfo merchInfo
            ) : base(merchInfo, paymentFormWidth: 400, paymentFormHeight: 500)
        {

            _dataService = dataService;
            _logger = loggingService;
            _settingsService = settingsService;

            DataRepository data = new DataRepository(_dataService, _logger, _settingsService);
            _braintreeSettings = data.GetSettings(merchInfo.Id);

            _currencyService = currencyService;
            _associateService = associateService;
            _braintreeService = new BraintreeService(_braintreeSettings, _logger, associateService);

            base.PaymentFormWidth = _braintreeSettings.IFrameWidth;
            base.PaymentFormHeight = _braintreeSettings.IFrameHeight;
        }

        public void ClearSettings()
        {
            DataRepository.ClearSettings(MerchantInfo.Id);
        }

        public override string GenerateOneTimeAuthToken(string payerId, int associateId = 0, string languageCode = "", string countryCode = "")
        {
            return _braintreeSettings.TokenizationKey;
        }

        public string GetClientToken(string payerId)
        {
            if (string.IsNullOrEmpty(payerId) || payerId == "0")
            {
                return _braintreeService.GetClientTokenWithoutCustomer();
            }
            else
            {
                return _braintreeService.GetClientTokenForExistingBraintreeCustomer(payerId);
            }
        }

        public SavePaymentModel SavePayment(SavePaymentModel paymentIn)
        {
            // First, the customer who owns the payment passed in must be found or created at Braintree.
            _braintreeService.GetPayerIdOrCreate(GetPayerIdAsInt(paymentIn.PayerId));

            // Call the SavePayment method in the Service class. This will update the Nonce to a permanent token.
            return _braintreeService.SavePayment(paymentIn);
        }
        public int GetPayerIdAsInt(string payerId)
        {
            int payerIdAsInt;
            if (payerId.All(char.IsDigit))
            {
                payerIdAsInt = Convert.ToInt32(payerId);
            }
            else
            {
                _logger.LogWarning($"Failed to convert payerId '{payerId}' to int. Expecting integer Disco ID.");
                throw new Exception($"Could not fetch payment information for the customer provided. (ID: {payerId})");
            }
            return payerIdAsInt;
        }
        /*public override string GetNewPayerId(int associateId)
        {
            return _braintreeService.GetPayerIdOrCreate(associateId);
        }*/

        /*public override PaymentMethod[] GetExternalPayments(string payerId)
        {
            int payerIdAsInt = GetPayerIdAsInt(payerId);
            return _braintreeService.GetCustomerPaymentMethods(payerIdAsInt, MerchantInfo.Currency); // The caller checks for null.
        }*/

        public override void DeletePayment(string payerId, string paymentMethodId)
        {
            _braintreeService.DeletePayment(payerId, paymentMethodId);
        }

        public override PaymentResponse ChargePayment(string payerId, NewPayment payment, int orderNumber)
        {
            // Incoming data validation
            if (string.IsNullOrWhiteSpace(payment.PaymentMethodId)) throw new ArgumentNullException(payment.PaymentMethodId);
            if (payment.Amount < _braintreeSettings.MinimumCurrencyAmount)
            {
                return new PaymentResponse
                {
                    Amount = payment.Amount,
                    Response = $"Braintree: Payment amount {payment.Amount} is less than the allowed threshhold of {_braintreeSettings.MinimumCurrencyAmount}",
                    ResponseId = "Error",
                    TransactionNumber = "0",
                    Status = PaymentStatus.Rejected
                };
            }

            // Write new BraintreeException class
            // Mark it as "Serializable", or "MarshalByRef" [Serializable]


            if (_associateService.GetAssociate(GetPayerIdAsInt(payerId)) == null) throw new ArgumentNullException($"PayorID of {payerId} is invalid.");

            var res = new PaymentResponse { Status = PaymentStatus.Rejected };
            try
            {
                var data = new ChargeData
                {
                    Amount = Convert.ToDecimal(payment.Amount),
                    PaymentMethodToken = payment.PaymentMethodId,
                    OrderNumber = orderNumber.ToString(),
                    ChannelPartnerId = _braintreeSettings.ChannelPartnerId
                };
                ChargeResponse chargeResponse = _braintreeService.ChargeAmount(data);

                res.Response = chargeResponse.ProcessorResponseMessage;
                res.ResponseId = chargeResponse.ProcessorResponseCode;
                res.TransactionNumber = chargeResponse.TransactionId;
                res.Status = chargeResponse.IsSuccessful ? PaymentStatus.Accepted : PaymentStatus.Rejected; // There may be more detail here, depending on what comes back from Braintree
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception thrown when sending or processing Braintree payment response for order {orderNumber}.");
            }

            res.Amount = payment.Amount;
            res.OrderNumber = orderNumber;
            res.PaymentType = "Authorization";
            res.Currency = payment?.CurrencyCode?.ToUpper();
            res.Merchant = MerchantInfo.Id;
            res.Redirect = false;
            return res;
        }

        // Tokenization Key vs. GetClientToken
        // There are additional functions Braintree has if you want it to be aware of the customer
        // you're working with. In that case, you can get a client token before render. The way Disco is set up,
        // it's already managing and displaying customer payments, so we don't need BT's help with that.
        public async override Task<AddPaymentFrameData> GetSavePaymentFrame(string payorId, int? associateId, string languageCode, string countryCode, Region region)
        {
            return new AddPaymentFrameData
            {
                IFrameHeight = _braintreeSettings.IFrameHeight,
                IFrameWidth = _braintreeSettings.IFrameWidth,
                IFrameURL = $"{Environment.GetEnvironmentVariable("ExtensionBaseURL")}/Merchants/AddCardFrame/Braintree?payorId={payorId}&merchantId={_braintreeSettings.DiscoMerchantId}"
            };
        }

        public async override Task<ExtendedPaymentResult> RefundPayment(string payerId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string referenceNumber, string transactionNumber, string authorizationCode)
        {
            int payerIdAsInt = GetPayerIdAsInt(payerId);
            var associate = await _associateService.GetAssociate(payerIdAsInt) ?? throw new ArgumentNullException($"PayorId of {payerId} is invalid.");

            //paymentAmount = await _currencyService.Round(paymentAmount, currencyCode);
            //refundAmount = await _currencyService.Round(refundAmount, currencyCode);
            var refundTrans = new RefundTransaction
            {
                Id = transactionNumber,
                RefundData = new RefundData
                {
                    Amount = paymentAmount,
                    PartialAmount = refundAmount,
                    Currency = currencyCode.ToUpper()
                }
            };

            var response = await _braintreeService.RefundTransaction(refundTrans);

            _logger.LogInformation($"Processed refund for order {orderNumber}. TransactionId: {transactionNumber}, Amount: {refundTrans.RefundData.Amount}, Returned status: {response}.");

            return new ExtendedPaymentResult
            {
                Amount = refundAmount,
                AuthorizationCode = authorizationCode,
                Currency = currencyCode.ToUpper(),
                TransactionNumber = transactionNumber,
                ResponseId = "0",
                Response = response.Message,
                Status = response.Message.Equals("success", StringComparison.OrdinalIgnoreCase) ? PaymentStatus.Accepted : PaymentStatus.Rejected,
                SpecialInstructionsURL = ""
            };
        }
    }
}
