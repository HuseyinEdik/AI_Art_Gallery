using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Home sayfasýndan direkt Artwork/Index'e yönlendir
            return RedirectToAction("Index", "Artwork");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
