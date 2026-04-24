using ATMPOSONLINE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ATMPOSONLINE.Data
{
    public class ATMPPKDBContext : IdentityDbContext<Users>
    {
        public ATMPPKDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .Property(u => u.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transactions>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);
           
            modelBuilder.Entity<Loan>()
           .Property(l => l.Amount)
           .HasPrecision(18, 2);

            modelBuilder.Entity<Loan>()
            .Property(l => l.InterestRate)
            .HasPrecision(5, 4); // e.g. 0.1000 = 10%

            modelBuilder.Entity<Loan>()
            .Property(l => l.TotalRepayment)
            .HasPrecision(18, 2);
        }
                       
       
    }


}

