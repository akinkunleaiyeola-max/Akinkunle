namespace ATMPOSONLINE.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }

        public decimal InterestRate { get; set; } = 0.1m; // 10%

        public decimal TotalRepayment { get; set; }

        public DateTime DateApplied { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected

        public string FullName { get; set; }
    }
}
