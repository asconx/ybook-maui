using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using yBook.Services;

namespace yBook.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _auth;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        public bool IsNotLoading => !IsLoading;

        public LoginViewModel(IAuthService auth)
        {
            _auth = auth;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            HasError     = false;
            ErrorMessage = string.Empty;
            IsLoading    = true;

            try
            {
                var success = await _auth.LoginAsync(Email.Trim(), Password);

                if (success)
                {
                    // Nawigacja do głównej strony — zastąp cały stos nawigacji
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    ErrorMessage = "Nieprawidłowy e-mail lub hasło.";
                    HasError     = true;
                }
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Brak połączenia z serwerem. Sprawdź internet.";
                HasError     = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd: {ex.Message}";
                HasError     = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password);
    }
}
