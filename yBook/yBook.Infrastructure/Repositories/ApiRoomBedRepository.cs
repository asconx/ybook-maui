using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Application.Ports;
using yBook.Domain.Entities;
using yBook.Infrastructure.Api;

namespace yBook.Infrastructure.Repositories;

public class ApiRoomBedRepository(HttpClient httpClient, IAuthRepository authRepository) : IRoomBedRepository
{
    private List<RoomBed>? _cache;

    public async Task<IReadOnlyList<RoomBed>> GetRoomBedsAsync()
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

        using var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoints.RoomBeds);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var items = ExtractItems(json, options);
        _cache = items.Select(MapRoomBed).ToList();
        return _cache;
    }

    private static List<RoomBedDto> ExtractItems(string json, JsonSerializerOptions options)
    {
        if (json.StartsWith("["))
        {
            return JsonSerializer.Deserialize<List<RoomBedDto>>(json, options) ?? [];
        }

        if (json.StartsWith("{"))
        {
            var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);
            if (wrapper.TryGetProperty("items", out var items))
            {
                return JsonSerializer.Deserialize<List<RoomBedDto>>(items.GetRawText(), options) ?? [];
            }

            if (wrapper.TryGetProperty("data", out var data))
            {
                return JsonSerializer.Deserialize<List<RoomBedDto>>(data.GetRawText(), options) ?? [];
            }
        }

        return [];
    }

    private static RoomBed MapRoomBed(RoomBedDto dto) => new()
    {
        Id = dto.Id,
        OrganizationId = dto.OrganizationId,
        DateModified = dto.DateModified,
        RoomId = dto.RoomId,
        Type = dto.Type
    };

    private class RoomBedDto
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        public int Type { get; set; }
    }
}
