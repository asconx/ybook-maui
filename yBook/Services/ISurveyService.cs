using yBook.Models;

namespace yBook.Services;

public interface ISurveyService
{
    Task<List<Survey>> GetSurveysAsync();
    Task<Survey?> GetSurveyByIdAsync(Guid id);
    Task<bool> AddSurveyAsync(Survey survey);
    Task<bool> UpdateSurveyAsync(Survey survey);
    Task<bool> DeleteSurveyAsync(Guid id);
}
