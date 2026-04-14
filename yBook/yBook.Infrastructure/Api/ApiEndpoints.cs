namespace yBook.Infrastructure.Api;

public static class ApiEndpoints
{
    public const string BaseUrl = "https://api.ybook.pl";
    public const string Authorize = $"{BaseUrl}/authorize";
    public const string Rooms = $"{BaseUrl}/room";
    public const string RoomPhotos = $"{BaseUrl}/entity/roomPhoto";
    public const string RoomBeds = $"{BaseUrl}/entity/roomBed";
    public const string Properties = $"{BaseUrl}/property";
    public const string RoomPriceModifiers = $"{BaseUrl}/entity/roomPriceModifier";
    public const string PriceModifierRooms = $"{BaseUrl}/entity/priceModifierRoom";
    public const string Files = $"{BaseUrl}/entity/file";
}
