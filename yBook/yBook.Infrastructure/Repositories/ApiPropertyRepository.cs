using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Application.Ports;
using yBook.Domain.Entities;
using yBook.Infrastructure.Api;

namespace yBook.Infrastructure.Repositories;

public class ApiPropertyRepository(HttpClient httpClient, IAuthRepository authRepository) : IPropertyRepository
{
    private List<Property>? _cache;

    public async Task<IReadOnlyList<Property>> GetPropertiesAsync()
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

        using var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoints.Properties);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var items = ExtractItems(json, options);
        _cache = items.Select(MapProperty).ToList();
        return _cache;
    }

    private static List<PropertyDto> ExtractItems(string json, JsonSerializerOptions options)
    {
        if (json.StartsWith("["))
        {
            return JsonSerializer.Deserialize<List<PropertyDto>>(json, options) ?? [];
        }

        if (json.StartsWith("{"))
        {
            var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);
            if (wrapper.TryGetProperty("items", out var items))
            {
                return JsonSerializer.Deserialize<List<PropertyDto>>(items.GetRawText(), options) ?? [];
            }

            if (wrapper.TryGetProperty("data", out var data))
            {
                return JsonSerializer.Deserialize<List<PropertyDto>>(data.GetRawText(), options) ?? [];
            }
        }

        return [];
    }

    private static Property MapProperty(PropertyDto dto) => new()
    {
        Id = dto.Id,
        OrganizationId = dto.OrganizationId,
        DateModified = dto.DateModified,
        Name = dto.Name,
        Address = dto.Address
    };

    private class PropertyDto
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}
