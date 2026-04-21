using yBook.Models;
using System.Diagnostics;

namespace yBook.Services
{
    public interface ILogiService
    {
        Task<(List<LogAkcji> Items, int Total)> GetLogiAsync(int start = 0, int limit = 50);
    }

    public class LogiService : ILogiService
    {
        private readonly ApiClient _api;

        public LogiService(IAuthService authService)
        {
            _api = new ApiClient(authService);
        }

        public async Task<(List<LogAkcji> Items, int Total)> GetLogiAsync(int start = 0, int limit = 50)
        {
            try
            {
                var response = await _api.GetAsync<LogiResponse>($"/log?start={start}&itemId=");
                var items = response?.Items?.Select(d => d.ToModel()).ToList() ?? new();
                return (items, response?.Total ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LogiService] Błąd: {ex.Message}");
                return (new(), 0);
            }
        }
    }
}