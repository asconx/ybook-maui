using System.Collections.ObjectModel;
using System.Text.Json;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Ustawienia;

public partial class PokojePage : ContentPage
{
    ObservableCollection<Pokoj> pokoje = new();
    private readonly IPanelService _panelService;
    private readonly IAuthService _authService;

    public PokojePage(IAuthService authService, IPanelService panelService)
    {
        InitializeComponent();

        _authService = authService;
        _panelService = panelService;

        PokojList.ItemsSource = pokoje;
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("..");
        });
        return true;
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
            var pokojList = await _panelService.GetPokoje();

            pokoje.Clear();
            foreach (var pokoj in pokojList)
            {
                pokoje.Add(pokoj);
            }
        }
        catch (UnauthorizedAccessException)
        {
            await DisplayAlert("Błąd", "Brak autoryzacji. Zaloguj się ponownie.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować pokojów: {ex.Message}", "OK");
        }
    }

    // 👆 TAP → DETAILS
    async void OnPokojTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var pokoj = frame?.BindingContext as Pokoj;
        if (pokoj == null) return;

        string action = await DisplayActionSheet(
            pokoj.Nazwa,
            "Anuluj",
            null,
            "Pokaż szczegóły");

        if (action == "Pokaż szczegóły")
        {
            await DisplayAlert(
                pokoj.Nazwa,
                $"Maksimum osób: {pokoj.MaxOsobLiczbą}\n" +
                $"Powierzchnia: {pokoj.Powierzchnia}m²\n" +
                $"Dostępny: {(pokoj.CzyDostepny ? "Tak" : "Nie")}\n\n" +
                $"{pokoj.Opis}",
                "OK");
        }
    }
}
