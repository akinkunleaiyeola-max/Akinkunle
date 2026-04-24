using System.ComponentModel.DataAnnotations;

namespace ATMPOSONLINE.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [StringLength(40, MinimumLength = 10, ErrorMessage = "The {0} must be at {2} and at max {1} character long")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("ConfirmPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "PIN is required")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN must be 4 digits")]
        [DataType(DataType.Password)]
        [Display(Name = "Create your ATMPIN")]
        public string ATMPIN { get; set; }
        //public string Role {  get; set; }

       
    }
}

