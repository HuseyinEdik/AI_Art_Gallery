using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AI_Art_Gallery.Services;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly SpringApiClient _api;
        private readonly ILogger<ArtworkController> _logger;

        public ArtworkController(SpringApiClient api, ILogger<ArtworkController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: Artwork/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Session.GetString("jwt");
                var artworks = await _api.GetAllArts(token);
                
                if (artworks == null || !artworks.Any())
                {
                    TempData["InfoMessage"] = "Henüz hiç eser paylaşılmamış. İlk paylaşımı sen yap!";
                }
                
                return View(artworks ?? new List<Artwork>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Eserler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return View(new List<Artwork>());
            }
        }

        // GET: Artwork/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("=== ARTWORK DETAILS REQUEST ===");
                _logger.LogInformation("Artwork ID: {Id}", id);
                
                // Cache busting için no-cache header ekle
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                
                var token = HttpContext.Session.GetString("jwt");
                _logger.LogInformation("Token from session: {HasToken}", !string.IsNullOrEmpty(token));
                
                var artwork = await _api.GetArtworkDetails(id, token);

                if (artwork == null)
                {
                    _logger.LogWarning("Artwork not found: ID={Id}", id);
                    TempData["ErrorMessage"] = "Eser bulunamadı.";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("Artwork loaded: ID={Id}, Title={Title}", artwork.Id, artwork.Title);
                _logger.LogInformation("=== ARTWORK OBJECT DETAILS ===");
                _logger.LogInformation("IsLikedByCurrentUser from API: {IsLiked}", artwork.IsLikedByCurrentUser);
                _logger.LogInformation("LikeCount from API: {Count}", artwork.LikeCount);
                _logger.LogInformation("CommentCount from API: {Count}", artwork.CommentCount);

                // Like bilgilerini ViewBag'e ekle (artık API'den sayı olarak geliyor)
                var likeCount = artwork.LikeCount;
                var isLikedByUser = artwork.IsLikedByCurrentUser;
                var commentCount = artwork.CommentCount;
                
                ViewBag.LikeCount = likeCount;
                ViewBag.CommentCount = commentCount;
                ViewBag.IsLiked = isLikedByUser;
                
                _logger.LogInformation("=== VIEW DATA ===");
                _logger.LogInformation("ViewBag.IsLiked set to: {IsLiked}", isLikedByUser);
                _logger.LogInformation("ViewBag.LikeCount set to: {Count}", likeCount);
                
                // Yorumları ayrı endpoint'ten çek (detaylı bilgi için)
                if (commentCount > 0 && !string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var comments = await _api.GetArtworkComments(id, token);
                        artwork.Comments = comments;
                        _logger.LogInformation("Comments fetched: {Count}", comments.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fetch comments for artwork {Id}", id);
                    }
                }

                // Kullanıcı bilgileri
                var currentUserId = HttpContext.Session.GetString("userId");
                ViewBag.CurrentUserId = currentUserId;
                ViewBag.IsOwner = !string.IsNullOrEmpty(currentUserId) && 
                                  artwork.AppUserId.ToString() == currentUserId;

                return View(artwork);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading artwork details: ID={Id}", id);
                TempData["ErrorMessage"] = "Eser detayları yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // GET: Artwork/Create
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Kategorileri API'den çek
            var categories = await _api.GetCategories();
            ViewBag.Categories = categories;
            return View();
        }

        // POST: Artwork/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)] // 10 MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        public async Task<IActionResult> Create(string title, string promptText, IFormFile imageFile, List<int> selectedCategoryIds)
        {
            var token = HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // DEBUG: Log kategorileri
            Console.WriteLine($"=== CREATE ARTWORK DEBUG ===");
            Console.WriteLine($"Title: {title}");
            Console.WriteLine($"PromptText: {promptText}");
            Console.WriteLine($"ImageFile: {imageFile?.FileName}");
            Console.WriteLine($"Selected Category IDs: {selectedCategoryIds?.Count ?? 0}");
            if (selectedCategoryIds != null && selectedCategoryIds.Any())
            {
                Console.WriteLine($"Category IDs: {string.Join(", ", selectedCategoryIds)}");
            }
            else
            {
                Console.WriteLine("NO CATEGORIES SELECTED!");
            }

            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    ModelState.AddModelError("", "Lütfen bir görsel dosyası seçin.");
                    var categories = await _api.GetCategories();
                    ViewBag.Categories = categories;
                    return View();
                }

                // Dosya boyutu kontrolü (10 MB)
                const long maxFileSize = 10 * 1024 * 1024;
                if (imageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("", $"Dosya boyutu çok büyük ({imageFile.Length / (1024.0 * 1024.0):F2} MB). Maksimum 10 MB olmalıdır.");
                    var categories = await _api.GetCategories();
                    ViewBag.Categories = categories;
                    return View();
                }

                Console.WriteLine($"File size: {imageFile.Length / (1024.0 * 1024.0):F2} MB");

                var artwork = await _api.CreateArtwork(title, promptText, imageFile, selectedCategoryIds ?? new List<int>(), token);

                TempData["SuccessMessage"] = "Eseriniz başarıyla paylaşıldı!";
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
                
                if (ex.Message.Contains("405"))
                {
                    ModelState.AddModelError("", "API endpoint'i POST metodunu kabul etmiyor. Spring Boot'ta @PostMapping kontrolü yapın.");
                }
                else if (ex.Message.Contains("413"))
                {
                    ModelState.AddModelError("", "Dosya boyutu çok büyük. Lütfen daha küçük bir dosya seçin (max 10 MB).");
                }
                else
                {
                    ModelState.AddModelError("", $"Eser paylaşılırken bir hata oluştu: {ex.Message}");
                }
                
                var categories = await _api.GetCategories();
                ViewBag.Categories = categories;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Eser paylaşılırken bir hata oluştu: " + ex.Message);
                var categories = await _api.GetCategories();
                ViewBag.Categories = categories;
                return View();
            }
        }

        // POST: Artwork/Delete/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _api.DeleteArtwork(id, token);
                TempData["SuccessMessage"] = "Eser başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Eser silinirken bir hata oluştu.";
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Artwork/ToggleLike/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var token = HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("ToggleLike attempt without token: ArtworkId={Id}", id);
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _logger.LogInformation("ToggleLike request: ArtworkId={Id}", id);
                
                var response = await _api.ToggleLike(id, token);
                
                _logger.LogInformation("ToggleLike successful: ArtworkId={Id}, IsLiked={IsLiked}, Count={Count}", 
                    id, response.IsLiked, response.LikeCount);
                
                TempData["SuccessMessage"] = response.IsLiked 
                    ? "Eseri beğendiniz! ❤️" 
                    : "Beğeni kaldırıldı.";
                
                return RedirectToAction("Details", new { id });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while toggling like: ArtworkId={Id}", id);
                TempData["ErrorMessage"] = "Beğeni işlemi başarısız oldu. Lütfen tekrar deneyin.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like: ArtworkId={Id}", id);
                TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Artwork/AddComment
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int artworkId, string content)
        {
            var token = HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _api.AddComment(artworkId, content, token);
                return RedirectToAction("Details", new { id = artworkId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Yorum eklenirken bir hata oluştu.";
                return RedirectToAction("Details", new { id = artworkId });
            }
        }

        // POST: Artwork/DeleteComment
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int artworkId)
        {
            var token = HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _api.DeleteComment(commentId, token);
                TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
                return RedirectToAction("Details", new { id = artworkId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Yorum silinirken bir hata oluştu.";
                return RedirectToAction("Details", new { id = artworkId });
            }
        }
    }
}
