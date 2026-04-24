using Microsoft.AspNetCore.Mvc;

namespace ATMPOSONLINE.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Fintech()
        {
            return View();
        }
        public IActionResult Consulting()
        {
            return View();
        }
    }
}
