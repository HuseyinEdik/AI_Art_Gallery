using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Controllers
{
    public class MLController : Controller
    {
        private readonly HttpClient _httpClient;

        public MLController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Predict(string promptText)
        {
            var payload = new
            {
                text = promptText
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "http://localhost:3001/predict", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "API'ye ulaþýlamadý";
                return View("Index");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<PromptResult>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return View("Result", result);
        }
    }
}
