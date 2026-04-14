using yBook.Application.Ports;
using yBook.Domain.Entities;

namespace yBook.Application.UseCases;

public class GetRoomPhotoUseCase(IRoomPhotoRepository roomPhotoRepository)
{
    public Task<RoomPhoto?> ExecuteAsync(int roomId)
        => roomPhotoRepository.GetFirstPhotoByRoomIdAsync(roomId);
}
