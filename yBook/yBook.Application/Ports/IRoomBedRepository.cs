using yBook.Domain.Entities;

namespace yBook.Application.Ports;

public interface IRoomBedRepository
{
    Task<IReadOnlyList<RoomBed>> GetRoomBedsAsync();
}
