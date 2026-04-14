using yBook.Domain.Entities;

namespace yBook.Application.UseCases;

public class GetRoomsWithImagesUseCase(
    GetRoomsUseCase getRoomsUseCase,
    GetPropertiesUseCase getPropertiesUseCase,
    GetRoomPriceModifiersUseCase getRoomPriceModifiersUseCase)
{
    public async Task<IReadOnlyList<Room>> ExecuteAsync()
    {
        var rooms = (await getRoomsUseCase.ExecuteAsync()).ToList();
        var properties = await getPropertiesUseCase.ExecuteAsync();
        var roomPriceModifiers = await getRoomPriceModifiersUseCase.ExecuteAsync();

        var propertiesById = properties.ToDictionary(property => property.Id, property => property);
        var modifierByRoomId = roomPriceModifiers
            .GroupBy(link => link.RoomId)
            .ToDictionary(group => group.Key, group => group.First().PriceModifierId);

        foreach (var room in rooms)
        {
            room.BedSummary = string.IsNullOrWhiteSpace(room.BedSummary)
                ? "Brak danych o łóżkach"
                : room.BedSummary;
            room.PriceModifierId = modifierByRoomId.TryGetValue(room.Id, out var priceModifierId) ? priceModifierId : null;

            if (propertiesById.TryGetValue(room.PropertyId, out var property))
            {
                room.PropertyName = property.Name;
                room.PropertyAddress = property.Address;

                // Fallback to property data when room payload misses display names.
                if (string.IsNullOrWhiteSpace(room.Name))
                {
                    room.Name = property.Name;
                }

                if (string.IsNullOrWhiteSpace(room.ShortName))
                {
                    room.ShortName = property.Name;
                }
            }
            room.ImageUrl = string.IsNullOrWhiteSpace(room.ImageUrl)
                ? "placeholder.png"
                : room.ImageUrl;
        }

        return rooms;
    }
}
