using yBook.Application.Ports;
using yBook.Application.UseCases;
using yBook.Domain.Entities;

namespace yBook.Application.Tests;

public class RoomsUseCasesTests
{
    [Fact]
    public async Task DownloadRoomImageUseCase_ReturnsBytes_FromFirstSuccessfulUrl()
    {
        var fileRepository = new FakeFileRepository
        {
            Urls = ["u1", "u2"],
            Payloads = new Dictionary<string, byte[]?>
            {
                ["u1"] = null,
                ["u2"] = [1, 2, 3]
            }
        };

        var sut = new DownloadRoomImageUseCase(fileRepository);
        var bytes = await sut.ExecuteAsync(349, 14);

        Assert.NotNull(bytes);
        Assert.Equal(3, bytes!.Length);
    }

    [Fact]
    public async Task GetRoomsWithImagesUseCase_UsesPlaceholder_WhenNoPhoto()
    {
        var getRooms = new GetRoomsUseCase(new FakeRoomRepositoryWithoutPhoto());
        var getProperties = new GetPropertiesUseCase(new FakePropertyRepository());
        var getRoomPriceModifiers = new GetRoomPriceModifiersUseCase(new FakeRoomPriceModifierRepository());
        var sut = new GetRoomsWithImagesUseCase(getRooms, getProperties, getRoomPriceModifiers);

        var result = await sut.ExecuteAsync();

        Assert.Single(result);
        Assert.Equal("placeholder.png", result[0].ImageUrl);
    }

    private sealed class FakeRoomRepositoryWithoutPhoto : IRoomRepository
    {
        public Task<IReadOnlyList<Room>> GetRoomsAsync()
            => Task.FromResult<IReadOnlyList<Room>>([new Room { Id = 68, Name = "Pokoj 1" }]);
    }

    private sealed class FakePropertyRepository : IPropertyRepository
    {
        public Task<IReadOnlyList<Property>> GetPropertiesAsync() => Task.FromResult<IReadOnlyList<Property>>([]);
    }

    private sealed class FakeRoomPriceModifierRepository : IRoomPriceModifierRepository
    {
        public Task<IReadOnlyList<RoomPriceModifier>> GetRoomPriceModifiersAsync() => Task.FromResult<IReadOnlyList<RoomPriceModifier>>([]);
    }

    private sealed class FakeFileRepository : IFileRepository
    {
        public List<string> Urls { get; set; } = [];
        public Dictionary<string, byte[]?> Payloads { get; set; } = [];

        public Task<IReadOnlyList<string>> BuildFileRequestUrlsAsync(int fileId, int? organizationId = null)
            => Task.FromResult<IReadOnlyList<string>>(Urls);

        public Task<byte[]?> DownloadFileBytesAsync(string url)
            => Task.FromResult(Payloads.TryGetValue(url, out var payload) ? payload : null);
    }
}
