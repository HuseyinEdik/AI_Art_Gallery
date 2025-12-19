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
                // API'den token ve kullanıcı bilgilerini al
                var loginResponse = await _api.LoginWithDetails(email, password);

                if (string.IsNullOrEmpty(loginResponse.Token))
                {
                    ViewBag.Error = "Giriş başarısız!";
                    return View();
                }

                // Token'ı session'a kaydet
                HttpContext.Session.SetString("jwt", loginResponse.Token);
                HttpContext.Session.SetString("userId", loginResponse.Id.ToString());
                HttpContext.Session.SetString("username", loginResponse.Username);
                HttpContext.Session.SetString("email", loginResponse.Email);
                HttpContext.Session.SetString("surname", loginResponse.Surname);

                // Cookie tabanlı kimlik doğrulama
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResponse.Id.ToString()),
                    new Claim(ClaimTypes.Name, loginResponse.Username),
                    new Claim(ClaimTypes.Email, loginResponse.Email),
                    new Claim(ClaimTypes.Surname, loginResponse.Surname),
                    new Claim("jwt", loginResponse.Token)
                };

                // Rolleri ekle
                foreach (var role in loginResponse.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

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

                TempData["SuccessMessage"] = $"Hoş geldin, {loginResponse.Username}!";
                return RedirectToAction("Index", "Artwork");
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = "Giriş başarısız! Email veya şifre hatalı.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Bir hata oluştu. Lütfen tekrar deneyin.";
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
        public async Task<IActionResult> Register(string username, string email, string surname, string password)
        {
            try
            {
                var response = await _api.Register(username, email, surname, password);

                TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Kayıt başarısız! Bu kullanıcı adı veya email zaten kullanılıyor.";
                return View();
            }
        }

        /* EMAIL DOĞRULAMA KALDIRILDI - İHTİYAÇ OLURSA AKTİF EDİLEBİLİR
        
        // GET: VerifyEmail
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            if (TempData["RegisteredEmail"] == null)
            {
                return RedirectToAction("Register");
            }

            ViewBag.Email = TempData["RegisteredEmail"]?.ToString();
            TempData.Keep("RegisteredEmail");
            return View();
        }

        // POST: VerifyEmail
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(string email, string verificationCode)
        {
            try
            {
                var response = await _api.VerifyEmail(email, verificationCode);

                TempData["SuccessMessage"] = "Email doğrulandı! Artık giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Doğrulama kodu hatalı veya süresi dolmuş!";
                ViewBag.Email = email;
                TempData["RegisteredEmail"] = email;
                return View();
            }
        }

        // POST: ResendVerificationCode
        [HttpPost]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            try
            {
                await _api.ResendVerificationCode(email);
                TempData["SuccessMessage"] = "Doğrulama kodu tekrar gönderildi!";
                TempData["RegisteredEmail"] = email;
                return RedirectToAction("VerifyEmail");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kod gönderilemedi. Lütfen tekrar deneyin.";
                TempData["RegisteredEmail"] = email;
                return RedirectToAction("VerifyEmail");
            }
        }
        
        */

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

            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Login");
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
