using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AI_Art_Gallery.Data;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // KAYIT OL SAYFASI
        public IActionResult Register()
        {
            return View();
        }

        // KAYIT OL İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> Register(AppUser user)
        {
            // Basit kayıt işlemi
            if (ModelState.IsValid)
            {
                // Email kontrolü (Aynı mail var mı?)
                if (await _context.AppUsers.AnyAsync(u => u.Email == user.Email))
                {
                    ViewBag.Error = "Bu email zaten kayıtlı.";
                    return View(user);
                }

                user.Role = "User"; // Varsayılan rol
                user.CreatedAt = DateTime.UtcNow;

                _context.AppUsers.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GİRİŞ YAP SAYFASI
        public IActionResult Login()
        {
            return View();
        }

        // GİRİŞ YAP İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Kullanıcıyı bul
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // KİMLİK KARTI OLUŞTURMA (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),           // Kullanıcı Adı
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID'si (Bunu resim yüklerken kullanacağız)
                    new Claim(ClaimTypes.Role, user.Role)                // Rolü
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                // Sisteme giriş yap (Cookie oluştur)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Artwork");
            }

            ViewBag.Error = "Email veya şifre hatalı!";
            return View();
        }

        // ÇIKIŞ YAP
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        // Sadece tasarımları görmek için geçici metodlar:
        public IActionResult ForgotPassword() { return View(); }
        public IActionResult VerifyCode() { return View(); }
        public IActionResult ResetPassword() { return View(); }
    }
}