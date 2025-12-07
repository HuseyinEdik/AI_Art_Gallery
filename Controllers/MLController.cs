using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AI_Art_Gallery.Controllers
{
    public class MLController : Controller
    {
        // İstersen sadece giriş yapanlar görsün diye [Authorize] ekleyebilirsin
        public IActionResult Index()
        {
            return View();
        }
    }
}