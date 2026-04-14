using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace yBook.Services
{
    /// <summary>
    /// Service do testowania i diagnostyki API
    /// </summary>
    public class BlockadeApiDiagnosticService
    {
        private static readonly HttpClient _httpClient = new();
        private const string ApiBaseUrl = "https://api.ybook.pl";
        private const string BlockadesEndpoint = "/blockade";

        /// <summary>
        /// Pobiera raw JSON z API (bez deserializacji) dla diagnostyki
        /// </summary>
        public static async Task<(bool success, string content, string errorMessage)> GetRawBlockadesAsync(string token)
        {
            try
            {
                var url = $"{ApiBaseUrl}{BlockadesEndpoint}";
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Fetching from: {url}");

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[Diagnostic] Success! Response length: {json.Length}");
                    
                    // Pretty print JSON
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(json);
                        var prettyJson = JsonSerializer.Serialize(jsonDoc, 
                            new JsonSerializerOptions { WriteIndented = true });
                        System.Diagnostics.Debug.WriteLine($"[Diagnostic] Formatted JSON:\n{prettyJson}");
                        return (true, prettyJson, null);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine($"[Diagnostic] Raw JSON:\n{json}");
                        return (true, json, null);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = $"HTTP {response.StatusCode}: {errorContent}";
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Error: {errorMsg}");
                return (false, null, errorMsg);
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ex.GetType().Name}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Exception: {errorMsg}");
                return (false, null, errorMsg);
            }
        }

        /// <summary>
        /// Sprawdza strukturę JSON i mapowanie do modeli
        /// </summary>
        public static void ValidateJsonStructure(string json)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Diagnostic] Validating JSON structure...");

                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    System.Diagnostics.Debug.WriteLine($"[Diagnostic] Root is Array with {root.GetArrayLength()} items");

                    if (root.GetArrayLength() > 0)
                    {
                        var firstItem = root[0];
                        System.Diagnostics.Debug.WriteLine("[Diagnostic] First item properties:");

                        foreach (var prop in firstItem.EnumerateObject())
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {prop.Name}: {prop.Value.ValueKind}");

                            // Check for rooms
                            if (prop.Name == "rooms" && prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                System.Diagnostics.Debug.WriteLine($"    Found {prop.Value.GetArrayLength()} rooms");
                                if (prop.Value.GetArrayLength() > 0)
                                {
                                    var firstRoom = prop.Value[0];
                                    System.Diagnostics.Debug.WriteLine("    First room properties:");
                                    foreach (var roomProp in firstRoom.EnumerateObject())
                                    {
                                        System.Diagnostics.Debug.WriteLine($"      - {roomProp.Name}: {roomProp.Value.ValueKind}");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    System.Diagnostics.Debug.WriteLine("[Diagnostic] Root is Object");
                    System.Diagnostics.Debug.WriteLine("[Diagnostic] Object properties:");
                    foreach (var prop in root.EnumerateObject())
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {prop.Name}: {prop.Value.ValueKind}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("[Diagnostic] JSON validation complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Validation error: {ex.Message}");
            }
        }
    }
}
