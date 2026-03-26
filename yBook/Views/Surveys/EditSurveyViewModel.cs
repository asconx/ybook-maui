using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Surveys;

[QueryProperty(nameof(SurveyId), "id")]
public partial class EditSurveyViewModel : ObservableObject
{
    private readonly ISurveyService _surveyService;

    [ObservableProperty]
    private Guid surveyId = Guid.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isNewSurvey = true;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    private Survey? _currentSurvey;

    public EditSurveyViewModel(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    public override async void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SurveyId) && SurveyId != Guid.Empty)
        {
            await LoadSurvey();
        }
    }

    private async Task LoadSurvey()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            IsNewSurvey = false;

            _currentSurvey = await _surveyService.GetSurveyByIdAsync(SurveyId);
            if (_currentSurvey != null)
            {
                Title = _currentSurvey.Title;
                Description = _currentSurvey.Description;
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
    public async Task SaveSurvey()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Tytuł ankiety jest wymagany";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            bool success;

            if (IsNewSurvey || SurveyId == Guid.Empty)
            {
                var newSurvey = new Survey
                {
                    Title = Title,
                    Description = Description
                };
                success = await _surveyService.AddSurveyAsync(newSurvey);
            }
            else
            {
                var updatedSurvey = new Survey
                {
                    Id = SurveyId,
                    Title = Title,
                    Description = Description
                };
                success = await _surveyService.UpdateSurveyAsync(updatedSurvey);
            }

            if (success)
            {
                await Shell.Current.DisplayAlert("Sukces", "Ankieta zapisana pomyślnie", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "Nie udało się zapisać ankiety";
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
    public async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
