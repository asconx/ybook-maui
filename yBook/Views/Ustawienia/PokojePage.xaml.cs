using System.Collections.ObjectModel;
using System.Text.Json;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Ustawienia;

public partial class PokojePage : ContentPage
{
    ObservableCollection<Pokoj> pokoje = new();
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public PokojePage(IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;
        _httpClient = new HttpClient();

        PokojList.ItemsSource = pokoje;
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
            // Check if user is authenticated
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji. Zaloguj się ponownie.", "OK");
                return;
            }

            // Get the token
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Błąd", "Nie znaleziono tokenu.", "OK");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/entity/room");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<RoomResponse>(json, options);

                pokoje.Clear();
                if (result?.Items != null)
                {
                    foreach (var item in result.Items)
                    {
                        pokoje.Add(new Pokoj
                        {
                            Id = item.Id,
                            OrganizationId = item.OrganizationId,
                            PropertyId = item.PropertyId,
                            DateModified = item.DateModified,
                            Nazwa = item.Name,
                            Type = item.Type,
                            CzyDostepny = item.IsAvailable == 1,
                            MaxOsobLiczbą = item.MaxNumberOfPeople,
                            Powierzchnia = item.Area,
                            Opis = item.Description,
                            ShortName = item.ShortName,
                            DefaultPrice = item.DefaultPrice,
                            Kolor = item.Color,
                            Standard = item.Standard,
                            MinOsobLiczbą = item.MinNumberOfPeople,
                            LockId = item.LockId,
                            CalendarPosition = item.CalendarPosition
                        });
                    }
                }
            }
            else
            {
                await DisplayAlert("Błąd", $"Nie udało się załadować pokojów: {response.StatusCode}", "OK");
            }
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
            "Pokaż szczegóły",
            "Zmień dostępność");

        if (action == "Zmień dostępność")
        {
            bool ok = await DisplayAlert(
                "Zmień dostępność",
                $"Zmienić dostępność pokoju '{pokoj.Nazwa}'?",
                "Tak",
                "Nie");
            
            if (ok)
            {
                bool success = await ToggleAvailabilityAsync(pokoj);
                if (success)
                {
                    pokoj.CzyDostepny = !pokoj.CzyDostepny;
                    
                    // Refresh UI
                    PokojList.ItemsSource = null;
                    PokojList.ItemsSource = pokoje;
                }
            }
        }

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

    private async Task<bool> ToggleAvailabilityAsync(Pokoj pokoj)
    {
        try
        {
            // Check authentication
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji.", "OK");
                return false;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Błąd", "Nie znaleziono tokenu.", "OK");
                return false;
            }

            // Prepare update payload
            var payload = JsonSerializer.Serialize(new
            {
                is_available = pokoj.CzyDostepny ? 0 : 1
            });

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.ybook.pl/entity/room/{pokoj.Id}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Dostępność została zmieniona.", "OK");
                return true;
            }
            else
            {
                await DisplayAlert("Błąd", $"Nie udało się zmienić dostępności: {response.StatusCode}", "OK");
                return false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się zmienić dostępności: {ex.Message}", "OK");
            return false;
        }
    }

    private class RoomItem
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int PropertyId { get; set; }
        public string DateModified { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int IsAvailable { get; set; }
        public int MaxNumberOfPeople { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public string ShortName { get; set; }
        public int DefaultPrice { get; set; }
        public string Color { get; set; }
        public string Standard { get; set; }
        public int MinNumberOfPeople { get; set; }
        public int LockId { get; set; }
        public int CalendarPosition { get; set; }
    }

    private class RoomResponse
    {
        public List<RoomItem> Items { get; set; }
        public int Total { get; set; }
    }
}
