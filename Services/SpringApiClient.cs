using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
            
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("http://localhost:8080/api");
            }
        }

        private void AddAuthHeader(string token)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task LogRequest(string method, string endpoint, object? body = null)
        {
            _logger.LogInformation("=== API REQUEST ===");
            _logger.LogInformation("Method: {Method}", method);
            _logger.LogInformation("Endpoint: {Endpoint}", endpoint);
            var fullUrl = new Uri(_http.BaseAddress!, endpoint);
            _logger.LogInformation("Full URL: {Url}", fullUrl.ToString());
            
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true });
                _logger.LogInformation("Request Body: {Body}", json);
            }
        }

        private async Task LogResponse(HttpResponseMessage response, bool includeBody = false)
        {
            _logger.LogInformation("=== API RESPONSE ===");
            _logger.LogInformation("Status Code: {StatusCode} ({StatusCodeNumber})", response.StatusCode, (int)response.StatusCode);
            
            if (response.Content.Headers.ContentLength.HasValue)
            {
                var sizeKB = response.Content.Headers.ContentLength.Value / 1024.0;
                _logger.LogInformation("Response Size: {SizeKB:F2} KB", sizeKB);
            }
            
            // Sadece hata durumunda veya açýkça istendiðinde body'yi logla
            if (includeBody || !response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // ImageUrl içeren büyük JSON'larý kýsalt
                if (content.Length > 1000)
                {
                    _logger.LogInformation("Response Body: [Large response - {Length} chars]", content.Length);
                }
                else
                {
                    _logger.LogInformation("Response Body: {Body}", content);
                }
            }
        }

        // LOGIN WITH DETAILS
        public async Task<LoginResponseDTO> LoginWithDetails(string email, string password)
        {
            var dto = new AuthRequestDTO { Email = email, Password = password };

            _logger.LogInformation("=== LOGIN REQUEST ===");
            _logger.LogInformation("Email: {Email}", email);
            await LogRequest("POST", "auth/login", new { email, password = "***HIDDEN***" });

            var res = await _http.PostAsJsonAsync("auth/login", dto);
            await LogResponse(res, includeBody: true);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadFromJsonAsync<LoginResponseDTO>();
            
            if (json != null)
            {
                _logger.LogInformation("Login successful. User: {Username}", json.Username);
                _logger.LogInformation("Roles: {Roles}", string.Join(", ", json.Roles));
            }
            
            return json ?? new LoginResponseDTO();
        }

        public async Task<string> Login(string email, string password)
        {
            var loginResponse = await LoginWithDetails(email, password);
            return loginResponse.Token;
        }

        // REGISTER
        public async Task<MessageResponseDTO> Register(string username, string email, string surname, string password)
        {
            var dto = new RegisterRequestDTO
            {
                Username = username,
                Email = email,
                Surname = surname,
                Password = password
            };

            _logger.LogInformation("=== REGISTER REQUEST ===");
            _logger.LogInformation("Username: {Username}, Email: {Email}", username, email);
            await LogRequest("POST", "auth/register", new { username, email, surname, password = "***HIDDEN***" });

            var res = await _http.PostAsJsonAsync("auth/register", dto);
            await LogResponse(res, includeBody: true);
            
            if (!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                _logger.LogError("Register failed: {Error}", errorContent);
                throw new HttpRequestException($"Register failed: {res.StatusCode}");
            }

            // API plain text veya JSON döndürebilir
            var contentType = res.Content.Headers.ContentType?.MediaType;
            MessageResponseDTO response;
            
            if (contentType?.Contains("application/json") == true)
            {
                // JSON response
                response = await res.Content.ReadFromJsonAsync<MessageResponseDTO>() 
                    ?? new MessageResponseDTO { Message = "Kayýt baþarýlý" };
            }
            else
            {
                // Plain text response
                var message = await res.Content.ReadAsStringAsync();
                response = new MessageResponseDTO { Message = message };
                _logger.LogInformation("Register response (plain text): {Message}", message);
            }
            
            return response;
        }

        // GET ALL ARTS
        public async Task<List<Artwork>> GetAllArts(string? token = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                    AddAuthHeader(token);
                }

                _logger.LogInformation("=== GET ALL ARTS ===");
                await LogRequest("GET", "arts/public");

                var response = await _http.GetAsync("arts/public", HttpCompletionOption.ResponseHeadersRead);
                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);
                
                // Response boyutunu kontrol et
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    var sizeMB = response.Content.Headers.ContentLength.Value / (1024.0 * 1024.0);
                    _logger.LogInformation("Response size: {SizeMB:F2} MB", sizeMB);
                    
                    if (sizeMB > 5.0)
                    {
                        _logger.LogWarning("Large response detected ({SizeMB:F2} MB). Consider implementing pagination or thumbnail URLs.", sizeMB);
                    }
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("GetAllArts returned {StatusCode}: {Error}", response.StatusCode, errorContent);
                    return new List<Artwork>();
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null, // API'den gelen format olduðu gibi kullanýlsýn
                    DefaultBufferSize = 81920 // 80KB buffer için streaming
                };
                var list = await JsonSerializer.DeserializeAsync<List<Artwork>>(stream, options);
                
                _logger.LogInformation("Successfully fetched {Count} artworks", list?.Count ?? 0);
                return list ?? new List<Artwork>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching artworks");
                return new List<Artwork>();
            }
        }

        // GET CURRENT USER
        public async Task<UserDTO> GetCurrentUser(string token)
        {
            try
            {
                _logger.LogInformation("=== GET CURRENT USER ===");
                
                _http.DefaultRequestHeaders.Authorization = null;
                AddAuthHeader(token);
                
                await LogRequest("GET", "auth/me");
                var response = await _http.GetAsync("auth/me");
                await LogResponse(response, includeBody: true);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("GetCurrentUser failed: Status {StatusCode}, Content: {Error}", response.StatusCode, errorContent);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("Token geçersiz veya süresi dolmuþ.");
                    }
                    
                    response.EnsureSuccessStatusCode();
                }
                
                var user = await response.Content.ReadFromJsonAsync<UserDTO>();
                
                if (user != null)
                {
                    _logger.LogInformation("User fetched: ID={Id}, Username={Username}", user.Id, user.Username);
                }
                
                return user ?? new UserDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user");
                throw;
            }
        }

        // GET USER ARTWORKS
        public async Task<List<Artwork>> GetUserArtworks(string token)
        {
            AddAuthHeader(token);
            var artworks = await _http.GetFromJsonAsync<List<Artwork>>("arts/my-artworks");
            return artworks ?? new List<Artwork>();
        }

        // GET ARTWORK DETAILS
        public async Task<Artwork?> GetArtworkDetails(int id, string? token = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                    AddAuthHeader(token);
                }

                _logger.LogInformation("=== GET ARTWORK DETAILS ===");
                _logger.LogInformation("Artwork ID: {Id}", id);
                
                await LogRequest("GET", $"arts/{id}");
                var response = await _http.GetAsync($"arts/{id}");
                await LogResponse(response, includeBody: false);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("GetArtworkDetails failed: ID={Id}, Status={StatusCode}, Error={Error}", 
                        id, response.StatusCode, errorContent);
                    return null;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                
                // DEBUG: Ýlk 2000 karakteri logla (likes, comments, categories görmek için)
                var preview = responseBody.Length > 2000 ? responseBody.Substring(0, 2000) + "..." : responseBody;
                _logger.LogInformation("API Response Preview (2000 chars): {Preview}", preview);
                
                // Farklý JSON formatlarýný desteklemek için esnek deserialization
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null,
                    // Bilinmeyen property'leri yoksay
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    // Number'larý string'den parse et
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };
                
                var artwork = JsonSerializer.Deserialize<Artwork>(responseBody, options);
                
                if (artwork != null)
                {
                    _logger.LogInformation("Artwork fetched: ID={Id}, Title={Title}, LikeCount={LikeCount}, CommentCount={CommentCount}, IsLikedByCurrentUser={IsLiked}", 
                        artwork.Id, artwork.Title, artwork.LikeCount, artwork.CommentCount, artwork.IsLikedByCurrentUser);
                    _logger.LogInformation("AppUser: {HasUser}, Username={Username}, UserId={UserId}", 
                        artwork.AppUser != null, artwork.AppUser?.Username ?? "NULL", artwork.AppUserId);
                    _logger.LogInformation("Category: {HasCategory}, CategoryName={CategoryName}", 
                        artwork.Category != null, artwork.Category?.Name ?? "NULL");
                }
                
                return artwork;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching artwork {Id}", id);
                return null;
            }
        }

        // GET ARTWORK COMMENTS
        public async Task<List<Comment>> GetArtworkComments(int id, string token)
        {
            try
            {
                _http.DefaultRequestHeaders.Authorization = null;
                AddAuthHeader(token);
                
                _logger.LogInformation("=== FETCHING COMMENTS ===");
                _logger.LogInformation("Artwork ID: {Id}", id);
                
                var response = await _http.GetAsync($"arts/{id}/comments");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetArtworkComments failed: ID={Id}, Status={StatusCode}", id, response.StatusCode);
                    return new List<Comment>();
                }
                
                var responseBody = await response.Content.ReadAsStringAsync();
                
                // DEBUG: Ýlk 500 karakteri logla
                var preview = responseBody.Length > 500 ? responseBody.Substring(0, 500) + "..." : responseBody;
                _logger.LogInformation("Comments Response Preview: {Preview}", preview);
                
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null
                };
                
                var comments = System.Text.Json.JsonSerializer.Deserialize<List<Comment>>(responseBody, options);
                
                if (comments != null && comments.Any())
                {
                    _logger.LogInformation("Comments parsed: Count={Count}", comments.Count);
                    foreach (var comment in comments)
                    {
                        _logger.LogInformation("Comment ID={Id}, Content={Content}, AppUser={HasUser}, Username={Username}", 
                            comment.Id, 
                            comment.Content.Substring(0, Math.Min(20, comment.Content.Length)), 
                            comment.AppUser != null,
                            comment.AppUser?.Username ?? "NULL");
                    }
                }
                
                return comments ?? new List<Comment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments for artwork {Id}", id);
                return new List<Comment>();
            }
        }

        // GET CATEGORIES
        public async Task<List<Category>> GetCategories()
        {
            try
            {
                _logger.LogInformation("=== GET CATEGORIES ===");
                await LogRequest("GET", "categories");

                var response = await _http.GetAsync("categories");
                await LogResponse(response, includeBody: false);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetCategories failed: {StatusCode}", response.StatusCode);
                    return new List<Category>();
                }

                var categories = await response.Content.ReadFromJsonAsync<List<Category>>();
                _logger.LogInformation("Fetched {Count} categories", categories?.Count ?? 0);
                return categories ?? new List<Category>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return new List<Category>();
            }
        }

        // CREATE ARTWORK
        public async Task<Artwork> CreateArtwork(string title, string promptText, IFormFile imageFile, List<int> categoryIds, string token)
        {
            AddAuthHeader(token);

            _logger.LogInformation("=== CREATE ARTWORK ===");
            _logger.LogInformation("Title: {Title}", title);
            _logger.LogInformation("Image: {FileName} ({SizeMB} MB)", imageFile?.FileName, imageFile?.Length / (1024.0 * 1024.0));
            _logger.LogInformation("Categories: {CategoryIds}", string.Join(", ", categoryIds));

            const long maxSize = 10 * 1024 * 1024;
            if (imageFile != null && imageFile.Length > maxSize)
            {
                throw new InvalidOperationException($"File size ({imageFile.Length / (1024.0 * 1024.0):F2} MB) exceeds 10 MB");
            }

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(title), "title");
            formData.Add(new StringContent(promptText ?? ""), "promptText");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileContent = new StreamContent(imageFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                formData.Add(fileContent, "imageFile", imageFile.FileName);
            }

            foreach (var catId in categoryIds)
            {
                formData.Add(new StringContent(catId.ToString()), "categoryIds");
            }

            try
            {
                _logger.LogInformation("Sending POST to arts/create...");
                var res = await _http.PostAsync("arts/create", formData);
                await LogResponse(res, includeBody: false);
                
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    _logger.LogError("Create failed: Status {StatusCode}, Content: {Error}", res.StatusCode, errorContent);
                }
                
                res.EnsureSuccessStatusCode();

                var artwork = await res.Content.ReadFromJsonAsync<Artwork>();
                _logger.LogInformation("Artwork created: ID {Id}", artwork?.Id);
                return artwork ?? new Artwork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating artwork");
                throw;
            }
        }

        // DELETE ARTWORK
        public async Task DeleteArtwork(int id, string token)
        {
            AddAuthHeader(token);
            var res = await _http.DeleteAsync($"arts/{id}");
            res.EnsureSuccessStatusCode();
        }

        // TOGGLE LIKE
        public async Task<InteractionResponse> ToggleLike(int id, string token)
        {
            try
            {
                _logger.LogInformation("=== TOGGLE LIKE REQUEST ===");
                _logger.LogInformation("Artwork ID: {Id}", id);
                
                _http.DefaultRequestHeaders.Authorization = null;
                AddAuthHeader(token);
                
                await LogRequest("POST", $"interactions/like/{id}");
                var res = await _http.PostAsync($"interactions/like/{id}", null);
                await LogResponse(res, includeBody: true);
                
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    _logger.LogError("ToggleLike failed: ID={Id}, Status={StatusCode}, Error={Error}", 
                        id, res.StatusCode, errorContent);
                }
                
                res.EnsureSuccessStatusCode();

                var response = await res.Content.ReadFromJsonAsync<InteractionResponse>();
                
                if (response != null)
                {
                    _logger.LogInformation("Like toggled successfully: IsLiked={IsLiked}, Count={Count}", 
                        response.IsLiked, response.LikeCount);
                }
                
                return response ?? new InteractionResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for artwork {Id}", id);
                throw;
            }
        }

        // ADD COMMENT
        public async Task<CommentDTO> AddComment(int artworkId, string content, string token)
        {
            AddAuthHeader(token);

            var dto = new CommentRequest { Content = content };
            var res = await _http.PostAsJsonAsync($"interactions/comment/{artworkId}", dto);
            res.EnsureSuccessStatusCode();

            var comment = await res.Content.ReadFromJsonAsync<CommentDTO>();
            return comment ?? new CommentDTO();
        }

        // DELETE COMMENT
        public async Task DeleteComment(int commentId, string token)
        {
            AddAuthHeader(token);
            var res = await _http.DeleteAsync($"interactions/comment/{commentId}");
            res.EnsureSuccessStatusCode();
        }

        // LOGOUT
        public async Task Logout(string token)
        {
            AddAuthHeader(token);
            await _http.PostAsync("auth/logout", null);
        }

        // ADMIN - GET DATABASE VIEWS
        public async Task<List<T>> GetDatabaseView<T>(string viewName, string token)
        {
            try
            {
                AddAuthHeader(token);
                
                _logger.LogInformation("=== GET DATABASE VIEW ===");
                _logger.LogInformation("View Name: {ViewName}", viewName);

                var response = await _http.GetAsync($"/api/admin/views/{viewName}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetDatabaseView failed: View={ViewName}, Status={StatusCode}", 
                        viewName, response.StatusCode);
                    return new List<T>();
                }
                
                var data = await response.Content.ReadFromJsonAsync<List<T>>();
                _logger.LogInformation("View data fetched: {Count} records", data?.Count ?? 0);
                
                return data ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching database view: {ViewName}", viewName);
                return new List<T>();
            }
        }
    }
}
