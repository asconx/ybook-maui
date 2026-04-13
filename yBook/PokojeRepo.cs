using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using yBook.Models;

namespace yBook.Helpers
{
    public class Pokoj
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        // Informacje z API
        public int? OrganizationId { get; set; }
        public string? DateModified { get; set; }
        public string? Day { get; set; }
        public int? CanArrive { get; set; }
        public int? CanDepart { get; set; }
        public int? ApiId { get; set; } // id z API
    }

    public static class PokojeRepo
    {
        public static List<Pokoj> Lista => new()
        {
            new() { Id = 68, Nazwa = "Ma³y pokój 1" },
            new() { Id = 69, Nazwa = "Pokój dwuosobowy typu Standard 2" },
            new() { Id = 70, Nazwa = "Pokój czteroosobowy typu Classic 3" },
            new() { Id = 71, Nazwa = "Pokój dwuosobowy typu Economy 4" },
            new() { Id = 72, Nazwa = "Pokój czteroosobowy typu Comfort 5" },
            new() { Id = 73, Nazwa = "Pokój Dwuosobowy typu Deluxe 6" },
            new() { Id = 74, Nazwa = "Pokój Dwuosobowy typu Deluxe 7" },
            new() { Id = 75, Nazwa = "Pokój Dwuosobowy typu Deluxe 8" },
            new() { Id = 76, Nazwa = "Pokój Dwuosobowy typu Deluxe 9" },
            new() { Id = 77, Nazwa = "Pokój Dwuosobowy typu Deluxe 10" },
            new() { Id = 78, Nazwa = "Pokój Dwuosobowy typu Deluxe 11" }
        };

        private static readonly HttpClient _httpClient = new();
        private const string ApiUrl = "https://api.ybook.pl/entity/arrivalDepartureAvailability";

        public static async Task<List<yBook.Models.ArrivalDepartureAvailability>> FetchArrivalDepartureAvailabilityAsync(string token, int year, int month)
        {
            var result = new List<yBook.Models.ArrivalDepartureAvailability>();
            try
            {
                var start = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
                var end = new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("yyyy-MM-dd");
                var url = $"{ApiUrl}?from={start}&to={end}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dto = JsonSerializer.Deserialize<yBook.Models.ArrivalDepartureAvailabilityResponse>(json, options);
                    if (dto?.Items != null)
                        result.AddRange(dto.Items);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PokojeRepo] FetchArrivalDepartureAvailabilityAsync error: {ex.Message}");
            }
            return result;
        }

        public static async Task<bool> SaveArrivalDepartureAvailabilityAsync(string token, List<yBook.Models.ArrivalDepartureAvailability> items)
        {
            try
            {
                var url = ApiUrl;
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonSerializer.Serialize(new { items });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PokojeRepo] SaveArrivalDepartureAvailabilityAsync error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> PostSingleAvailabilityAsync(string token, ArrivalDepartureAvailabilityPost item)
        {
            try
            {
                var url = ApiUrl;
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonSerializer.Serialize(new[] { item });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PokojeRepo] PostSingleAvailabilityAsync error: {ex.Message}");
                return false;
            }
        }
    }
}