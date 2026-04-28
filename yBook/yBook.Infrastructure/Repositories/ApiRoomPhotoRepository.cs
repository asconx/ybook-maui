using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Application.Ports;
using yBook.Domain.Entities;
using yBook.Infrastructure.Api;

namespace yBook.Infrastructure.Repositories;

public class ApiRoomPhotoRepository(HttpClient httpClient, IAuthRepository authRepository) : IRoomPhotoRepository
{
    private List<RoomPhoto>? _cache;

    public async Task<IReadOnlyList<RoomPhoto>> GetRoomPhotosAsync()
    {
        if (_cache != null)
        {
            return _cache;
        }

        var token = await authRepository.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Brak tokenu autoryzacyjnego.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoints.RoomPhotos);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var items = ExtractItems(json, options);
        _cache = items.Select(MapRoomPhoto).ToList();
        return _cache;
    }

    public async Task<RoomPhoto?> GetFirstPhotoByRoomIdAsync(int roomId)
    {
        var photos = await GetRoomPhotosAsync();
        return photos.FirstOrDefault(x => x.RoomId == roomId);
    }

    private static List<RoomPhotoDto> ExtractItems(string json, JsonSerializerOptions options)
    {
        if (json.StartsWith("["))
        {
            return JsonSerializer.Deserialize<List<RoomPhotoDto>>(json, options) ?? [];
        }

        if (json.StartsWith("{"))
        {
            var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);
            if (wrapper.TryGetProperty("items", out var items))
            {
                return JsonSerializer.Deserialize<List<RoomPhotoDto>>(items.GetRawText(), options) ?? [];
            }

            if (wrapper.TryGetProperty("data", out var data))
            {
                return JsonSerializer.Deserialize<List<RoomPhotoDto>>(data.GetRawText(), options) ?? [];
            }
        }

        return [];
    }

    private static RoomPhoto MapRoomPhoto(RoomPhotoDto dto) => new()
    {
        Id = dto.Id,
        OrganizationId = dto.OrganizationId,
        RoomId = dto.RoomId,
        DateModified = dto.DateModified,
        FileId = dto.FileId
    };

    private class RoomPhotoDto
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        [JsonPropertyName("file_id")]
        public int FileId { get; set; }
    }
}
