using bt = Braintree;
using System;
using Braintree.Exceptions;
using Braintree;
using DirectScale.Disco.Extension.Services;
using DirectScale.Disco.Extension;
using DirectScaleBraintree.Models;
using PaymentMethod = DirectScale.Disco.Extension.PaymentMethod;
using System.Linq;
using DirectScaleBraintree.Interfaces;

namespace DirectScaleBraintree
{
    public class BraintreeService : IBraintreeService
    {
        private readonly BraintreeSettings _braintreeSettings;
        private readonly ILoggingService _logger;
        private static readonly string _className = typeof(BraintreeService).FullName;
        private readonly IAssociateService _associateService;

        public BraintreeService(BraintreeSettings btSettings, ILoggingService logger, IAssociateService associateService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _associateService = associateService ?? throw new ArgumentNullException(nameof(associateService));
            _braintreeSettings = btSettings ?? throw new ArgumentNullException(nameof(associateService));
        }

        public string GetClientTokenForExistingBraintreeCustomer(string customerId)
        {
            if (GetBraintreeCustomer(customerId) != null)
            {
                return BraintreeGateway.ClientToken.Generate(new ClientTokenRequest { CustomerId = customerId, MerchantAccountId = _braintreeSettings.BraintreeMerchantAccountId });
            }
            throw new Exception($"Client Token Fetch Failed: Merchant processor account was not found for customer ID {customerId}.");
        }

        public string GetClientTokenWithoutCustomer()
        {
            return BraintreeGateway.ClientToken.Generate(new ClientTokenRequest { MerchantAccountId = _braintreeSettings.BraintreeMerchantAccountId });
        }

