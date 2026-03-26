using yBook.Models;

namespace yBook.Services;

public class SurveyService : ISurveyService
{
    private readonly List<Survey> _surveys = new();
    private readonly HttpClient _http;

    public SurveyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Survey>> GetSurveysAsync()
    {
        return await Task.FromResult(_surveys.OrderByDescending(s => s.CreatedAt).ToList());
    }

    public async Task<Survey?> GetSurveyByIdAsync(Guid id)
    {
        return await Task.FromResult(_surveys.FirstOrDefault(s => s.Id == id));
    }

    public async Task<bool> AddSurveyAsync(Survey survey)
    {
        try
        {
            if (survey == null || string.IsNullOrWhiteSpace(survey.Title))
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
            if (survey == null || string.IsNullOrWhiteSpace(survey.Title))
                return false;

            var existing = _surveys.FirstOrDefault(s => s.Id == survey.Id);
            if (existing == null)
                return false;

            existing.Title = survey.Title;
            existing.Description = survey.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyService] UpdateSurveyAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteSurveyAsync(Guid id)
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
}
