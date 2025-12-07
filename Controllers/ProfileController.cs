using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_Art_Gallery.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AI_Art_Gallery.Controllers
{
    [Authorize] // BU SATIR ÇOK ÖNEMLİ: Sadece giriş yapanlar buraya girebilir!
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Giriş yapan kullanıcının ID'sini Çerezden (Cookie) al
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) return RedirectToAction("Login", "Auth");

            // 2. Kullanıcının bilgilerini bul
            var user = await _context.AppUsers.FindAsync(int.Parse(userId));

            // 3. Kullanıcının yüklediği resimleri bul
            var myArtworks = await _context.Artworks
                .Where(a => a.AppUserId == int.Parse(userId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // 4. Kullanıcı bilgisini View'a taşı (ViewBag ile pratikçe)
            ViewBag.User = user;

            // 5. Resim listesini modele ver
            return View(myArtworks);
        }
    }
}