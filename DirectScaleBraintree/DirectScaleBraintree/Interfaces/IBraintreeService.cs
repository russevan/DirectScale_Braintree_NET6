using DirectScaleBraintree.Models;

namespace DirectScaleBraintree.Interfaces
{
    public interface IBraintreeService
    {
        Task<ChargeResponse> ChargeAmount(ChargeData data);
        Task<RefundResponse> RefundTransaction(RefundTransaction refundData);
        Task<SavePaymentModel> SavePayment(SavePaymentModel request);
        Task<string> GetPayerIdOrCreate(int associateId);
        void DeletePayment(string payorId, string paymentMethodId);
        DirectScale.Disco.Extension.PaymentMethod[] GetCustomerPaymentMethods(int payorId, string CurrencyCode);
    }
}

