using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Application.Ports;
using yBook.Domain.Entities;
using yBook.Infrastructure.Api;

namespace yBook.Infrastructure.Repositories;

public class ApiRoomRepository(HttpClient httpClient, IAuthRepository authRepository) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> GetRoomsAsync()
    {
        var token = await authRepository.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Brak tokenu autoryzacyjnego.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoints.Rooms);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var dtoList = ExtractItems(json, options);
        return dtoList.Select(MapRoom).ToList();
    }

    private static List<RoomDto> ExtractItems(string json, JsonSerializerOptions options)
    {
        if (json.StartsWith("["))
        {
            return JsonSerializer.Deserialize<List<RoomDto>>(json, options) ?? [];
        }

        if (json.StartsWith("{"))
        {
            var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);
            if (wrapper.TryGetProperty("items", out var items))
            {
                return JsonSerializer.Deserialize<List<RoomDto>>(items.GetRawText(), options) ?? [];
            }

            if (wrapper.TryGetProperty("data", out var data))
            {
                return JsonSerializer.Deserialize<List<RoomDto>>(data.GetRawText(), options) ?? [];
            }
        }

        return [];
    }

    private static Room MapRoom(RoomDto dto) => new()
    {
        Id = dto.Id,
        OrganizationId = dto.OrganizationId ?? 0,
        PropertyId = dto.PropertyId ?? 0,
        DateModified = dto.DateModified,
        Name = dto.Name ?? string.Empty,
        Type = dto.Type ?? 0,
        IsAvailable = dto.IsAvailable == 1,
        MaxPeople = dto.MaxNumberOfPeople ?? 0,
        Area = dto.Area?.ToString() ?? "0",
        Description = dto.Description ?? string.Empty,
        ShortName = dto.ShortName ?? string.Empty,
        DefaultPrice = dto.DefaultPrice ?? 0,
        Color = dto.Color ?? "#ffffff",
        Standard = dto.Standard ?? string.Empty,
        MinPeople = dto.MinNumberOfPeople ?? 0,
        LockId = dto.LockId ?? 0,
        CalendarPosition = dto.CalendarPosition ?? 0,
        PhotoFileId = dto.Photos?.FirstOrDefault()?.Id,
        ImageUrl = BuildImageUrl(dto.Photos?.FirstOrDefault()?.Path),
        BedSummary = BuildBedSummary(dto.Beds),
        AmenitySummary = BuildAmenitySummary(dto.Amenities),
        BedItems = BuildBedItems(dto.Beds),
        AmenityItems = BuildAmenityItems(dto.Amenities)
    };

    private static string BuildImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "placeholder.png";
        }

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return $"{ApiEndpoints.BaseUrl}{path}";
    }

    private static string BuildBedSummary(List<RoomBedDto>? beds)
    {
        if (beds == null || beds.Count == 0)
        {
            return "Brak danych o łóżkach";
        }

        var grouped = beds
            .GroupBy(bed => bed.Type)
            .Select(group => $"{group.Count()}x {MapBedType(group.Key)}");

        return string.Join(", ", grouped);
    }

    private static string MapBedType(int type) => type switch
    {
        1 => "Łóżko pojedyncze",
        2 => "Łóżko podwójne",
        3 => "Duże łóżko podwójne 180-200 cm",
        4 => "Dwa łóżka pojedyncze",
        5 => "Łóżko piętrowe",
        6 => "Rozkładana sofa",
        _ => $"Typ {type}"
    };

    private static string BuildAmenitySummary(List<RoomAmenityDto>? amenities)
    {
        if (amenities == null || amenities.Count == 0)
        {
            return "Brak udogodnień";
        }

        var grouped = amenities
            .GroupBy(amenity => amenity.Type)
            .Select(group => $"{group.Count()}x {MapAmenityType(group.Key)}");

        return string.Join(", ", grouped);
    }

    private static List<string> BuildBedItems(List<RoomBedDto>? beds)
    {
        if (beds == null || beds.Count == 0)
        {
            return [];
        }

        return beds.Select(bed => MapBedType(bed.Type)).ToList();
    }

    private static List<string> BuildAmenityItems(List<RoomAmenityDto>? amenities)
    {
        if (amenities == null || amenities.Count == 0)
        {
            return [];
        }

        return amenities
            .Select(amenity => MapAmenityType(amenity.Type))
            .Distinct()
            .ToList();
    }

    private static string MapAmenityType(int type) => type switch
    {
        3 => "Biurko",
        15 => "Mydło",
        16 => "Ogrzewanie",
        20 => "TV",
        21 => "Ręczniki",
        23 => "Suszarka",
        24 => "Czajnik",
        25 => "Szafa",
        26 => "Wi-Fi",
        _ => $"Typ {type}"
    };

    private class RoomDto
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int? OrganizationId { get; set; }

        [JsonPropertyName("property_id")]
        public int? PropertyId { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        public string? Name { get; set; }
        public int? Type { get; set; }

        [JsonPropertyName("is_available")]
        public int IsAvailable { get; set; }

        [JsonPropertyName("max_number_of_people")]
        public int? MaxNumberOfPeople { get; set; }

        [JsonConverter(typeof(StringToIntConverter))]
        public int? Area { get; set; }

        public string? Description { get; set; }

        [JsonPropertyName("short_name")]
        public string? ShortName { get; set; }

        [JsonPropertyName("default_price")]
        public int? DefaultPrice { get; set; }

        public string? Color { get; set; }
        public string? Standard { get; set; }

        [JsonPropertyName("min_number_of_people")]
        public int? MinNumberOfPeople { get; set; }

        [JsonPropertyName("lock_id")]
        public int? LockId { get; set; }

        [JsonPropertyName("calendar_position")]
        public int? CalendarPosition { get; set; }

        public List<RoomAmenityDto>? Amenities { get; set; }
        public List<RoomBedDto>? Beds { get; set; }
        public List<RoomPhotoDto>? Photos { get; set; }
    }

    private class RoomAmenityDto
    {
        public int Id { get; set; }
        public int Type { get; set; }
    }

    private class RoomBedDto
    {
        public int Id { get; set; }
        public int Type { get; set; }
    }

    private class RoomPhotoDto
    {
        public int Id { get; set; }
        public string? Path { get; set; }
    }

    private class StringToIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => reader.GetInt32(),
                JsonTokenType.String when int.TryParse(reader.GetString(), out var value) => value,
                JsonTokenType.Null => null,
                _ => null
            };
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
                return;
            }

            writer.WriteNullValue();
        }
    }
}
