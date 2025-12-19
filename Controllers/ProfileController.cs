using Microsoft.AspNetCore.Mvc;
using AI_Art_Gallery.Services;
using AI_Art_Gallery.Models;
using AI_Art_Gallery.Models.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AI_Art_Gallery.Controllers
{
    [Authorize] // BU SATIR ÇOK ÖNEMLİ: Sadece giriş yapanlar buraya girebilir!
    public class ProfileController : Controller
    {
        private readonly SpringApiClient _apiClient;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(SpringApiClient apiClient, ILogger<ProfileController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Giriş yapan kullanıcının token'ını al
            var token = HttpContext.Session.GetString("jwt");
            
            _logger.LogInformation("=== PROFILE INDEX ===");
            _logger.LogInformation("Token from session: {HasToken}", !string.IsNullOrEmpty(token));
            
            if (string.IsNullOrEmpty(token)) 
            {
                _logger.LogWarning("No token found in session, redirecting to login");
                TempData["ErrorMessage"] = "Lütfen giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // DEBUG: Session ve Claims içeriğini logla
                _logger.LogInformation("=== SESSION DATA ===");
                _logger.LogInformation("userId: {UserId}", HttpContext.Session.GetString("userId"));
                _logger.LogInformation("username: {Username}", HttpContext.Session.GetString("username"));
                _logger.LogInformation("email: {Email}", HttpContext.Session.GetString("email"));
                _logger.LogInformation("surname: {Surname}", HttpContext.Session.GetString("surname"));
                
                _logger.LogInformation("=== CLAIMS DATA ===");
                _logger.LogInformation("User.Identity.Name: {Name}", User.Identity?.Name);
                _logger.LogInformation("User.Identity.IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                // Önce session'dan kullanıcı bilgilerini al
                var userId = HttpContext.Session.GetString("userId");
                var username = HttpContext.Session.GetString("username");
                var email = HttpContext.Session.GetString("email");
                var surname = HttpContext.Session.GetString("surname");

                UserDTO user;

                if (!string.IsNullOrEmpty(username))
                {
                    // Session'dan kullanıcı bilgileri varsa kullan
                    _logger.LogInformation("Using user data from session: {Username}", username);
                    user = new UserDTO
                    {
                        Id = !string.IsNullOrEmpty(userId) ? int.Parse(userId) : 0,
                        Username = username,
                        Email = email ?? "",
                        Surname = surname ?? ""
                    };
                }
                else
                {
                    // Session'da bilgi yoksa Claims'den al
                    _logger.LogInformation("Session empty, trying Claims");
                    var claimsUsername = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
                    var claimsEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    var claimsSurname = User.FindFirst(ClaimTypes.Surname)?.Value;
                    var claimsId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(claimsUsername))
                    {
                        _logger.LogInformation("Using user data from claims: {Username}", claimsUsername);
                        user = new UserDTO
                        {
                            Id = !string.IsNullOrEmpty(claimsId) ? int.Parse(claimsId) : 0,
                            Username = claimsUsername,
                            Email = claimsEmail ?? "",
                            Surname = claimsSurname ?? ""
                        };
                    }
                    else
                    {
                        // Son çare: API'den al (404 hatası alabilir)
                        _logger.LogInformation("No session/claims data, fetching from API");
                        try
                        {
                            user = await _apiClient.GetCurrentUser(token);
                        }
                        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
                        {
                            _logger.LogError("GetCurrentUser returned 404 and no fallback data available");
                            
                            // Minimum bilgilerle devam et
                            user = new UserDTO
                            {
                                Id = 0,
                                Username = "Kullanıcı",
                                Email = "Bilinmiyor",
                                Surname = ""
                            };
                            
                            TempData["WarningMessage"] = "Kullanıcı bilgileriniz tam olarak yüklenemedi. Lütfen tekrar giriş yapın.";
                        }
                    }
                }
                
                _logger.LogInformation("Final user data: Username={Username}, Email={Email}", user.Username, user.Email);

                // 3. API'den kullanıcının yüklediği resimleri al
                _logger.LogInformation("Fetching user artworks");
                var myArtworks = await _apiClient.GetUserArtworks(token);

                // 4. Kullanıcı bilgisini View'a taşı
                ViewBag.User = user;
                
                _logger.LogInformation("Profile page loaded successfully for user: {Username}", user.Username);

                // 5. Resim listesini modele ver
                return View(myArtworks);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access to profile");
                TempData["ErrorMessage"] = "Oturumunuz sona erdi. Lütfen tekrar giriş yapın.";
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile page");
                TempData["ErrorMessage"] = "Profil bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Artwork");
            }
        }
    }
}