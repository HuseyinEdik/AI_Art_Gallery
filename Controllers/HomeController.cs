using Microsoft.AspNetCore.Mvc;
using AI_Art_Gallery.Models;
using AI_Art_Gallery.Data; // Veritabaný için gerekli

namespace AI_Art_Gallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor: Veritabaný baðlantýsýný alýyoruz
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}