using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AI_Art_Gallery.Services;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly SpringApiClient _api;

        public ArtworkController(SpringApiClient api)
        {
            _api = api;
        }

        // GET: Artwork/Index
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("jwt");
            var artworks = await _api.GetAllArts(token);
            return View(artworks);
        }

        // GET: Artwork/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var token = HttpContext.Session.GetString("jwt");
            var artwork = await _api.GetArtworkDetails(id, token);

            if (artwork == null)
            {
                return NotFound();
            }

            // Like bilgilerini ViewBag'e ekle
            ViewBag.LikeCount = artwork.Likes?.Count ?? 0;
            ViewBag.IsLiked = false; // API'den gelecek

            return View(artwork);
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
        public async Task<IActionResult> Create(string title, string promptText, IFormFile imageFile, List<int> selectedCategoryIds)
        {
            var token = HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
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

                var artwork = await _api.CreateArtwork(title, promptText, imageFile, selectedCategoryIds ?? new List<int>(), token);

                TempData["SuccessMessage"] = "Eseriniz başarıyla paylaşıldı!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
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
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _api.ToggleLike(id, token);
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Beğeni işlemi başarısız oldu.";
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
