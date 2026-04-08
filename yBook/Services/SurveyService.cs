using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Models;

namespace yBook.Services;

public class SurveyService : ISurveyService
{
    private const string BaseUrl = "https://api.ybook.pl";
    private readonly List<Survey> _surveys = new();
    private readonly HttpClient _http;

    public SurveyService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Pobiera ankiety z API https://api.ybook.pl/entity/survey
    /// </summary>
    public async Task<List<Survey>> FetchSurveysFromApiAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/entity/survey");

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = response.StatusCode;
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[SurveyService] API error {statusCode}: {content}");
                return new List<Survey>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<SurveyResponseDto>(json, options);

            if (dto?.Items == null)
                return new List<Survey>();

            _surveys.Clear();
            _surveys.AddRange(dto.Items);

            System.Diagnostics.Debug.WriteLine($"[SurveyService] Pobrano {_surveys.Count} ankiet z API");
            return _surveys.ToList();
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] HTTP error: {httpEx.Message}");
            return new List<Survey>();
        }
        catch (TaskCanceledException timeoutEx)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] Timeout: {timeoutEx.Message}");
            return new List<Survey>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] FetchSurveysFromApiAsync error: {ex.Message}");
            return new List<Survey>();
        }
    }

    public async Task<List<Survey>> GetSurveysAsync()
    {
        return await Task.FromResult(_surveys.OrderByDescending(s => s.DateModified).ToList());
    }

    public async Task<Survey?> GetSurveyByIdAsync(int id)
    {
        return await Task.FromResult(_surveys.FirstOrDefault(s => s.Id == id));
    }

    public async Task<bool> AddSurveyAsync(Survey survey)
    {
        try
        {
            if (survey == null)
                return false;

            _surveys.Add(survey);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] AddSurveyAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateSurveyAsync(Survey survey)
    {
        try
        {
            if (survey == null)
                return false;

            var existing = _surveys.FirstOrDefault(s => s.Id == survey.Id);
            if (existing == null)
                return false;

            existing.DateModified = survey.DateModified;
            existing.Aspect1 = survey.Aspect1;
            existing.Aspect2 = survey.Aspect2;
            existing.Aspect3 = survey.Aspect3;
            existing.Aspect4 = survey.Aspect4;
            existing.Aspect5 = survey.Aspect5;
            existing.Aspect6 = survey.Aspect6;
            existing.Aspect7 = survey.Aspect7;
            existing.Question1 = survey.Question1;
            existing.Question2 = survey.Question2;
            existing.Question3 = survey.Question3;

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] UpdateSurveyAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteSurveyAsync(int id)
    {
        try
        {
            var survey = _surveys.FirstOrDefault(s => s.Id == id);
            if (survey == null)
                return false;

            _surveys.Remove(survey);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] DeleteSurveyAsync error: {ex.Message}");
            return false;
        }
    }

    // ── DTO dla odpowiedzi API ────────────────────────────────────────

    private class SurveyResponseDto
    {
        [JsonPropertyName("items")]
        public List<Survey> Items { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
