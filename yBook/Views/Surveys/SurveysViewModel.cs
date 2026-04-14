using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Surveys;

public partial class SurveysViewModel : ObservableObject
{
    private readonly ISurveyService _surveyService;

    [ObservableProperty]
    private ObservableCollection<Survey> surveys = [];

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public SurveysViewModel(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    [RelayCommand]
    private async Task FetchSurveysFromApiAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;

            // Pobierz ankiety z API
            await _surveyService.FetchSurveysFromApiAsync();

            // Następnie wczytaj z pamięci lokalnej
            var result = await _surveyService.GetSurveysAsync();
            Surveys = new ObservableCollection<Survey>(result ?? []);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd przy pobieraniu ankiet: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[SurveysViewModel] FetchSurveysFromApi error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSurveyAsync(Survey survey)
    {
        if (survey == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Potwierdzenie",
            $"Czy naprawdę chcesz usunąć ankietę?",
            "Tak", "Nie");

        if (!confirm) return;

        try
        {
            IsLoading = true;
            bool success = await _surveyService.DeleteSurveyAsync(survey.Id);

            if (success)
            {
                Surveys.Remove(survey);
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
    private async Task AddSurveyAsync()
    {
        await Shell.Current.GoToAsync("EditSurveyPage");
    }

    [RelayCommand]
    private async Task EditSurveyAsync(Survey survey)
    {
        if (survey == null) return;
        await Shell.Current.GoToAsync($"EditSurveyPage?id={survey.Id}");
    }
}
