using yBook.Application.Ports;
using yBook.Domain.Entities;

namespace yBook.Application.UseCases;

public class GetRoomBedsUseCase(IRoomBedRepository roomBedRepository)
{
    public Task<IReadOnlyList<RoomBed>> ExecuteAsync()
        => roomBedRepository.GetRoomBedsAsync();
}
