using yBook.Application.Ports;
using yBook.Infrastructure.Api;

namespace yBook.Infrastructure.Repositories;

public class ApiFileRepository(HttpClient httpClient, IAuthRepository authRepository) : IFileRepository
{
    public Task<IReadOnlyList<string>> BuildFileRequestUrlsAsync(int fileId, int? organizationId = null)
    {
        var urls = new List<string>
        {
            $"{ApiEndpoints.Files}/{fileId}",
            $"{ApiEndpoints.Files}?id={fileId}"
        };

        if (organizationId.HasValue)
        {
            urls.Add($"{ApiEndpoints.Files}/{fileId}?organization_id={organizationId.Value}");
            urls.Add($"{ApiEndpoints.Files}?id={fileId}&organization_id={organizationId.Value}");
        }

        return Task.FromResult<IReadOnlyList<string>>(urls.Distinct().ToList());
    }

    public async Task<byte[]?> DownloadFileBytesAsync(string url)
    {
        var token = await authRepository.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}
