using Microsoft.AspNetCore.Mvc;

namespace ATMPOSONLINE.Controllers
{
    public class Transaction_Page : Controller
    {
        public IActionResult Transaction()
        {
            return View();
        }
           
    }
}
