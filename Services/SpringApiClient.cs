using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using AI_Art_Gallery.Models;
using AI_Art_Gallery.Models.DTO;

namespace AI_Art_Gallery.Services
{
    public class SpringApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<SpringApiClient> _logger;

        public SpringApiClient(HttpClient http, ILogger<SpringApiClient> logger)
        {
            _http = http;
            _logger = logger;
            
            // Base address HttpClient factory'den gelecek
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("http://localhost:8080");
            }
        }

        private void AddAuthHeader(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // LOGIN
        public async Task<string> Login(string username, string password)
        {
            var dto = new AuthRequestDTO
            {
                Username = username,
                Password = password
            };

            var res = await _http.PostAsJsonAsync("/api/auth/login", dto);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadFromJsonAsync<LoginResponseDTO>();
            return json?.Token ?? "";
        }

        // GETALLARTS
        public async Task<List<Artwork>> GetAllArts(string? token = null)
        {
            if (!string.IsNullOrWhiteSpace(token))
                AddAuthHeader(token);

            var list = await _http.GetFromJsonAsync<List<Artwork>>("/api/arts/public");
            return list ?? new List<Artwork>();
        }

        // LIKEART
        public async Task LikeArt(long id, string token)
        {
            AddAuthHeader(token);
            var res = await _http.PostAsync($"/api/interactions/like/{id}", null);
            res.EnsureSuccessStatusCode();
        }

        // COMMENTART
        public async Task CommentArt(long id, string content, string token)
        {
            AddAuthHeader(token);

            var body = new { content };
            var res = await _http.PostAsJsonAsync($"/api/interactions/comment/{id}", body);
            res.EnsureSuccessStatusCode();
        }

        // GET CURRENT USER
        public async Task<UserDTO> GetCurrentUser(string token)
        {
            AddAuthHeader(token);
            var user = await _http.GetFromJsonAsync<UserDTO>("/api/auth/me");
            return user ?? new UserDTO();
        }

        // GET USER ARTWORKS
        public async Task<List<Artwork>> GetUserArtworks(string token)
        {
            AddAuthHeader(token);
            var artworks = await _http.GetFromJsonAsync<List<Artwork>>("/api/arts/my-artworks");
            return artworks ?? new List<Artwork>();
        }

        // REGISTER
        public async Task<MessageResponseDTO> Register(string username, string email, string password)
        {
            var dto = new AuthRequestDTO
            {
                Username = username,
                Email = email,
                Password = password
            };

            var res = await _http.PostAsJsonAsync("/api/auth/register", dto);
            res.EnsureSuccessStatusCode();

            var response = await res.Content.ReadFromJsonAsync<MessageResponseDTO>();
            return response ?? new MessageResponseDTO { Message = "Kayıt başarılı" };
        }

        // GET ARTWORK DETAILS
        public async Task<Artwork?> GetArtworkDetails(int id, string? token = null)
        {
            if (!string.IsNullOrWhiteSpace(token))
                AddAuthHeader(token);

            var artwork = await _http.GetFromJsonAsync<Artwork>($"/api/arts/{id}");
            return artwork;
        }

        // CREATE ARTWORK
        public async Task<Artwork> CreateArtwork(string title, string promptText, IFormFile imageFile, List<int> categoryIds, string token)
        {
            AddAuthHeader(token);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(title), "title");
            formData.Add(new StringContent(promptText), "promptText");

            // Görsel dosyasını ekle
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileContent = new StreamContent(imageFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                formData.Add(fileContent, "imageFile", imageFile.FileName);
            }

            // Kategorileri ekle
            foreach (var catId in categoryIds)
            {
                formData.Add(new StringContent(catId.ToString()), "categoryIds");
            }

            var res = await _http.PostAsync("/api/arts/create", formData);
            res.EnsureSuccessStatusCode();

            var artwork = await res.Content.ReadFromJsonAsync<Artwork>();
            return artwork ?? new Artwork();
        }

        // DELETE ARTWORK
        public async Task DeleteArtwork(int id, string token)
        {
            AddAuthHeader(token);
            var res = await _http.DeleteAsync($"/api/arts/{id}");
            res.EnsureSuccessStatusCode();
        }

        // TOGGLE LIKE
        public async Task<InteractionResponse> ToggleLike(int id, string token)
        {
            AddAuthHeader(token);
            var res = await _http.PostAsync($"/api/interactions/like/{id}", null);
            res.EnsureSuccessStatusCode();

            var response = await res.Content.ReadFromJsonAsync<InteractionResponse>();
            return response ?? new InteractionResponse();
        }

        // ADD COMMENT
        public async Task<CommentDTO> AddComment(int artworkId, string content, string token)
        {
            AddAuthHeader(token);

            var dto = new CommentRequest { Content = content };
            var res = await _http.PostAsJsonAsync($"/api/interactions/comment/{artworkId}", dto);
            res.EnsureSuccessStatusCode();

            var comment = await res.Content.ReadFromJsonAsync<CommentDTO>();
            return comment ?? new CommentDTO();
        }

        // DELETE COMMENT
        public async Task DeleteComment(int commentId, string token)
        {
            AddAuthHeader(token);
            var res = await _http.DeleteAsync($"/api/interactions/comment/{commentId}");
            res.EnsureSuccessStatusCode();
        }

        // GET CATEGORIES
        public async Task<List<Category>> GetCategories()
        {
            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/categories");
            return categories ?? new List<Category>();
        }

        // LOGOUT
        public async Task Logout(string token)
        {
            AddAuthHeader(token);
            await _http.PostAsync("/api/auth/logout", null);
        }
    }
}
