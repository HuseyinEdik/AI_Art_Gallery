using Microsoft.AspNetCore.Mvc;
using AI_Art_Gallery.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AI_Art_Gallery.Controllers
{
    [Authorize] // BU SATIR ÇOK ÖNEMLİ: Sadece giriş yapanlar buraya girebilir!
    public class ProfileController : Controller
    {
        private readonly SpringApiClient _apiClient;

        public ProfileController(SpringApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Giriş yapan kullanıcının token'ını al
            var token = HttpContext.Session.GetString("jwt");
            
            if (string.IsNullOrEmpty(token)) 
                return RedirectToAction("Login", "Auth");

            // 2. API'den kullanıcı bilgilerini al
            var user = await _apiClient.GetCurrentUser(token);

            // 3. API'den kullanıcının yüklediği resimleri al
            var myArtworks = await _apiClient.GetUserArtworks(token);

            // 4. Kullanıcı bilgisini View'a taşı
            ViewBag.User = user;

            // 5. Resim listesini modele ver
            return View(myArtworks);
        }
    }
}