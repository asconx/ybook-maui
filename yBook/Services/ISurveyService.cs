using yBook.Models;

namespace yBook.Services;

public interface ISurveyService
{
    Task<List<Survey>> GetSurveysAsync();
    Task<List<Survey>> FetchSurveysFromApiAsync();
    Task<Survey?> GetSurveyByIdAsync(int id);
    Task<bool> AddSurveyAsync(Survey survey);
    Task<bool> UpdateSurveyAsync(Survey survey);
    Task<bool> DeleteSurveyAsync(int id);
}
