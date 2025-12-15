using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using AI_Art_Gallery.Services;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class AuthController : Controller
    {
        private readonly SpringApiClient _api;

        public AuthController(SpringApiClient api)
        {
            _api = api;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                string token = await _api.Login(email, password);

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Giriş başarısız!";
                    return View();
                }

                // Token'ı session'a kaydet
                HttpContext.Session.SetString("jwt", token);

                // Cookie tabanlı kimlik doğrulama
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.NameIdentifier, email),
                    new Claim("jwt", token)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Artwork");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Giriş başarısız! Kullanıcı adı veya şifre hatalı.";
                return View();
            }
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            try
            {
                var response = await _api.Register(username, email, password);

                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Kayıt başarısız! Bu kullanıcı adı veya email zaten kullanılıyor.";
                return View();
            }
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("jwt");
            
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    await _api.Logout(token);
                }
                catch { }
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Artwork");
        }

        // GET: ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewBag.Message = "Şifre sıfırlama özelliği yakında eklenecek.";
            return View();
        }
    }
}
