using DirectScaleBraintree.Models.Merchants.Braintree;
using Microsoft.AspNetCore.Mvc;

namespace DirectScaleBraintree.Controllers.Merchants
{
    public class AddCardFrame : Controller
    {
        public AddCardFrame()
        {
        }

        [Route("Merchants/AddCardFrame/Braintree")]
        public async Task<IActionResult> Braintree(string payorId, int merchantId)
        {
            BraintreeSettings btSettings = new(merchantId);

            var addCardFrameBraintree = new AddCardFrameViewModel
            {
                BraintreeTokenizationKey = btSettings.TokenizationKey,
                BraintreeMerchantAccountId = btSettings.BraintreeMerchantAccountId,
                BaseUrl = Environment.GetEnvironmentVariable("ExtensionBaseURL"),
                DiscoMerchantId = merchantId,
                EnableDropInApplePay = btSettings.EnableDropInApplePay,
                EnableDropInGooglePay = btSettings.EnableDropInGooglePay,
                EnableDropInPayPal = btSettings.EnableDropInPayPal,
                EnableDropInPayPalCredit = btSettings.EnableDropInPayPalCredit,
                EnableDropInVenmo = btSettings.EnableDropInVenmo,
                GooglePayMerchantId = btSettings.GooglePayMerchantId,
                IsLive = btSettings.IsLive,
                PayorId = payorId
            };


            return View("../Merchants/Braintree/AddCardFrame", addCardFrameBraintree);
        }
    }
}
