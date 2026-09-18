namespace ARSoftware.Models
{
    public class Payment
    {
        public long Id { get; set; }
        public long PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public decimal Amount { get; set; }

        // Payment Type: Cash, Online, UPI, RTGS, NEFT, Cheque, Bank Transfer
        public string PaymentType { get; set; } = "Cash";

        // NEW: Bank Detail - Cash સિવાય જરૂરી
        public string BankName { get; set; } = ""; // SBI, HDFC, BOB, ICICI
        public string AccountNo { get; set; } = ""; // Account No / UPI ID
        public string TransactionId { get; set; } = ""; // UTR / RTGS No / NEFT No / Transaction ID
        public string ChequeNo { get; set; } = ""; // Cheque Number
        public string IFSC { get; set; } = ""; // IFSC Code

        public DateTime PaymentDate { get; set; } = DateTime.Now.Date;
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}