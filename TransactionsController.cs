using ATMPOSONLINE.Data;
using ATMPOSONLINE.Models;
using ATMPOSONLINE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATMPOSONLINE.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly ATMPPKDBContext _context;
        private string GenerateReference()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }


        public TransactionsController(SignInManager<Users> signInManager, UserManager<Users> userManager, ATMPPKDBContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this._context = context;
        }
        public async Task<IActionResult> History()
        {
            //the profile of logged-in user
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //used JavaScript to find transactions for this user
            var transactions = _context.Transactions
                .Where(t => t.AccountNumber == user.AccountNumber)
                .OrderByDescending(t => t.Date)
                .ToList();

            return View(transactions);
        }
        public async Task<IActionResult> Deposit()
        {
            
            var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    TempData["Balance"] = user.Balance.ToString("N2"); 
                }

                return View();
        }
        [HttpGet]
        public JsonResult GetAccountName(string accountNumber)
        {
            var user = userManager.Users
                .FirstOrDefault(u => u.AccountNumber == accountNumber);
            if (user == null)
            {
                return Json(null);
            }
            return Json(user.FullName);
        }              
        public IActionResult DepositSummary()
        {
            var model = new DepositViewModel
            {
                AccountNumber = TempData["AccountNumber"]?.ToString(),
                FullName = TempData["FullName"]?.ToString(),
                Amount = Convert.ToDecimal(TempData["Amount"]), //want it to convert back in decimal place
                Description = TempData["Description"]?.ToString(),
                ATMPIN = TempData["ATMPIN"]?.ToString(),
          //    Reference = GenerateReference(),
            };            
            TempData.Keep();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {          
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Store temporarily (Session or TempData)
            TempData["AccountNumber"] = model.AccountNumber;
            TempData["FullName"] = model.FullName;
            TempData["Amount"] = model.Amount.ToString();
            TempData["Description"] = model.Description;
            TempData["Reference"] = GenerateReference();
            TempData["ATMPIN"] = model.ATMPIN;
          
            //Go to Summary page
             return RedirectToAction("DepositSummary");
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmDeposit()
        {
            var depositor = await userManager.GetUserAsync(User);
            var accountNumber = TempData["AccountNumber"]?.ToString();
            var fullName = TempData["FullName"]?.ToString();
            var amount = Convert.ToDecimal(TempData["Amount"]);
            var description = TempData["Description"]?.ToString();
            var reference = GenerateReference();
            var pin = TempData["ATMPIN"]?.ToString();

            //retrieve the user
            var user = userManager.Users
            .FirstOrDefault(u => u.AccountNumber == accountNumber);

            if (user == null)
            {
                TempData["Error"] = "Account not found";
                return RedirectToAction("Deposit");
            }
            bool isValidPin = BCrypt.Net.BCrypt.Verify(pin, user.ATMPIN);
            if (!isValidPin)
            {
                TempData["Error"] = "Invalid PIN";
                return RedirectToAction("Deposit");
            }
            // the user Updated balance
            user.Balance += amount;
            await userManager.UpdateAsync(user);
            
            // transaction storage
            var transaction = new Transactions
            {
                AccountNumber = user.AccountNumber,
                FullName = depositor.FullName,
                Amount = amount,
                Description = description +  " Deposit from "  + depositor.FullName,
                Type = "Deposit",
                Date = DateTime.Now,
                Status = "Approved",
                Reference = GenerateReference(),
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            string Reference = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            //PASS DATA TO RECEIPT
            TempData["Name"] = user.FullName;
            TempData["Account"] = user.AccountNumber;
            TempData["Amount"] = transaction.Amount.ToString("N2");
            //TempData["Balance"] = user.Balance.ToString("N2");
            TempData["Description"] = transaction.Description;
            TempData["Date"] = DateTime.Now.ToString("g");
            TempData["Reference"] = reference;

            return RedirectToAction("Receipt");

        }
        public async Task<IActionResult> Transfer()
        {            
                var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    TempData["Balance"] = user.Balance.ToString("N2"); 
                }

                return View();            
        }
        [HttpGet]
        public JsonResult GetFullAccountName(string accountNumber)
        {
            var user = userManager.Users
             .FirstOrDefault(u => u.AccountNumber == accountNumber);

            if (user == null)
            {
                return Json(null);
            }

            return Json(user.FullName);
        }
       [HttpPost]
        public async Task<IActionResult> Transfer(TransferViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var sender = await userManager.GetUserAsync(User);
            if (sender == null)
            {
                ModelState.AddModelError("", "User not authenticated");
                return View(model);
            }
            var receiver = userManager.Users
                .FirstOrDefault(u => u.AccountNumber == model.ReceiverAccountNumber);
            if (receiver == null)
            {
                ModelState.AddModelError("ReceiverAccountNumber", "Receiver not found");
                return View(model);
            }
            if (sender.AccountNumber == receiver.AccountNumber)
            {
                ModelState.AddModelError("", "Cannot transfer to yourself");
                return View(model);
            }
            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Invalid amount");
                return View(model);
            }
            if (!BCrypt.Net.BCrypt.Verify(model.ATMPIN, sender.ATMPIN))
            {
                ModelState.AddModelError("ATMPIN", "Invalid PIN");
                return View(model);
            }

            if (sender.Balance < model.Amount)
            {
                ModelState.AddModelError("Amount", "Insufficient balance");
                return View(model);
            }
            // I store temporarily in the session before confirmation
          TempData["SenderAccount"] = sender.AccountNumber;
            TempData["SenderName"] = sender.FullName;
            TempData["ReceiverAccount"] = receiver.AccountNumber;
            TempData["ReceiverName"] = receiver.FullName;
            TempData["Amount"] = model.Amount.ToString();
            TempData["Description"] = model.Description;
            TempData["Reference"] = GenerateReference();

            return RedirectToAction("ConfirmTransfer");
        }
        public IActionResult ConfirmTransfer()
        {
            var model = new TransferViewModel
            {
     //         SenderAccountNumber = TempData["SenderAccount"]?.ToString(),                 
                FullName = TempData["ReceiverName"]?.ToString(),
                ReceiverAccountNumber = TempData["ReceiverAccount"]?.ToString(),
                Amount = Convert.ToDecimal(TempData["Amount"]),
                Description = TempData["Description"]?.ToString(),
        //      Reference = GenerateReference()
            };

            TempData.Keep(); 
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmTransferSave()
        {
            var senderAccount = TempData["SenderAccount"]?.ToString();
            var receiverAccount = TempData["ReceiverAccount"]?.ToString();
            var amount = Convert.ToDecimal(TempData["Amount"]);
            var description = TempData["Description"]?.ToString();
            var date = DateTime.Now.ToString();
            var reference = TempData["Reference"]?.ToString();

            var sender = userManager.Users.FirstOrDefault(u => u.AccountNumber == senderAccount);
            var receiver = userManager.Users.FirstOrDefault(u => u.AccountNumber == receiverAccount);
            if (sender == null || receiver == null)
                return RedirectToAction("Transfer");
            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    sender.Balance -= amount;
                    receiver.Balance += amount;

                    await userManager.UpdateAsync(sender);
                    await userManager.UpdateAsync(receiver);

                    _context.Transactions.Add(new Transactions
                    {
                        AccountNumber = sender.AccountNumber,
                        Amount = amount,
                        Type = "Transfer-Out",
                        FullName = sender.FullName,
                        Description = "Transfer to " + receiver.FullName,
                        Date = DateTime.Now,
                        Status = "Approved",
                        Reference = GenerateReference()
                    });
                    _context.Transactions.Add(new Transactions
                    {
                        AccountNumber = receiver.AccountNumber,
                        Amount = amount,
                        FullName = receiver.FullName,
                        Type = "Transfer-In",
                        Description = "From " + sender.FullName,
                        Date = DateTime.Now,
                        Status = "Approved",
                        Reference = GenerateReference()
                    });
                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    TempData["SuccessMessage"] = "Transfer Successful!";
                    TempData["Balance"] = sender.Balance.ToString("N2");

                    return RedirectToAction("Receipt");
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    TempData["Error"] = "Transfer failed";
                    return RedirectToAction("Transfer");
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> Withdraw()
        {
            var user = await userManager.GetUserAsync(User);

            if (user != null)
            {
                TempData["Balance"] = user.Balance.ToString("N2");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Withdraw(WithdrawViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // PIN Verification
            bool isValidPin = BCrypt.Net.BCrypt.Verify(model.ATMPIN, user.ATMPIN);

            if (!isValidPin)
            {
                ModelState.AddModelError("ATMPIN", "Invalid PIN");
                return View(model);
            }
            // Checking user's balance
            if (user.Balance < model.Amount)
            {
                ModelState.AddModelError("Amount", "Insufficient balance");
                return View(model);
            }
            if (model.Amount > 70000)
            {
                ModelState.AddModelError("Amount", "Max withdrawal is ₦70,000");
                return View(model);
            }

            // Making an immediate deduction before commitment
            user.Balance -= model.Amount;
            await userManager.UpdateAsync(user);
        
            // I save the TRANSACTION
            var transaction = new Transactions
            {
                AccountNumber = user.AccountNumber,
                FullName = user.FullName,
                Amount = model.Amount,
                Type = "Withdraw",
                Description = model.Description,
                Date = DateTime.Now,
                Status = "Approved",
                Reference = GenerateReference()
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Withdrawal successful!";
            TempData["Balance"] = user.Balance.ToString("N2");          

            //PASS DATA TO RECEIPT
            TempData["Name"] = user.FullName;
            TempData["Account"] = user.AccountNumber;
            TempData["Amount"] = model.Amount.ToString("N2");
            //TempData["Balance"] = user.Balance.ToString("N2");
            TempData["Date"] = DateTime.Now.ToString("g");
            TempData["Reference"] = GenerateReference();

            return RedirectToAction("Receipt");
           
        }
        public IActionResult Receipt()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ApplyLoan()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ApplyLoan(LoanViewModel model)
        {
                    if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // I will Calculate loan details here
            decimal interestRate = 0.1m;
            decimal totalRepayment = model.Amount + (model.Amount * interestRate);

            // loan confirmation page (NOT saving yet)
            var confirmModel = new LoanViewModel
            {
                Amount = model.Amount,
                InterestRate = interestRate,
                TotalRepayment = totalRepayment,
                FullName = user.FullName,
                AccountNumber = user.AccountNumber
            };

            return View("ConfirmLoan", confirmModel);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmLoan(LoanViewModel model)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // 💾 Save loan now
            var loan = new Loan
            {
                AccountNumber = user.AccountNumber,
                FullName = user.FullName,
                Amount = model.Amount,
                InterestRate = model.InterestRate,
                TotalRepayment = model.TotalRepayment,
                DateApplied = DateTime.Now,
                Status = "Pending"
            };
           // var rows = await _context.SaveChangesAsync();
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Loan request submitted successfully!";
            return RedirectToAction("ApplyLoan");
        }
       
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

            // the user gets credited with loan amount approved
            user.Balance += loan.Amount;
            await userManager.UpdateAsync(user);

            // I hidded the loan status here
            loan.Status = "Approved";

            //point of saving TRANSACTION
            _context.Transactions.Add(new Transactions
            {
                AccountNumber = user.AccountNumber,
                FullName= user.FullName,
                Amount = loan.Amount,
                Type = "Loan Credit",
                Description = "Loan Approved",
                Date = DateTime.Now
            });

            // All my savings in one place
            var rows = await _context.SaveChangesAsync();
            TempData["Debug"] = "Rows affected: " + rows;
            return RedirectToAction("PendingLoans");
        }
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
                return NotFound();

            loan.Status = "Rejected";

            await _context.SaveChangesAsync();

            return RedirectToAction("LoanRequests");
        }
        [HttpPost]  
        [Authorize(Roles = "Admin")]
        public IActionResult LoanRequests()
        {
            var loans = _context.Loans
                .OrderByDescending(l => l.DateApplied)
                .ToList();

            return View(loans);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveTransaction(int id)
        {
            var t = await _context.Transactions.FindAsync(id);

            if (t == null)
                return NotFound();

            if (t.Status != "Pending")
                return RedirectToAction("PendingTransactions");

            var user = userManager.Users
                .FirstOrDefault(u => u.AccountNumber == t.AccountNumber);

            if (user == null)
                return NotFound();

            //I inserted my logic of when Admin needs to approve some level of amount
            switch (t.Type)
            {
                case "Deposit":
                    user.Balance += t.Amount;
                    break;

                case "Withdraw":
                    if (user.Balance < t.Amount)
                    {
                        TempData["Error"] = "Insufficient balance";
                        return RedirectToAction("PendingTransactions");
                    }
                    user.Balance -= t.Amount;
                    break;

                case "Transfer":
                    var receiverAccount = t.Description.Replace("To ", "");
                    var receiver = userManager.Users.FirstOrDefault(u => u.AccountNumber == receiverAccount);

                    if (receiver != null && user.Balance >= t.Amount)
                    {
                        user.Balance -= t.Amount;
                        receiver.Balance += t.Amount;
                        await userManager.UpdateAsync(receiver);
                    }
                    break;
            }
            // balance is viewed
            await userManager.UpdateAsync(user);

            // the status of approval viewed by the Admin
            t.Status = "Approved";

            // save/ commit transaction into the DB table
            await _context.SaveChangesAsync();

            return RedirectToAction("PendingTransactions");
        }
    }
}
