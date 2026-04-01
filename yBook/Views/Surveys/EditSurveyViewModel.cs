using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Surveys;

[QueryProperty(nameof(SurveyIdRaw), "id")]
public partial class EditSurveyViewModel : ObservableObject
{
    private readonly ISurveyService _surveyService;

    // QueryProperty musi byc string - MAUI nie konwertuje automatycznie
    [ObservableProperty]
    private string surveyIdRaw = string.Empty;

    private int _surveyId = 0;

    [ObservableProperty]
    private string aspect1 = string.Empty;

    [ObservableProperty]
    private string aspect2 = string.Empty;

    [ObservableProperty]
    private string aspect3 = string.Empty;

    [ObservableProperty]
    private string aspect4 = string.Empty;

    [ObservableProperty]
    private string aspect5 = string.Empty;

    [ObservableProperty]
    private string question1 = string.Empty;

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
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SurveyIdRaw))
        {
            if (int.TryParse(SurveyIdRaw, out var id) && id > 0)
            {
                _surveyId = id;
                IsNewSurvey = false;
                _ = LoadSurveyAsync();
            }
            else
            {
                _surveyId = 0;
                IsNewSurvey = true;
            }
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
                Aspect1 = _currentSurvey.Aspect1;
                Aspect2 = _currentSurvey.Aspect2;
                Aspect3 = _currentSurvey.Aspect3;
                Aspect4 = _currentSurvey.Aspect4;
                Aspect5 = _currentSurvey.Aspect5;
                Question1 = _currentSurvey.Question1;
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
        if (string.IsNullOrWhiteSpace(Aspect1))
        {
            ErrorMessage = "Aspecty ankiety są wymagane";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            bool success;

            if (IsNewSurvey || _surveyId == 0)
            {
                success = await _surveyService.AddSurveyAsync(new Survey
                {
                    Aspect1 = Aspect1,
                    Aspect2 = Aspect2,
                    Aspect3 = Aspect3,
                    Aspect4 = Aspect4,
                    Aspect5 = Aspect5,
                    Question1 = Question1
                });
            }
            else
            {
                success = await _surveyService.UpdateSurveyAsync(new Survey
                {
                    Id = _surveyId,
                    Aspect1 = Aspect1,
                    Aspect2 = Aspect2,
                    Aspect3 = Aspect3,
                    Aspect4 = Aspect4,
                    Aspect5 = Aspect5,
                    Question1 = Question1
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