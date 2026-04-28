using yBook.Domain.Entities;

namespace yBook.Application.Ports;

public interface IRoomPriceModifierRepository
{
    Task<IReadOnlyList<RoomPriceModifier>> GetRoomPriceModifiersAsync();
}
