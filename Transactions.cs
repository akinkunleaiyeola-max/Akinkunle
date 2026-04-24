namespace ATMPOSONLINE.Models
{
    public class Transactions
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; } 
        public string FullName { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Type { get; set; } // Deposit / Withdrawal / transfer / Loan
        public string Status { get; set; }
        public string Reference { get; set; }
    }
}
