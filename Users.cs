using Microsoft.AspNetCore.Identity;

namespace ATMPOSONLINE.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public string ATMPIN {  get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; } = 0;
        public string Status {  get; set; }
        public string Reference { get; set; }

    }
}
