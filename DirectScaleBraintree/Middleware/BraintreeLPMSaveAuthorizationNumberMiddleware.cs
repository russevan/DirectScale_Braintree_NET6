using DirectScale.Disco.Extension.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Middleware
{
    internal class BraintreeLPMSaveAuthorizationNumberMiddleware
    {
        private readonly IOrderService _orderService;
        public BraintreeLPMSaveAuthorizationNumberMiddleware(RequestDelegate next, IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
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
            var orderId = requestBody.OrderNumber;
            var authNumber = requestBody.AuthorizationNumber;
            var order = await _orderService.GetOrderByOrderNumber(orderId);
            var pendingOrderPayments = order.Payments.Where(x => x.IsPending == true).ToArray();

            // TODO ADD check here and don't assume first payment

            var pendingOrderPayment = pendingOrderPayments[0];

            var paymentUpdate = new DirectScale.Disco.Extension.OrderPaymentStatusUpdate()
            {
                OrderPaymentId = pendingOrderPayment.PaymentId,
                AuthorizationNumber = authNumber,
                PaymentStatus = DirectScale.Disco.Extension.PaymentStatus.Pending,
                PayType = pendingOrderPayment.PayType,
                ReferenceNumber = pendingOrderPayment.Reference,
                TransactionNumber = pendingOrderPayment.TransactionNumber,
                ResponseDescription = pendingOrderPayment.PaymentResponse,
                SavedPaymentId = pendingOrderPayment.SavedPaymentId
            };

            await _orderService.FinalizeOrderPaymentStatus(paymentUpdate);

            context.Response.StatusCode = 204;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("");
        }

        internal class RequestBody
        {
            public int OrderNumber { get; set; }
            public string AuthorizationNumber { get; set; }

        }
    }
}
