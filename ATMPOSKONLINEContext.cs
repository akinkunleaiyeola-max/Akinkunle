using ATMPOSONLINE.Models;
using Microsoft.EntityFrameworkCore;

namespace ATMPOSONLINE.Data
{
    public class ATMPOSKONLINEContext : DbContext
    {
        public ATMPOSKONLINEContext(DbContextOptions<ATMPOSKONLINEContext> options) : base(options)
        {
        }
        public DbSet<ATMONLINE> ATMONLINE { get; set; }                
    }
}
