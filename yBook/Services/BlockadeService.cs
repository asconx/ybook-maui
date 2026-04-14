using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Models;

namespace yBook.Services
{
    public interface IBlockadeService
    {
        Task<List<BlockadeDto>> FetchBlockadesAsync(string token);
        Task<List<BlockadeRoomDto>> FetchBlockadeRoomsAsync(string token);
        Task<BlockadeDto> CreateBlockadeAsync(string token, BlockadeDto blockade);
        Task<BlockadeDto> UpdateBlockadeAsync(string token, int id, BlockadeDto blockade);
        Task<bool> DeleteBlockadeAsync(string token, int id); // 🔥 DODANE
    }

    public class BlockadeService : IBlockadeService
    {
        private static readonly HttpClient _httpClient = new();
        private const string ApiBaseUrl = "https://api.ybook.pl";
        private const string BlockadesEndpoint = "/blockade";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // =========================
        // FETCH BLOCKADES
        // =========================
        public async Task<List<BlockadeDto>> FetchBlockadesAsync(string token)
        {
            var url = $"{ApiBaseUrl}{BlockadesEndpoint}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine("=== FETCH BLOCKADES ===");
            System.Diagnostics.Debug.WriteLine(json);

            if (!response.IsSuccessStatusCode)
                return new List<BlockadeDto>();

            return JsonSerializer.Deserialize<List<BlockadeDto>>(json, JsonOptions) ?? new();
        }

        // =========================
        // FETCH ROOMS (from blockades)
        // =========================
        public async Task<List<BlockadeRoomDto>> FetchBlockadeRoomsAsync(string token)
        {
            var blockades = await FetchBlockadesAsync(token);

            var rooms = blockades
                .Where(b => b.Rooms != null)
                .SelectMany(b => b.Rooms)
                .GroupBy(r => r.RoomId)
                .Select(g => g.First())
                .ToList();

            return rooms;
        }

        // =========================
        // CREATE BLOCKADE
        // =========================
        public async Task<BlockadeDto> CreateBlockadeAsync(string token, BlockadeDto blockade)
        {
            var url = $"{ApiBaseUrl}{BlockadesEndpoint}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(blockade, JsonOptions);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine("========== CREATE REQUEST ==========");
            System.Diagnostics.Debug.WriteLine(json);
            System.Diagnostics.Debug.WriteLine("====================================");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine("========== CREATE RESPONSE ==========");
            System.Diagnostics.Debug.WriteLine($"STATUS: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine(responseBody);
            System.Diagnostics.Debug.WriteLine("====================================");

            if (!response.IsSuccessStatusCode)
                return null;

            try
            {
                return JsonSerializer.Deserialize<List<BlockadeDto>>(responseBody, JsonOptions)?.FirstOrDefault();
            }
            catch
            {
                return JsonSerializer.Deserialize<BlockadeDto>(responseBody, JsonOptions);
            }
        }

        // =========================
        // UPDATE BLOCKADE
        // =========================
        public async Task<BlockadeDto> UpdateBlockadeAsync(string token, int id, BlockadeDto blockade)
        {
            var url = $"{ApiBaseUrl}{BlockadesEndpoint}/{id}";

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(blockade, JsonOptions);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine("========== UPDATE REQUEST ==========");
            System.Diagnostics.Debug.WriteLine(json);
            System.Diagnostics.Debug.WriteLine("====================================");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine("========== UPDATE RESPONSE ==========");
            System.Diagnostics.Debug.WriteLine($"STATUS: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine(responseBody);
            System.Diagnostics.Debug.WriteLine("====================================");

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonSerializer.Deserialize<BlockadeDto>(responseBody, JsonOptions);
        }

        // =========================
        // DELETE BLOCKADE
        // =========================
        public async Task<bool> DeleteBlockadeAsync(string token, int id)
        {
            try
            {
                var url = $"{ApiBaseUrl}{BlockadesEndpoint}/{id}";

                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                System.Diagnostics.Debug.WriteLine("========== DELETE REQUEST ==========");
                System.Diagnostics.Debug.WriteLine($"DELETE {url}");
                System.Diagnostics.Debug.WriteLine("====================================");

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine("========== DELETE RESPONSE ==========");
                System.Diagnostics.Debug.WriteLine($"STATUS: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine(responseBody);
                System.Diagnostics.Debug.WriteLine("====================================");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DELETE ERROR: {ex.Message}");
                return false;
            }
        }
    }
}