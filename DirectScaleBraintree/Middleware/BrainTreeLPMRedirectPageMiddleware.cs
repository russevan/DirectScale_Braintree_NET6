using DirectScale.Disco.Extension.Services;
using DirectScaleBraintree.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DirectScaleBraintree.Middleware
{
    internal class BraintreeLPMRedirectPageMiddleware
    {
        private readonly IOrderService _orderService;
        private readonly IAssociateService _associateService;
        private readonly IBraintreeSettingsService _braintreeSettingsService;

        public BraintreeLPMRedirectPageMiddleware(RequestDelegate next, IBraintreeSettingsService braintreeSettingsService, IOrderService orderService, IAssociateService associateService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _associateService = associateService ?? throw new ArgumentNullException(nameof(associateService));
            _braintreeSettingsService = braintreeSettingsService ?? throw new ArgumentNullException(nameof(braintreeSettingsService));
        }
        private string AppendParameterToUrlQueryString(string url, string parameterName, string value)
        {
            var uriBuilder = new UriBuilder(url);
            var uriQueryParams = HttpUtility.ParseQueryString(uriBuilder.Query);

            uriQueryParams.Add(Uri.EscapeDataString(parameterName), Uri.EscapeDataString(value));

            uriBuilder.Query = uriQueryParams.ToString();
            return uriBuilder.ToString();
        }
        public async Task InvokeAsync(HttpContext context)
        {
            int.TryParse(context.Request.Query["orderNumber"], out int orderNumber);
            // TODO: error handling for orderNumber not present
            int.TryParse(context.Request.Query["associateId"], out int asssociateId);
            // TODO: error handling for orderNumber not present
            string returnUrl = context.Request.Query["returnUrl"];
            var successReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? String.Empty : AppendParameterToUrlQueryString(returnUrl, "paymentSuccessful", "true");
            var successReturnUrlJs = string.IsNullOrWhiteSpace(successReturnUrl) ? String.Empty : $"window.location.replace(\"{successReturnUrl}\"); // using replace so user cannot go back to prior page.";
            var failedReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? String.Empty : AppendParameterToUrlQueryString(returnUrl, "paymentSuccessful", "false");
            var failedReturnUrlJs = string.IsNullOrWhiteSpace(failedReturnUrl) ? String.Empty : $"window.location.replace(\"{failedReturnUrl}\"); // using replace so user cannot go back to prior page.";
            var order = await _orderService.GetOrderByOrderNumber(orderNumber);
            var associate = await _associateService.GetAssociate(order.AssociateId);
            // TODO: Check that associate Id in the URL is the same as the payment.
            var pendingOrderPayments = order.Payments.Where(x => x.IsPending == true).ToArray();
            // TODO: ADD check here and don't assume first payment
            var pendingOrderPayment = pendingOrderPayments[0];
            var paymentCurrency = pendingOrderPayment.CurrencyCode;
            var paymentAmount = string.Format("{0:N2}", Math.Round(pendingOrderPayment.Amount, 2));
            var braintreeTokenizatonKey = _braintreeSettingsService.TokenizationKey;
            var merchantAccountId = _braintreeSettingsService.MerchantAccountId(paymentCurrency);
            var fallbackUrl = "https://your-domain.com/page-to-complete-checkout"; // TODO: Is this correct?
            // TODO: Hide or Show what payment button options based on configurations.
            var html =
$@"
<!DOCTYPE html>

<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title></title>

    <!-- jQuery. -->
    <script src=""https://code.jquery.com/jquery-3.6.0.js"" integrity=""sha256-H+K7U5CnXl1h5ywQfKtSj8PCmoN9aaq30gDh27Xc0jk="" crossorigin=""anonymous""></script>

</head>
<body>
    <button id=""ideal-button"" onclick=""startLocalPayment('ideal')"">iDEAL</button>
    <button id=""bancontact-button"" onclick=""startLocalPayment('bancontact')"">Bancontact</button>
    <button id=""sofort-button"" onclick=""startLocalPayment('sofort')"">Sofort</button>
    <button id=""giropay-button"" onclick=""startLocalPayment('sofort')"">GiroPay</button>
    <!-- Load the client component. -->
    <script src=""https://js.braintreegateway.com/web/3.77.0/js/client.min.js""></script>

    <!-- Load the local payment component. -->
    <script src=""https://js.braintreegateway.com/web/3.77.0/js/local-payment.min.js""></script>
    <script>
        // ***** INITIALIZE THE COMPONENT ***** -START
        var localPaymentInstance;

        // Create a client.
        braintree.client.create({{
            authorization: '{braintreeTokenizatonKey}'
        }}, function (clientErr, clientInstance) {{

            // Stop if there was a problem creating the client.
            // This could happen if there is a network error or if the authorization
            // is invalid.
            if (clientErr) {{
                console.error('Error creating client:', clientErr);
                return;
            }}

            // Create a local payment component.
            braintree.localPayment.create({{
                client: clientInstance,
                merchantAccountId: merchantAccountId()
            }}, function (localPaymentErr, paymentInstance) {{

                // Stop if there was a problem creating local payment component.
                // This could happen if there was a network error or if it's incorrectly
                // configured.
                if (localPaymentErr) {{
                    console.error('Error creating local payment:', localPaymentErr);
                    return;
                }}

                localPaymentInstance = paymentInstance;
            }});
        }});
        // ***** INITIALIZE THE COMPONENT ***** -END

        function merchantAccountId() {{
            return (""{merchantAccountId}"");
        }}

        function startLocalPayment(type) {{
            localPaymentInstance.startPayment({{
                paymentType: type,
                paymentTypeCountryCode: '{order?.BillAddress?.CountryCode.ToUpper()}',
                amount: '{paymentAmount}',
                fallback: {{ // see Fallback section for details on these params
                    url: '{fallbackUrl}',
                    buttonText: 'Complete Payment'
                }},
                email: '{associate.EmailAddress}',
                currencyCode: '{paymentCurrency}',
                givenName: '{associate.LegalFirstName}', // *************** NOTE!!! Should it be legal name or display name?
                surname: '{associate.LegalLastName}',
                countryCode: '{order?.BillAddress.CountryCode.ToUpper()}', //  *************** NOTE!!! Should it be Bill address or ship address?
                address: {{
                    countryCode: '{order?.BillAddress.CountryCode.ToUpper()}', //  *************** NOTE!!! Should it be Bill address or ship address?
                }},
                onPaymentStart: function (data, start) {{
                    console.log('PaymentId to save:' + data.paymentId);
                    $.ajaxSetup({{
                        headers: {{
                            'Content-Type': ""application/json""
                        }}
                    }});
                    $.post(""SaveAuthorizationNumber"", JSON.stringify({{ orderNumber: {orderNumber}, AuthorizationNumber: data.paymentId }}), function () {{ }})
                        .done(function () {{
                            console.log('Saved PaymentId: ' + data.paymentId + ' successfully.');
                        }})
                        .fail(function (data) {{
                            console.log('Failed to save PaymentId: ' + data.paymentId);
                        }})
                    start(); // Call start to initiate the popup
                }}
            }}, function (startPaymentError, payload) {{
                if (startPaymentError) {{
                    if (startPaymentError.code === 'LOCAL_PAYMENT_POPUP_CLOSED') {{
                        console.error('Customer closed Local Payment popup.');
                    }} else {{
                        console.error('Error!', startPaymentError);
                        {failedReturnUrlJs}
                    }}
                }} else {{
                    // Send the nonce to your server to create a transaction
                    console.log('Created Nonce: ' + payload.nonce);
                    $.ajaxSetup({{
                        headers: {{
                            'Content-Type': ""application/json""
                        }}
                    }});
                    $.post(""CreateTransaction"", JSON.stringify({{ orderNumber: {orderNumber}, Nonce: payload.nonce, MerchantAccountId: '{merchantAccountId}' }}), function () {{ }})
                        .done(function () {{
                            console.log('Saved Nonce: ' + payload.nonce + ' successfully.');
                            {successReturnUrlJs}
                        }})
                        .fail(function (data) {{
                            console.log('Failed to save Nonce: ' + payload.nonce);
                            {failedReturnUrlJs}
                        }})
                }}
            }});
        }}
    </script>
</body>
</html>
";
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
            //await context.Response.SendFileAsync("C:\\Code\\DirectScale_Braintree_NET6\\DirectScaleBraintree\\Pages\\DirectScaleBraintreeRedirect.html");
        }
    }
}
