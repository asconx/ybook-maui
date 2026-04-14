using yBook.Application.Ports;
using yBook.Domain.Entities;

namespace yBook.Application.UseCases;

public class GetRoomPriceModifiersUseCase(IRoomPriceModifierRepository roomPriceModifierRepository)
{
    public Task<IReadOnlyList<RoomPriceModifier>> ExecuteAsync()
        => roomPriceModifierRepository.GetRoomPriceModifiersAsync();
}
