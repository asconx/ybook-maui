using System.Net.Http;
using yBook.Services;

namespace yBook.Services;

public interface IAuthenticatedImageService
{
    Task<Stream?> GetImageStreamAsync(string imageUrl);
    Task<byte[]?> GetImageBytesAsync(string imageUrl);
}

public class AuthenticatedImageService : IAuthenticatedImageService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public AuthenticatedImageService(IAuthService authService, HttpClient httpClient)
    {
        _authService = authService;
        _httpClient = httpClient;
    }

    public async Task<Stream?> GetImageStreamAsync(string imageUrl)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] Fetching image stream: {imageUrl}");

            if (!await _authService.IsAuthenticatedAsync())
            {
                System.Diagnostics.Debug.WriteLine("[AuthenticatedImageService] ✗ User is not authenticated");
                return null;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[AuthenticatedImageService] ✗ No auth token available");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] Sending authenticated request...");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                await LogFailureDiagnosticsAsync(response);
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] ✓ Image stream obtained successfully");
            return stream;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] ✗ Error fetching image: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> GetImageBytesAsync(string imageUrl)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] Fetching image bytes: {imageUrl}");

            if (!await _authService.IsAuthenticatedAsync())
            {
                System.Diagnostics.Debug.WriteLine("[AuthenticatedImageService] ✗ User is not authenticated");
                return null;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[AuthenticatedImageService] ✗ No auth token available");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            System.Diagnostics.Debug.WriteLine("[AuthenticatedImageService] Sending authenticated request...");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                await LogFailureDiagnosticsAsync(response);
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] ✓ Image bytes obtained ({bytes.Length} bytes)");
            return bytes;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] ✗ Error fetching image: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static async Task LogFailureDiagnosticsAsync(HttpResponseMessage response)
    {
        var requestUri = response.RequestMessage?.RequestUri?.ToString() ?? "unknown";
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";

        System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] ✗ Request failed with status: {response.StatusCode}");
        System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] - Final request URI: {requestUri}");
        System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] - Response content-type: {contentType}");

        var responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            System.Diagnostics.Debug.WriteLine("[AuthenticatedImageService] - Response body is empty");
            return;
        }

        var compactBody = responseContent.Replace(Environment.NewLine, " ").Trim();
        if (compactBody.Length > 500)
        {
            compactBody = $"{compactBody[..500]}...(truncated)";
        }

        var bodyKind = contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
            ? "JSON"
            : contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
                ? "HTML"
                : "text/binary";

        System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] - Error payload type: {bodyKind}");
        System.Diagnostics.Debug.WriteLine($"[AuthenticatedImageService] - Response body preview: {compactBody}");
    }
}
