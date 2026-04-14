using yBook.Domain.Entities;

namespace yBook.Application.Ports;

public interface IRoomPhotoRepository
{
    Task<IReadOnlyList<RoomPhoto>> GetRoomPhotosAsync();
    Task<RoomPhoto?> GetFirstPhotoByRoomIdAsync(int roomId);
}
