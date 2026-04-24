using ATMPOSONLINE.Data;
using ATMPOSONLINE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ATMPOSONLINE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly ATMPPKDBContext _context;


        public AdminController(SignInManager<Users> signInManager, UserManager<Users> userManager, ATMPPKDBContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this._context = context;
        }

        public IActionResult PendingLoans()
        {
            var loans = _context.Loans
                .Where(l => l.Status == "Pending")
                .OrderByDescending(l => l.DateApplied)
                .ToList();

            return View(loans);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveLoanConfirm(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
                return NotFound();

            return View(loan); // send loan to view
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
                return NotFound();

            if (loan.Status != "Pending")
                return RedirectToAction("LoanRequest");

            var user = userManager.Users
                .FirstOrDefault(u => u.AccountNumber == loan.AccountNumber);

            if (user == null)
                return NotFound();

            // 💰 CREDIT USER
            user.Balance += loan.Amount;
            await userManager.UpdateAsync(user);

            // ✅ UPDATE LOAN STATUS
            loan.Status = "Approved";

            // 🧾 SAVE TRANSACTION
            _context.Transactions.Add(new Transactions
            {
                AccountNumber = user.AccountNumber,
                FullName = user.FullName,
                Amount = loan.Amount,
                Type = "Loan Credit",
                Description = "Loan Approved",
                Date = DateTime.Now,
                Status = loan.Status
            });

            // 💾 SAVE EVERYTHING
            var rows = await _context.SaveChangesAsync();
            TempData["Debug"] = "Rows affected: " + rows;
            return RedirectToAction("LoanRequests");
        }
    }
}
