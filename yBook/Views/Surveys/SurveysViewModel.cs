using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Surveys;

public partial class SurveysViewModel : ObservableObject
{
    private readonly ISurveyService _surveyService;

    [ObservableProperty]
    private List<Survey> surveys = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public SurveysViewModel(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    [RelayCommand]
    public async Task LoadSurveys()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _surveyService.GetSurveysAsync();
            Surveys = result ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[SurveysViewModel] LoadSurveys error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteSurvey(Survey survey)
    {
        if (survey == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Potwierdzenie",
            $"Czy naprawdę chcesz usunąć ankietę '{survey.Title}'?",
            "Tak",
            "Nie");

        if (!confirm)
            return;

        try
        {
            IsLoading = true;
            bool success = await _surveyService.DeleteSurveyAsync(survey.Id);

            if (success)
            {
                await LoadSurveysCommand.ExecuteAsync(null);
                await Shell.Current.DisplayAlert("Sukces", "Ankieta usunięta pomyślnie", "OK");
            }
            else
            {
                ErrorMessage = "Nie udało się usunąć ankiety";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddSurvey()
    {
        await Shell.Current.GoToAsync("edit-survey/new");
    }

    [RelayCommand]
    public async Task EditSurvey(Survey survey)
    {
        if (survey == null)
            return;

        await Shell.Current.GoToAsync($"edit-survey/{survey.Id}");
    }
}
