namespace ATMPOSONLINE.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class DepositViewModel
    {
        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string Description { get; set; }

       [Required(ErrorMessage = "PIN is required")]
       [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN must be 4 digits")]
       [DataType(DataType.Password)]
        public string ATMPIN { get; set; }

 //   public string Reference { get; set; }

        public string FullName { get; set; }
       }
}
