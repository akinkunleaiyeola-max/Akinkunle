using System.ComponentModel.DataAnnotations;

namespace ATMPOSONLINE.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [StringLength(40, MinimumLength = 10, ErrorMessage = "The {0} must be at {2} and at max {1} character")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("ConfirmNewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Password Confirmation is Required")]
        [DataType(DataType.Password)]
       // [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; }
    }
}
