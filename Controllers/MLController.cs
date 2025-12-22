using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class MLController : Controller
    {
        private readonly HttpClient _httpClient;

        public MLController()
        {
            _httpClient = new HttpClient();
        }

        // Sayfa
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // 🔥 FETCH BURAYA JSON GÖNDERİYOR
        [HttpPost]
        public async Task<IActionResult> Analyze([FromBody] PromptDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Prompt boş");
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "http://localhost:3001/api/analyze",
                    new { text = request.Text }
                );

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, "Node.js hata verdi");
                }

                var result = await response.Content.ReadFromJsonAsync<MlResultDto>();
                return Json(result);
            }
            catch
            {
                return StatusCode(500, "Node.js API'ye ulaşılamadı");
            }
        }
    }

    // 🔥 JSON MODEL
    public class PromptDto
    {
        public string Text { get; set; }
    }
}
