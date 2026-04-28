using yBook.Application.Ports;
using yBook.Domain.Entities;

namespace yBook.Application.UseCases;

public class GetRoomsUseCase(IRoomRepository roomRepository)
{
    public Task<IReadOnlyList<Room>> ExecuteAsync()
        => roomRepository.GetRoomsAsync();
}
