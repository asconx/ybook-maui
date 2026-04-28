using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Application.Ports;
using yBook.Domain.Entities;
using yBook.Infrastructure.Api;

namespace yBook.Infrastructure.Repositories;

public class ApiRoomPriceModifierRepository(HttpClient httpClient, IAuthRepository authRepository) : IRoomPriceModifierRepository
{
    private List<RoomPriceModifier>? _cache;

    public async Task<IReadOnlyList<RoomPriceModifier>> GetRoomPriceModifiersAsync()
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

        foreach (var endpoint in BuildCandidateEndpoints())
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = ExtractItems(json, options);
            if (items.Count == 0)
            {
                continue;
            }

            _cache = items.Select(MapRoomPriceModifier).ToList();
            return _cache;
        }

        _cache = [];
        return _cache;
    }

    private static IEnumerable<string> BuildCandidateEndpoints()
    {
        yield return ApiEndpoints.RoomPriceModifiers;
        yield return ApiEndpoints.PriceModifierRooms;
        yield return $"{ApiEndpoints.BaseUrl}/entity/roomPriceModifierLink";
    }

    private static List<RoomPriceModifierDto> ExtractItems(string json, JsonSerializerOptions options)
    {
        if (json.StartsWith("["))
        {
            return JsonSerializer.Deserialize<List<RoomPriceModifierDto>>(json, options) ?? [];
        }

        if (json.StartsWith("{"))
        {
            var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);
            if (wrapper.TryGetProperty("items", out var items))
            {
                return JsonSerializer.Deserialize<List<RoomPriceModifierDto>>(items.GetRawText(), options) ?? [];
            }

            if (wrapper.TryGetProperty("data", out var data))
            {
                return JsonSerializer.Deserialize<List<RoomPriceModifierDto>>(data.GetRawText(), options) ?? [];
            }
        }

        return [];
    }

    private static RoomPriceModifier MapRoomPriceModifier(RoomPriceModifierDto dto) => new()
    {
        Id = dto.Id,
        OrganizationId = dto.OrganizationId,
        DateModified = dto.DateModified,
        RoomId = dto.RoomId,
        PriceModifierId = dto.PriceModifierId
    };

    private class RoomPriceModifierDto
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("price_modifier_id")]
        public int PriceModifierId { get; set; }
    }
}
