using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;
using yBook.ViewModels;

namespace yBook.Views.Ustawienia;

public partial class PokojePage : ContentPage
{
    private readonly PokojeViewModel _viewModel;
    private readonly IPanelService _panelService;
    private ObservableCollection<Pokoj> pokoje => _viewModel.Pokoje;

    public PokojePage(IPanelService panelService, PokojeViewModel viewModel)
    {
        InitializeComponent();

        _panelService = panelService;
        _viewModel = viewModel;

        BindingContext = _viewModel;
        PokojList.ItemsSource = _viewModel.Pokoje;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPokoje();
    }

    private async Task LoadPokoje()
    {
        try
        {
            await _viewModel.LoadAsync();

            if (_viewModel.Pokoje.Count == 0)
            {
                await DisplayAlert("Brak danych", "Nie znaleziono żadnych pokojów", "OK");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PokojePage] ✗ AUTHORIZATION ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine("[PokojePage] Session expired, redirecting to login...");
            await DisplayAlert("Logowanie", "Twoja sesja wygasła. Zaloguj się ponownie.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PokojePage] ✗ ERROR: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[PokojePage] Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine("[PokojePage] ==================== LOAD POKOJE FAILED ====================");
            await DisplayAlert("Błąd API", ex.Message, "OK");
        }
    }

    async void OnPokojTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not Pokoj pokoj)
            return;

        string? action = await DisplayActionSheet(
            pokoj.Nazwa ?? "Bez nazwy",
            "Anuluj",
            null,
            "Pokaż szczegóły");

        if (action == "Pokaż szczegóły")
        {
            await DisplayAlert(
                pokoj.Nazwa ?? "Pokój",
                $"Maksimum osób: {pokoj.MaxOsobLiczbą}\n" +
                $"Powierzchnia: {pokoj.Powierzchnia}m²\n" +
                $"Dostępny: {(pokoj.CzyDostepny ? "Tak" : "Nie")}\n\n" +
                $"{pokoj.Opis ?? "Brak opisu"}",
                "OK");
        }
    }

    async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadPokoje();
        RefreshControl.IsRefreshing = false;
    }
}
