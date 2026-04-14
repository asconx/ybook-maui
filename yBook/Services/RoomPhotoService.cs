using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Models;

namespace yBook.Services;

public interface IRoomPhotoService
{
    Task<List<RoomPhoto>> GetRoomPhotosAsync();
    Task<List<RoomPhoto>> GetRoomPhotosByRoomIdAsync(int roomId);
    Task<RoomPhoto?> GetFirstRoomPhotoByRoomIdAsync(int roomId);
    Task<string?> GetPhotoFileUrlAsync(int fileId);
    Task<IReadOnlyList<string>> GetPhotoFileRequestUrlsAsync(int fileId, int? organizationId = null);
}

public class RoomPhotoService : IRoomPhotoService
{
    private const string BaseUrl = "https://api.ybook.pl";
    private const string RoomPhotoEndpoint = $"{BaseUrl}/entity/roomPhoto";
    private const string FileEndpoint = $"{BaseUrl}/entity/file";

    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private List<RoomPhoto>? _cachedPhotos;

    public RoomPhotoService(IAuthService authService, HttpClient httpClient)
    {
        _authService = authService;
        _httpClient = httpClient;
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Brak tokenu autoryzacyjnego");
        return token;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public async Task<List<RoomPhoto>> GetRoomPhotosAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ==================== GET ROOM PHOTOS START ====================");

            if (_cachedPhotos != null)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Returning cached photos ({_cachedPhotos.Count} total)");
                return _cachedPhotos;
            }

            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] Cache empty, fetching from API...");

            if (!await _authService.IsAuthenticatedAsync())
            {
                System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ✗ User is not authenticated");
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");
            }

            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ✓ User authenticated");

            var token = await GetAuthTokenAsync();
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Token retrieved (length: {token.Length})");

            var request = CreateRequest(HttpMethod.Get, RoomPhotoEndpoint, token);
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Sending request to: {RoomPhotoEndpoint}");

            var response = await _httpClient.SendAsync(request);
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✗ Server error: {response.StatusCode}");
                throw new HttpRequestException($"Błąd serwera: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Response received ({json.Length} bytes)");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Response preview: {json.Substring(0, Math.Min(300, json.Length))}...");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            List<RoomPhoto>? photos = null;

            if (json.StartsWith("["))
            {
                System.Diagnostics.Debug.WriteLine("[RoomPhotoService] Parsing as direct array...");
                photos = JsonSerializer.Deserialize<List<RoomPhoto>>(json, options);
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Array parsed: {photos?.Count ?? 0} items");
            }
            else if (json.StartsWith("{"))
            {
                System.Diagnostics.Debug.WriteLine("[RoomPhotoService] Parsing as wrapped object...");
                var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);

                if (wrapper.TryGetProperty("data", out var data))
                {
                    System.Diagnostics.Debug.WriteLine("[RoomPhotoService] Found 'data' property");
                    photos = JsonSerializer.Deserialize<List<RoomPhoto>>(data.GetRawText(), options);
                }
                else if (wrapper.TryGetProperty("items", out var items))
                {
                    System.Diagnostics.Debug.WriteLine("[RoomPhotoService] Found 'items' property");
                    // Log the first few raw items to inspect structure
                    var itemsList = items.EnumerateArray().ToList();
                    System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Total items in array: {itemsList.Count}");

                    for (int i = 0; i < Math.Min(3, itemsList.Count); i++)
                    {
                        System.Diagnostics.Debug.WriteLine($"[RoomPhotoService]   Raw item {i}: {itemsList[i].GetRawText()}");
                    }

                    photos = JsonSerializer.Deserialize<List<RoomPhoto>>(items.GetRawText(), options);

                    if (photos != null && photos.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ✓ Photos deserialized successfully");
                        System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] First photo RoomId: {photos[0].RoomId}");
                    }
                }
                else if (wrapper.TryGetProperty("roomPhotos", out var roomPhotos))
                {
                    System.Diagnostics.Debug.WriteLine("[RoomPhotoService] Found 'roomPhotos' property");
                    photos = JsonSerializer.Deserialize<List<RoomPhoto>>(roomPhotos.GetRawText(), options);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ⚠ No known wrapper property found");
                    System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Available properties: {string.Join(", ", wrapper.EnumerateObject().Select(p => p.Name))}");
                }
            }

            if (photos == null)
            {
                System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ⚠ Photos is null, initializing empty list");
                photos = [];
            }

            _cachedPhotos = photos;
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Successfully cached {photos.Count} room photos");

            // Log first few photos with details
            var uniqueRoomIds = photos.Select(p => p.RoomId).Distinct().OrderBy(x => x).ToList();
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Unique RoomIDs in cache ({uniqueRoomIds.Count}): {string.Join(", ", uniqueRoomIds.Take(20))}");
            if (uniqueRoomIds.Count > 20)
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService]   ... and {uniqueRoomIds.Count - 20} more RoomIDs");

            foreach (var photo in photos.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService]   - Photo ID: {photo.Id}, RoomID: {photo.RoomId}, FileID: {photo.FileId}, DateModified: {photo.DateModified}");
            }

            if (photos.Count > 5)
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService]   ... and {photos.Count - 5} more");

            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ==================== GET ROOM PHOTOS COMPLETE ====================");
            return photos;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✗ ERROR: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ==================== GET ROOM PHOTOS FAILED ====================");
            throw new Exception($"Błąd pobierania zdjęć pokojów: {ex.Message}", ex);
        }
    }

    public async Task<List<RoomPhoto>> GetRoomPhotosByRoomIdAsync(int roomId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ==================== GET PHOTOS BY ROOM ID ({roomId}) START ====================");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Fetching all photos and filtering by RoomID: {roomId}...");

            var photos = await GetRoomPhotosAsync();
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Total photos fetched: {photos.Count}");

            var filteredPhotos = photos.Where(p => p.RoomId == roomId).ToList();
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Found {filteredPhotos.Count} photos for RoomID {roomId}");

            foreach (var photo in filteredPhotos)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService]   - Photo ID: {photo.Id}, FileID: {photo.FileId}");
            }

            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ==================== GET PHOTOS BY ROOM ID COMPLETE ====================");
            return filteredPhotos;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✗ ERROR in GetRoomPhotosByRoomIdAsync ({roomId}): {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Stack trace: {ex.StackTrace}");
            return [];
        }
    }

    public async Task<RoomPhoto?> GetFirstRoomPhotoByRoomIdAsync(int roomId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ==================== GET FIRST PHOTO BY ROOM ID ({roomId}) START ====================");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Requesting first photo for RoomID: {roomId}...");

            var photos = await GetRoomPhotosAsync();
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Total photos in cache: {photos.Count}");

            var firstPhoto = photos.FirstOrDefault(p => p.RoomId == roomId);

            if (firstPhoto != null)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Found first photo - ID: {firstPhoto.Id}, FileID: {firstPhoto.FileId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✗ No photos found for RoomID {roomId}");
                var availableRoomIds = photos.Select(p => p.RoomId).Distinct().OrderBy(x => x).ToList();
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Available RoomIDs ({availableRoomIds.Count} unique): {string.Join(", ", availableRoomIds)}");
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Looking for RoomID {roomId}, but photos have RoomIds: {string.Join(", ", photos.Select(p => $"{p.Id}({p.RoomId})").Take(10))}...");
            }

            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ==================== GET FIRST PHOTO BY ROOM ID COMPLETE ====================");
            return firstPhoto;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✗ ERROR in GetFirstRoomPhotoByRoomIdAsync ({roomId}): {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<string?> GetPhotoFileUrlAsync(int fileId)
    {
        var requestUrls = await GetPhotoFileRequestUrlsAsync(fileId);
        return requestUrls.FirstOrDefault();
    }

    public async Task<IReadOnlyList<string>> GetPhotoFileRequestUrlsAsync(int fileId, int? organizationId = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ==================== GET PHOTO FILE REQUEST URLS (FileID: {fileId}) START ====================");

            if (!await _authService.IsAuthenticatedAsync())
            {
                System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ✗ User is not authenticated");
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");
            }

            var requestUrls = new List<string>
            {
                $"{FileEndpoint}/{fileId}",
                $"{FileEndpoint}?id={fileId}",
            };

            if (organizationId.HasValue)
            {
                requestUrls.Add($"{FileEndpoint}/{fileId}?organization_id={organizationId.Value}");
                requestUrls.Add($"{FileEndpoint}?id={fileId}&organization_id={organizationId.Value}");
            }

            var uniqueUrls = requestUrls.Distinct().ToList();
            foreach (var requestUrl in uniqueUrls)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] - Candidate file request URL: {requestUrl}");
            }

            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✓ Generated {uniqueUrls.Count} candidate URL(s)");
            System.Diagnostics.Debug.WriteLine("[RoomPhotoService] ==================== GET PHOTO FILE REQUEST URLS COMPLETE ====================");
            return uniqueUrls;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] ✗ ERROR in GetPhotoFileRequestUrlsAsync ({fileId}): {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RoomPhotoService] Stack trace: {ex.StackTrace}");
            return [];
        }
    }
}
