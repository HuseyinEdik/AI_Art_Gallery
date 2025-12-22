using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AI_Art_Gallery.Services;
using AI_Art_Gallery.Models.ViewModels;
using System.Text;

namespace AI_Art_Gallery.Controllers
{
    [Authorize(Roles = "ROLE_ADMIN,Admin")] // Her iki formatý da destekle
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly SpringApiClient _api;

        public AdminController(ILogger<AdminController> logger, IWebHostEnvironment environment, SpringApiClient api)
        {
            _logger = logger;
            _environment = environment;
            _api = api;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin panel accessed by user: {Username}", User.Identity?.Name);
            
            try
            {
                var token = HttpContext.Session.GetString("jwt");
                
                // Ýstatistikleri API'den çek
                var artworks = await _api.GetAllArts(token);
                
                ViewBag.TotalArtworks = artworks?.Count ?? 0;
                ViewBag.TotalLikes = artworks?.Sum(a => a.LikeCount) ?? 0;
                ViewBag.TotalComments = artworks?.Sum(a => a.CommentCount) ?? 0;
                
                // Log dosyasý sayýsý
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs");
                var logFileCount = Directory.Exists(logDirectory) 
                    ? Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories).Length 
                    : 0;
                ViewBag.LogFileCount = logFileCount;
                
                // Son eklenen görseller (son 5)
                var recentArtworks = artworks?.OrderByDescending(a => a.CreatedAt).Take(5).ToList() ?? new List<Models.Artwork>();
                ViewBag.RecentArtworks = recentArtworks;
                
                // API saðlýk kontrolü
                ViewBag.ApiStatus = "Çalýþýyor";
                ViewBag.ApiUrl = "http://localhost:8080/api";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                ViewBag.TotalArtworks = 0;
                ViewBag.TotalLikes = 0;
                ViewBag.TotalComments = 0;
                ViewBag.LogFileCount = 0;
                ViewBag.ApiStatus = "Hata";
            }
            
