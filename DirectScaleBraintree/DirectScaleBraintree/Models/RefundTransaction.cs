namespace DirectScaleBraintree.Models
{
    public class RefundTransaction
    {
        public string Id { get; set; }

        public RefundData RefundData { get; set; }
    }

    public class RefundData
    {
        public string Currency { get; set; }

        public double Amount { get; set; }

        public double PartialAmount { get; set; }

    }
}

