using yBook.Domain.Entities;

namespace yBook.Application.Ports;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetRoomsAsync();
}
