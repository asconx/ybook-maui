using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using yBook.Models.Api;
using yBook.ViewModels;

namespace yBook.Services.Api
{
    public class ActiveReservationService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly Services.IAuthService _auth;

        public ActiveReservationService(HttpClient http, Services.IAuthService auth)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        public async Task<(ReceptionViewModel? Data, string? Error)> GetActiveReservationsAsync()
        {
            try
            {
                var token = await _auth.GetTokenAsync();

                using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/activeReservation");
                req.Headers.Accept.Clear();
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(token))
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await _http.SendAsync(req).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    return (null, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                }

                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Empty response");

                var dto = JsonSerializer.Deserialize<ActiveReservationsResponse>(content, _jsonOptions);
                if (dto == null) return (null, "Failed to deserialize response");

                var model = MapToReceptionViewModel(dto);
                return (model, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ActiveReservationService.GetActiveReservationsAsync error: {ex}");
                return (null, ex.Message);
            }
        }

        ReceptionViewModel MapToReceptionViewModel(ActiveReservationsResponse dto)
        {
            var result = new ReceptionViewModel { Today = dto.Today ?? string.Empty };
            DateTime.TryParse(dto.Today, out var todayDt);

            if (dto.Items != null)
            {
                foreach (var it in dto.Items)
                {
                    var res = it.Reservation;
                    var room = it.Room;
                    var client = it.Client;
                    if (res == null) continue;

                    DateTime.TryParse(res.DateFrom ?? string.Empty, out var df);
                    DateTime.TryParse(res.DateTo ?? string.Empty, out var dt);

                    var item = new ReceptionItemViewModel
                    {
                        Id = res.Id,
                        RoomId = room?.Id ?? 0,
                        RoomName = room?.Name ?? string.Empty,
                        RoomShortName = room?.ShortName ?? string.Empty,
                        ClientName = client?.Name ?? string.Empty,
                        ClientPhone = client?.Phone ?? string.Empty,
                        DateFrom = df,
                        DateTo = dt,
                        IsCheckedIn = res.IsCheckedIn,
                        IsPaid = res.IsPaid,
                        IsUntilToday = res.IsUntilToday,
                        StatusId = res.StatusId
                    };

                    item.IsActive = todayDt >= item.DateFrom && todayDt <= item.DateTo;
                    item.IsEndingToday = item.IsUntilToday;
                    item.IsStartingToday = item.DateFrom.Date == todayDt.Date;

                    result.Reservations.Add(item);
                }

                // sorting: active first, then by dateTo ascending
                var sorted = new List<ReceptionItemViewModel>(result.Reservations);
                sorted.Sort((a, b) =>
                {
                    if (a.IsActive && !b.IsActive) return -1;
                    if (!a.IsActive && b.IsActive) return 1;
                    return DateTime.Compare(a.DateTo, b.DateTo);
                });

                result.Reservations.Clear();
                foreach (var s in sorted) result.Reservations.Add(s);
            }

            return result;
        }
    }
}
