namespace yBook.Application.Ports;

public interface IFileRepository
{
    Task<IReadOnlyList<string>> BuildFileRequestUrlsAsync(int fileId, int? organizationId = null);
    Task<byte[]?> DownloadFileBytesAsync(string url);
}
