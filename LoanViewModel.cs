using System.ComponentModel.DataAnnotations;

namespace ATMPOSONLINE.ViewModels
{
    public class LoanViewModel
    {
        
        [Required]
        public decimal Amount { get; set; }
        public string FullName { get; set; }
        public string AccountNumber { get; set; }
        public decimal InterestRate { get; set; }
        public decimal TotalRepayment { get; set; }
    }
}
