using yBook.Application.Ports;

namespace yBook.Application.UseCases;

public class DownloadRoomImageUseCase(IFileRepository fileRepository)
{
    public async Task<byte[]?> ExecuteAsync(int fileId, int? organizationId = null)
    {
        var urls = await fileRepository.BuildFileRequestUrlsAsync(fileId, organizationId);
        foreach (var url in urls)
        {
            var bytes = await fileRepository.DownloadFileBytesAsync(url);
            if (bytes != null && bytes.Length > 0)
            {
                return bytes;
            }
        }

        return null;
    }
}