            return View();
        }

        // GET: Admin/Logs
        public IActionResult Logs(int page = 1, int pageSize = 100)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs");
                var logFiles = new List<LogFileInfo>();

                if (Directory.Exists(logDirectory))
                {
                    var files = Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories)
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .ToList();

                    foreach (var fileInfo in files)
                    {
                        logFiles.Add(new LogFileInfo
                        {
                            FileName = fileInfo.Name,
                            FilePath = fileInfo.FullName,
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            SizeMB = fileInfo.Length / (1024.0 * 1024.0)
                        });
                    }
                }

                ViewBag.TotalLogFiles = logFiles.Count;
                return View(logFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading log files");
                TempData["ErrorMessage"] = "Log dosyalarý yüklenirken bir hata oluþtu.";
                return View(new List<LogFileInfo>());
            }
        }

        // GET: Admin/ViewLog
        public IActionResult ViewLog(string fileName, int lines = 500)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "Log dosyasý bulunamadý.";
                    return RedirectToAction("Logs");
                }

                // Dosyanýn son N satýrýný oku
                var logLines = ReadLastLines(filePath, lines);

                ViewBag.FileName = fileName;
                ViewBag.TotalLines = logLines.Count;
                ViewBag.RequestedLines = lines;

                return View(logLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading log file: {FileName}", fileName);
                TempData["ErrorMessage"] = "Log dosyasý okunurken bir hata oluþtu.";
                return RedirectToAction("Logs");
            }
        }

        // GET: Admin/DownloadLog
        public IActionResult DownloadLog(string fileName)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "Log dosyasý bulunamadý.";
                    return RedirectToAction("Logs");
                }

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading log file: {FileName}", fileName);
                TempData["ErrorMessage"] = "Log dosyasý indirilirken bir hata oluþtu.";
                return RedirectToAction("Logs");
            }
        }

        // POST: Admin/ClearLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearLog(string fileName)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.WriteAllText(filePath, string.Empty);
                    _logger.LogWarning("Log file cleared by admin: {FileName}, User: {Username}", fileName, User.Identity?.Name);
                    TempData["SuccessMessage"] = $"{fileName} dosyasý temizlendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Log dosyasý bulunamadý.";
                }

                return RedirectToAction("Logs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing log file: {FileName}", fileName);
                TempData["ErrorMessage"] = "Log dosyasý temizlenirken bir hata oluþtu.";
                return RedirectToAction("Logs");
            }
        }

        // POST: Admin/DeleteLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLog(string fileName)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogWarning("Log file deleted by admin: {FileName}, User: {Username}", fileName, User.Identity?.Name);
                    TempData["SuccessMessage"] = $"{fileName} dosyasý silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Log dosyasý bulunamadý.";
                }

                return RedirectToAction("Logs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting log file: {FileName}", fileName);
                TempData["ErrorMessage"] = "Log dosyasý silinirken bir hata oluþtu.";
                return RedirectToAction("Logs");
            }
        }

        // Helper: Dosyanýn son N satýrýný oku
        private List<string> ReadLastLines(string filePath, int lines)
        {
            var result = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                var allLines = new List<string>();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    allLines.Add(line);
                }

                // Son N satýrý al
                result = allLines.Skip(Math.Max(0, allLines.Count - lines)).ToList();
            }

            return result;
        }

        // GET: Admin/DatabaseViews - Database View'larýný göster
        public async Task<IActionResult> DatabaseViews()
        {
            try
            {
                var token = HttpContext.Session.GetString("jwt");
                
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Oturum süresi doldu. Lütfen tekrar giriþ yapýn.";
                    return RedirectToAction("Login", "Auth");
                }

                _logger.LogInformation("Loading database views dashboard");

                var model = new AdminDashboardViewModel
                {
                    CategoryStats = await _api.GetDatabaseView<CategoryStatViewModel>("vw_categorystats", token),
                    ActiveUsers = await _api.GetDatabaseView<ActiveUserViewModel>("vw_activeusers", token),
                    RecentUploads = await _api.GetDatabaseView<RecentUploadViewModel>("vw_recentuploads", token)
                };

                _logger.LogInformation("Database views loaded: Categories={CategoryCount}, Users={UserCount}, Uploads={UploadCount}",
                    model.CategoryStats.Count, model.ActiveUsers.Count, model.RecentUploads.Count);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading database views");
                TempData["ErrorMessage"] = "Database view'larý yüklenirken bir hata oluþtu.";
                return View(new AdminDashboardViewModel());
            }
        }

        // GET: Admin/DetailedArts - Detaylý eser listesi
        public async Task<IActionResult> DetailedArts()
        {
            try
            {
                var token = HttpContext.Session.GetString("jwt");
                
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var arts = await _api.GetDatabaseView<DetailedArtViewModel>("vw_detailedartlist", token);
                
                _logger.LogInformation("Loaded {Count} detailed arts", arts.Count);
                
                return View(arts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading detailed arts");
                TempData["ErrorMessage"] = "Eserler yüklenirken bir hata oluþtu.";
                return View(new List<DetailedArtViewModel>());
            }
        }

        // GET: Admin/SystemLogs - Sistem loglarý (database'den)
        public async Task<IActionResult> SystemLogs()
        {
            try
            {
                var token = HttpContext.Session.GetString("jwt");
                
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var logs = await _api.GetDatabaseView<LogSummaryViewModel>("vw_logsummary", token);
                
                _logger.LogInformation("Loaded {Count} system logs", logs.Count);
                
                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading system logs");
                TempData["ErrorMessage"] = "Loglar yüklenirken bir hata oluþtu.";
                return View(new List<LogSummaryViewModel>());
            }
        }
    }

    // Log dosyasý bilgisi için model
    public class LogFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public double SizeMB { get; set; }
    }
}
