using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Surveys;

[QueryProperty(nameof(SurveyIdRaw), "id")]
public partial class EditSurveyViewModel : ObservableObject
{
    private readonly ISurveyService _surveyService;

    // QueryProperty musi byc string - MAUI nie konwertuje automatycznie na Guid
    [ObservableProperty]
    private string surveyIdRaw = string.Empty;

    private Guid _surveyId = Guid.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isNewSurvey = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private Survey? _currentSurvey;

    public EditSurveyViewModel(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    protected override async void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
        {
            _surveyId = guid;
            IsNewSurvey = false;
            _ = LoadSurveyAsync();
        }
        else
        {
            _surveyId = Guid.Empty;
            IsNewSurvey = true;
        }
    }

    private async Task LoadSurveyAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _currentSurvey = await _surveyService.GetSurveyByIdAsync(_surveyId);
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
    private async Task SaveSurveyAsync()
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

            if (IsNewSurvey || _surveyId == Guid.Empty)
            {
                success = await _surveyService.AddSurveyAsync(new Survey
                {
                    Title = Title,
                    Description = Description
                });
            }
            else
            {
                success = await _surveyService.UpdateSurveyAsync(new Survey
                {
                    Id = _surveyId,
                    Title = Title,
                    Description = Description
                });
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
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}