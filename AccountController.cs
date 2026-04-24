using ATMPOSONLINE.Models;
using ATMPOSONLINE.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ATMPOSONLINE.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private string GenerateReference()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }
        private string GenerateAccountNumber()
        {
            Random random = new Random();
            string accountNumber;
            do
            {
                accountNumber = "60" + random.Next(10000000, 19999999).ToString();
            }
            while (userManager.Users.Any(u => u.AccountNumber == accountNumber));

            return accountNumber;
        }
        public async Task<IActionResult> MakeAdmin()
        {
            var user = await userManager.FindByEmailAsync("kunle3000@yahoo.com");

            if (user == null)
                return Content("User not found");

            await userManager.AddToRoleAsync(user, "Admin");

            return Content("User is now Admin");
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);

                    // ✅ Check role
                    if (await userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("PendingLoans", "Admin");
                    }

                    // 👤 Normal user
                    return RedirectToAction("Transaction", "Transaction_Page");
                }
                else
                {
                    ModelState.AddModelError("", "Email or Password is incorrect");
                    return View(model);
                }
            }
            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string accountNumber = GenerateAccountNumber();
                string hashedPin = BCrypt.Net.BCrypt.HashPassword(model.ATMPIN);

                Users users = new Users()
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    ATMPIN = hashedPin,
                    AccountNumber = accountNumber,
                    Reference = GenerateReference(),
                    Status = "Created"
                };

                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded)
                {
                    /*//Assign selected role
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await userManager.AddToRoleAsync(users, model.Role);
                    }
                    else
                    {
                        // fallback
                        await userManager.AddToRoleAsync(users, "User");
                    }*/

                    TempData["AccountNumber"] = accountNumber;

                    return RedirectToAction("RegisterSuccess", "RegisterSuccess");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
     public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Wrong Email Address, Please check again!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                }
            }
            return View(model);
        }
        public IActionResult ChangePassword(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = UserName });
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var users = await userManager.FindByEmailAsync(model.Email);
                if (users != null)
                {
                    var result = await userManager.RemovePasswordAsync(users);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(users, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "emailAddress not found!");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong, try again");
                return View(model);
            }

        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}

