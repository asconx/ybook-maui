using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using yBook.Models.Api;

namespace yBook.Services.Api
{
    /// <summary>
    /// Prosty klient API do pobierania pokoi z https://api.ybook.pl/room
    /// Zwraca listę obiektów Api.Room
    /// </summary>
    public class RoomApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoomApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Asynchronicznie pobiera listę pokoi z API.
        /// </summary>
        /// <param name="bearerToken">Token w formacie "TOKEN" (bez słowa Bearer)</param>
        /// <returns>Lista pokoi (List&lt;Room&gt;). W przypadku błędu zwraca pustą listę.</returns>
        public async Task<List<Room>> GetRoomsAsync(string bearerToken)
        {
            var result = new List<Room>();

            try
            {
                // przygotowanie żądania
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/room");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Authorization: Bearer <TOKEN>
                if (!string.IsNullOrWhiteSpace(bearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                }

                // wykonanie żądania
                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    // logowanie błędnej odpowiedzi
                    System.Diagnostics.Debug.WriteLine($"RoomApiService: nieudane żądanie. Status: {(int)response.StatusCode} - {response.ReasonPhrase}");
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(content))
                {
                    System.Diagnostics.Debug.WriteLine("RoomApiService: odpowiedź była pusta.");
                    return result;
                }

                // deserializacja odpowiedzi do modelu RoomsResponse
                var roomsResp = JsonSerializer.Deserialize<RoomsResponse>(content, _jsonOptions);
                if (roomsResp?.Items != null)
                {
                    result = roomsResp.Items;
                }

                return result;
            }
            catch (Exception ex)
            {
                // obsługa wyjątków i logowanie
                System.Diagnostics.Debug.WriteLine($"RoomApiService: wyjątek podczas pobierania pokoi: {ex}");
                return result;
            }
        }
    }
}
