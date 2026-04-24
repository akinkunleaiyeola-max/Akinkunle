using System.ComponentModel.DataAnnotations;


namespace ATMPOSONLINE.Models
{
    public class ATMONLINE
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "PIN is required")]
        [MaxLength(4, ErrorMessage = "PIN must be 4 digits")]
        [RegularExpression("^[0-9]{4}$", ErrorMessage = "PIN must be numeric")]
        public int ATMPIN { get; set; }
        public int CorrectPin         { get; set; } 
        public double Available_Ballance { get;set; }
       //public string AccountNumber { get; set; }
        public double Current_Account  { get; set; } 
        public double TransferLocally  { get; set; }
        public double Savings_Ballance { get; set; }
        public double Amount_Withdrawn { get; set; }
        public double Current_Ballance { get; set; }
        public int Deposit { get; set; }
        public DateTime DateTime_Withdrawn { get; set; } = DateTime.Now;
    }
}
