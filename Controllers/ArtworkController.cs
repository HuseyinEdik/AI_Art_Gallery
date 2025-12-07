using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_Art_Gallery.Data;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly AppDbContext _context;

        public ArtworkController(AppDbContext context)
        {
            _context = context;
        }

        // 1. READ (Listeleme - Anasayfa)
        public async Task<IActionResult> Index()
        {
            var artworks = await _context.Artworks
                .Include(a => a.AppUser)  // <--- BU SATIR İSMİ GETİRİR (Artık Anonim yazmaz)
                .Include(a => a.Likes)    // <--- BU SATIR BEĞENİ SAYISINI GETİRİR
                .Include(a => a.Categories)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(artworks);
        }

        public IActionResult Create()
        {
            // Tüm kategorileri listeye (Multi-Select) koymak için gönderiyoruz
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artwork artwork, IFormFile imageFile, int[] selectedCategoryIds)
        {
            // 1. Kullanıcı Giriş Kontrolü (Opsiyonel: İstersen açık bırakabilirsin)
            // if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");

            // 2. Dosya Kaydetme
            if (imageFile != null)
            {
                var extension = Path.GetExtension(imageFile.FileName);
                var newImageName = Guid.NewGuid() + extension;
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using (var stream = new FileStream(Path.Combine(folderPath, newImageName), FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                artwork.ImageUrl = "/images/" + newImageName;
            }

            // 3. Kullanıcı ID Atama (Eğer giriş yapmışsa)
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null) artwork.AppUserId = int.Parse(userId);
            }

            if (selectedCategoryIds != null && selectedCategoryIds.Length > 0)
            {
                // Seçilen ID'lere sahip kategorileri veritabanından bulup resme ekliyoruz
                artwork.Categories = _context.Categories
                    .Where(c => selectedCategoryIds.Contains(c.Id))
                    .ToList();
            }

            // --- HATALARI TEMİZLE (Kritik Kısım) ---
            ModelState.Remove("ImageUrl");
            ModelState.Remove("AppUser");
            ModelState.Remove("AppUserId");
            ModelState.Remove("Comments");
            ModelState.Remove("Likes");

            if (ModelState.IsValid)
            {
                _context.Add(artwork);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Görsel yüklendi.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(artwork);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var artwork = await _context.Artworks
                .Include(a => a.AppUser)       // <--- SAHİBİNİ GETİRİR
                .Include(a => a.Likes)         // <--- BEĞENİLERİ GETİRİR
                .Include(a => a.Comments).ThenInclude(c => c.AppUser) // <--- YORUMLARI GETİRİR
                .Include(a => a.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (artwork == null) return NotFound();

            // Beğeni kontrolü
            bool isLiked = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    isLiked = artwork.Likes.Any(l => l.AppUserId == int.Parse(userId));
                }
            }

            ViewBag.IsLiked = isLiked;
            ViewBag.LikeCount = artwork.Likes.Count;

            return View(artwork);
        }
        // --- GÖNDERİ SİLME (Gelişmiş) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Resmi bul (İlişkileriyle beraber)
            var artwork = await _context.Artworks
                .Include(a => a.Comments) // Yorumlarını getir
                .Include(a => a.Likes)    // Beğenilerini getir
                .FirstOrDefaultAsync(m => m.Id == id);

            if (artwork == null) return NotFound();

            // 2. Güvenlik Kontrolü: Silen kişi Sahibi mi veya Admin mi?
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") && artwork.AppUserId.ToString() != userId)
            {
                return Forbid(); // Yetkisiz işlem
            }

            // 3. ÖNCE BAĞLI VERİLERİ SİL (Hata almamak için)
            if (artwork.Comments.Any()) _context.Comments.RemoveRange(artwork.Comments);
            if (artwork.Likes.Any()) _context.Likes.RemoveRange(artwork.Likes);

            // 4. SONRA RESMİ SİL
            _context.Artworks.Remove(artwork);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Görsel ve tüm verileri silindi.";
            return RedirectToAction(nameof(Index));
        }

        // --- YORUM SİLME (YENİ) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return NotFound();

            // Güvenlik: Yorumu sadece Yazan Kişi veya Admin silebilir
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("Admin") || comment.AppUserId.ToString() == userId)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum silindi.";
            }

            // Detay sayfasına geri dön
            return RedirectToAction("Details", new { id = comment.ArtworkId });
        }

        // 1. BEĞENİ İŞLEMİ (Toggle: Varsa siler, yoksa ekler)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.ArtworkId == id && l.AppUserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike); // Beğeniyi Kaldır
            }
            else
            {
                _context.Likes.Add(new Like { ArtworkId = id, AppUserId = userId }); // Beğeni Ekle
            }

            await _context.SaveChangesAsync();

            // Detay sayfasına geri dön ama olduğu yerde kalsın
            return RedirectToAction("Details", new { id = id });
        }

        // 2. YORUM YAPMA İŞLEMİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int artworkId, string content)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");

            if (!string.IsNullOrWhiteSpace(content))
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                var comment = new Comment
                {
                    ArtworkId = artworkId,
                    AppUserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorumunuz eklendi!";
            }

            return RedirectToAction("Details", new { id = artworkId });
        }
    }
}