        public Customer CreateCustomer(Associate associate)
        {
            try
            {
                _logger.LogInformation($"BraintreeService > CreateCustomer - Attempting to create customer Id={associate.AssociateId},Email={associate.EmailAddress}");
                var request = new CustomerRequest
                {
                    Id = associate.AssociateId.ToString(), // We'll specify our own ID, using the unique ID of the distributor. 
                    FirstName = associate.DisplayFirstName,
                    LastName = associate.DisplayLastName,
                    Company = associate.CompanyName,
                    Email = associate.EmailAddress,
                    Phone = associate.TextNumber
                };

                var gateway = BraintreeGateway;
                Result<Customer> result = gateway.Customer.Create(request);

                bool success = result.IsSuccess();
                if (success)
                {
                    Customer customer = result.Target;
                    return customer;
                }
                else
                {
                    foreach (ValidationError error in result.Errors.DeepAll())
                    {
                        _logger.LogError(new Exception(), $"{_className}.CreateCustomer - Failed to Create Braintree Customer. AssociateId: {associate.AssociateId} Message: {error.Message}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_className}.CreateCustomer - Exception thrown while Create Braintree Customer. AssociateId: {associate.AssociateId}");
            }
            return null;
        }

        public ChargeResponse ChargeAmount(ChargeData data)
        {
            ChargeResponse responseMsg = null;
            try
            {
                var gateway = BraintreeGateway;
                TransactionRequest request = new TransactionRequest
                {
                    Amount = data.Amount,
                    PaymentMethodToken = data.PaymentMethodToken, // For a one-time payment without storing the card, we'll use the "PaymentMethodNonce". For now, we're not working that way.
                    MerchantAccountId = _braintreeSettings.BraintreeMerchantAccountId,
                    OrderId = data.OrderNumber,
                    Channel = data.ChannelPartnerId,
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true,
                        StoreInVault = true
                    }
                };

                Result<Transaction> result = gateway.Transaction.Sale(request);
                Transaction transaction = result.Target;

                if (result.IsSuccess())
                {
                    responseMsg = new ChargeResponse
                    {
                        IsSuccessful = true,
                        TransactionId = transaction.Id,
                        ProcessorResponseCode = transaction.ProcessorResponseCode,
                        ProcessorResponseMessage = transaction.ProcessorResponseText,
                        ProcessorResponseAdditionalData = transaction.AdditionalProcessorResponse
                    };
                }
                else
                {
                    responseMsg = new ChargeResponse
                    {
                        IsSuccessful = false,
                        ProcessorResponseCode = "-1",
                        ProcessorResponseMessage = result.Message
                    };
                    foreach (ValidationError error in result.Errors.DeepAll())
                    {
                        _logger.LogError(new Exception(), $"{_className}.ChargeAmount - Failed to Charge Amount. Amount: {data.Amount},Order: {data.OrderNumber}, Error: {error.Message}");
                        responseMsg.Errors.Add($"{error.Code}|{error.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_className}.ChargeAmount - Exception thrown while Charge Amount. Amount: {data.Amount},Order: {data.OrderNumber}", ex);
            }

            return responseMsg ?? new ChargeResponse { IsSuccessful = false, ProcessorResponseCode = "0", ProcessorResponseMessage = "A non-processor error occurred." };
        }

        public async Task<RefundResponse> RefundTransaction(RefundTransaction refundData)
        {
            string message = "";
            try
            {
                RefundResponse success = new RefundResponse { Message = "success" };
                var gateway = BraintreeGateway;

                Transaction transaction = gateway.Transaction.Find(refundData.Id);
                if (transaction.Amount > 0)
                {
                    if (transaction.Status == TransactionStatus.SETTLED || transaction.Status == TransactionStatus.SETTLING)
                    {
                        if (refundData.RefundData.Amount.Equals(refundData.RefundData.PartialAmount))
                        {
                            Result<Transaction> result = gateway.Transaction.Refund(refundData.Id);
                            if (result.IsSuccess())
                            {
                                return success;
                            }
                            else
                            {
                                foreach (ValidationError error in result.Errors.DeepAll())
                                {
                                    _logger.LogError(new Exception(), $"{_className}.RefundTransaction - Failed to Refund Transaction. Amount: {refundData.RefundData.PartialAmount},Id: {refundData.Id}, Message: {error.Message}");
                                    message += error.Message;
                                }
                            }
                        }
                        else
                        {
                            Result<Transaction> result = gateway.Transaction.Refund(refundData.Id, Convert.ToDecimal(refundData.RefundData.PartialAmount));
                            if (result.IsSuccess())
                            {
                                return success;
                            }
                            else
                            {
                                foreach (ValidationError error in result.Errors.DeepAll())
                                {
                                    _logger.LogError(new Exception(), $"{_className}.RefundTransaction - Failed to Refund Transaction. Amount: {refundData.RefundData.PartialAmount},Id: {refundData.Id}, Message: {error.Message}");
                                    message += error.Message;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (refundData.RefundData.Amount.Equals(refundData.RefundData.PartialAmount))
                        {
                            Result<Transaction> result = gateway.Transaction.Void(refundData.Id);
                            if (result.IsSuccess())
                            {
                                return success;
                            }
                            else
                            {
                                foreach (ValidationError error in result.Errors.DeepAll())
                                {
                                    _logger.LogError(new Exception(), $"{_className}.RefundTransaction - Failed to RefundTransaction. Amount: {refundData.RefundData.PartialAmount},Id: {refundData.Id}, Message: {error.Message}");
                                    message += error.Message;
                                }
                            }
                        }
                        else
                        {
                            return new RefundResponse { Message = "Partial Refund not allowed if transaction is not settled!" };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.LogError(e, $"{_className}.RefundTransaction - Exception thrown while Refund Transaction. Amount: {refundData.RefundData.PartialAmount},Id: {refundData.Id}", e);
            }

            return new RefundResponse { Message = message != "" ? message : "failed" };
        }

        private IBraintreeGateway _gateway;
        private IBraintreeGateway BraintreeGateway
        {
            get
            {
                if (_gateway == null) _gateway = new BraintreeGateway(
                    _braintreeSettings.BraintreeEnvironment,
                    _braintreeSettings.BraintreeMerchantId,
                    _braintreeSettings.PublicKey,
                    _braintreeSettings.PrivateKey

                );
                return _gateway;
            }
        }

        public Customer GetBraintreeCustomer(string customerId)
        {
            try { return BraintreeGateway.Customer.Find(customerId); }
            catch (NotFoundException) { return null; }
        }

        public SavePaymentModel SavePayment(SavePaymentModel request)
        {
            _logger.LogInformation($"{_className}.SavePayment", $"Entered SavePayment. payorId: {request.PayerId}, Token: {request.PaymentNonce}, Name: {request.CardHolderName}.");

            // By this time, the customer already exists in Braintree (see "GetPayorIdOrCreate")
            try
            {
                var createRequest = new PaymentMethodRequest
                {
                    CustomerId = request.PayerId,
                    PaymentMethodNonce = request.PaymentNonce,
                    DeviceData = request.DeviceData
                };

                Result<bt.PaymentMethod> paymentCreateResult = BraintreeGateway.PaymentMethod.Create(createRequest);

                if (paymentCreateResult.IsSuccess()) return GetPaymentInfoFromBraintreePaymentMethod(paymentCreateResult.Target, request);
                else throw new Exception($"Could not save card: {paymentCreateResult.Message}");
                
            }
            catch (Exception e)
            {
                _logger.LogError(new Exception(), $"{_className}.SavePayment", $"Exception thrown when saving Braintree payment. Payor: {request.PayerId}. Exception: {e}");
                throw e;
            }
        }

        // This will ensure that a customer account exists for Braintree, given the information at hand.
        // It will check for an account (we've decided to use the Disco Associate ID as the ID of the customer, @see CreateCustomer)
        // and if it's not found, it will create one.
        public async Task<string> GetPayerIdOrCreate(int associateId)
        {
            var associate = await _associateService.GetAssociate(associateId);

            if (associate == null)
            {
                throw new ArgumentNullException($"AssociateID of {associateId} is invalid.");
            }

            // The payor ID is valid and exists in the Disco system. Let's see if this person exists in Braintree:
            var btCust = GetBraintreeCustomer(associateId.ToString());

            if (btCust == null) // The account does NOT exist at Braintree. Create a new customer. ***
            {
                btCust = CreateCustomer(associate);

                if (btCust == null || string.IsNullOrEmpty(btCust.Id))
                {
                    _logger.LogError(new Exception(), $"{ _className}.GetPayorIdOrCreate - Braintree Customer not created for associate {associateId}.");
                    throw new Exception("Customer account could not be created with specified payment information."); // This may need more data from the response object. Why did the create fail?
                }
            }
            return btCust.Id;
        }

        public PaymentMethod[] GetCustomerPaymentMethods(int payerId, string currencyCode)
        {
            var customer = GetBraintreeCustomer(payerId.ToString());
            if (customer == null) { throw new Exception($"Associate with ID {payerId} not found while fetching Braintree {currencyCode} payment method list."); }

            var paymentMethodList = customer.PaymentMethods;
            var iteration = 0;
            if (paymentMethodList.Length > 0)
            {
                var discoPaymentList = new PaymentMethod[paymentMethodList.Length];
                foreach (var m in paymentMethodList)
                {
                    var payInfo = GetPaymentInfoFromBraintreePaymentMethod(m, new SavePaymentModel());
                    var singlePaymentMethod = new PaymentMethod
                    {
                        Ending = payInfo.LastFour,
                        AssociateId = payerId,
                        Name = payInfo.CardType,
                        PaymentMethodId = payInfo.PaymentToken,
                        CanDelete = true,
                        MerchantId = _braintreeSettings.DiscoMerchantId,
                        Expires = GetCardExpirationFromBraintreePaymentMethod(m),
                        IconClass = payInfo.CardType.ToLower(),
                        CurrencyCode = currencyCode
                    };
                    discoPaymentList[iteration++] = singlePaymentMethod;
                }
                return discoPaymentList;
            }
            else return null; // The caller's caller looks for a null response
        }

        public void DeletePayment(string payorId, string paymentMethodId)
        {
            try
            {
                BraintreeGateway.PaymentMethod.Delete(paymentMethodId);
            }
            catch (NotFoundException e)
            {
                _logger.LogError(e, $"Payment with ID (token) of {paymentMethodId} not found on the Braintree system.");
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Payment with ID (token) of {paymentMethodId} not deleted for an unknown reason. {exc.Message}");
            }
        }

        public SavePaymentModel GetPaymentInfoFromBraintreePaymentMethod(bt.PaymentMethod btPaymentMethod, SavePaymentModel dsModel)
        {
            SavePaymentModel paymentOut = dsModel.Clone();

            // Clear the one-time-use token, because it's already been used
            paymentOut.PaymentNonce = string.Empty;

            var typeName = btPaymentMethod.GetType().Name;
            switch (typeName)
            {
                case "PayPalAccount":
                    var payPalInfo = (PayPalAccount)btPaymentMethod;
                    paymentOut.CardType = "PayPal";
                    paymentOut.LastFour = payPalInfo.Email;
                    paymentOut.PaymentToken = payPalInfo.Token;
                    break;
                case "CreditCard":
                    var creditCardInfo = (CreditCard)btPaymentMethod;
                    paymentOut.CardType = creditCardInfo.CardType.GetDescription();
                    paymentOut.LastFour = creditCardInfo.LastFour;
                    paymentOut.PaymentToken = creditCardInfo.Token;
                    paymentOut.ExpireMonth = creditCardInfo.ExpirationMonth;
                    paymentOut.ExpireYear = creditCardInfo.ExpirationYear;
                    break;
                case "VenmoAccount":
                    var venmoInfo = (VenmoAccount)btPaymentMethod;
                    paymentOut.CardType = "Venmo";
                    paymentOut.LastFour = venmoInfo.Username;
                    paymentOut.PaymentToken = venmoInfo.Token;
                    break;
                default:
                    throw new Exception($"Payment card type {typeName} is not yet supported.");
            }
            return paymentOut;
        }

        public DateTime GetCardExpirationFromBraintreePaymentMethod(bt.PaymentMethod btPaymentMethod)
        {
            var typeName = btPaymentMethod.GetType().Name;

            switch (typeName)
            {
                case "CreditCard":
                    var creditCardInfo = (CreditCard)btPaymentMethod;
                    if (creditCardInfo.ExpirationYear.All(char.IsDigit) && creditCardInfo.ExpirationMonth.All(char.IsDigit))
                        return new DateTime(Convert.ToInt32(creditCardInfo.ExpirationYear), Convert.ToInt32(creditCardInfo.ExpirationMonth), 1);
                    else
                        return new DateTime(2099, 1, 1);
                default:
                    return new DateTime(2099, 1, 1);
            }
        }
    }
}
