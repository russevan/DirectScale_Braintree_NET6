using Braintree;
using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;
using DirectScaleBraintree.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Middleware
{
    internal class BraintreeLPMCreateTransactionMiddleware
    {
        private readonly IOrderService _orderService;
        private readonly IBraintreeLocalPaymentMethodsService _braintreeLocalPaymentMethodsService;

        public BraintreeLPMCreateTransactionMiddleware(RequestDelegate next, IBraintreeLocalPaymentMethodsService braintreeLocalPaymentMethodsService, IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _braintreeLocalPaymentMethodsService = braintreeLocalPaymentMethodsService ?? throw new ArgumentNullException(nameof(braintreeLocalPaymentMethodsService));
        }
        public async Task InvokeAsync(HttpContext context)
        {
            string bodyString = string.Empty;
            if (context.Request.ContentType == "application/json")
            {
                using (var reader = new StreamReader(context.Request.Body))
                {
                    bodyString = await reader.ReadToEndAsync();
                }

            }
            var requestBody = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestBody>(bodyString);
            var orderNumber = requestBody.OrderNumber;
            var nonce = requestBody.Nonce;
            var merchantAccountId = requestBody.MerchantAccountId;
            var orderInfo = await _orderService.GetOrderByOrderNumber(orderNumber);
            var pendingOrderPayments = orderInfo.Payments.Where(x => x.IsPending == true).ToArray();

            // TODO: ADD check here and don't assume first payment

            var pendingOrderPayment = pendingOrderPayments[0];

            TransactionRequest request = new TransactionRequest
            {
                Amount = (decimal)pendingOrderPayment.Amount,
                PaymentMethodNonce = nonce,
                OrderId = orderNumber.ToString(),
                MerchantAccountId = merchantAccountId,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true, // Required
                }
            };

            var transactionResult = await _braintreeLocalPaymentMethodsService.CreateTransaction(request);

            if (transactionResult.IsSuccess())
            {
                var transactionId = transactionResult.Target.Id;
                var paymentUpdate = new DirectScale.Disco.Extension.OrderPaymentStatusUpdate()
                {
                    OrderPaymentId = pendingOrderPayment.PaymentId,
                    AuthorizationNumber = pendingOrderPayment.AuthorizationNumber,
                    PaymentStatus = PaymentStatus.Pending,
                    PayType = pendingOrderPayment.PayType,
                    ReferenceNumber = nonce,
                    TransactionNumber = pendingOrderPayment.TransactionNumber,
                    ResponseDescription = pendingOrderPayment.PaymentResponse,
                    SavedPaymentId = pendingOrderPayment.SavedPaymentId
                };

                if (transactionResult.Target.Status == TransactionStatus.SETTLING) // TODO: Need to add whatever is considered paid
                {
                    // TODO: Add logging
                    //System.Console.WriteLine("Transaction ID: " + result.Target.Id);
                    paymentUpdate.TransactionNumber = transactionId;
                    paymentUpdate.PaymentStatus = DirectScale.Disco.Extension.PaymentStatus.Accepted;

                    await _orderService.FinalizeOrderPaymentStatus(paymentUpdate);
                }
                else if (transactionResult.Target.Status == TransactionStatus.PROCESSOR_DECLINED) // TODO: Need to add whatever is considered failed
                {
                    paymentUpdate.TransactionNumber = transactionId;
                    paymentUpdate.PaymentStatus = DirectScale.Disco.Extension.PaymentStatus.Rejected;
                    // TODO: return here
                }
                else
                {
                    // TODO LOG HERE. return failure?
                }
                orderInfo = await _orderService.GetOrderByOrderNumber(orderNumber);

                //_loggingService.LogInformation($"{_className}.ProcessPaymentUpdate - Finalizing Order {orderInfo.OrderNumber}.");

                if (orderInfo.Status == OrderStatus.Paid)
                {
                    _orderService.FinalizeAcceptedOrder(orderInfo);
                }
                else if (orderInfo.Status == OrderStatus.Declined)
                {
                    _orderService.FinalizeNonAcceptedOrder(orderInfo);
                }
                else
                {
                    //_loggingService.LogInformation($"{_className}.ProcessPaymentUpdate - The status of Order {orderInfo.OrderNumber} is neither Paid or Declined. Order not finalized.");
                }
            }
            else
            {
                throw new Exception($"Couldn't create a transaction: {transactionResult.Message}."); // is this declined or failed?
                // TODO: Add logging
                //System.Console.WriteLine(result.Message);
            }


            context.Response.StatusCode = 204;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("");
        }

        internal class RequestBody
        {
            public int OrderNumber { get; set; }
            public string Nonce { get; set; }
            public string MerchantAccountId { get; set; }

        }
    }
}
