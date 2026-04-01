using System.Collections.ObjectModel;
using System.Text.Json;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Rabaty;

public partial class RabatyPage : ContentPage
{
    ObservableCollection<Rabat> rabaty = new();
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public RabatyPage(IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;
        _httpClient = new HttpClient();

        RabatyList.ItemsSource = rabaty;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRabaty();
    }

    private async Task LoadRabaty()
    {
        try
        {
            // Check if user is authenticated
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji. Zaloguj się ponownie.", "OK");
                return;
            }

            // Get the token and add it to the request
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Błąd", "Nie znaleziono tokenu.", "OK");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/entity/discount");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<DiscountResponse>(json, options);

                rabaty.Clear();
                if (result?.Items != null)
                {
                    foreach (var item in result.Items)
                    {
                        rabaty.Add(new Rabat
                        {
                            Id = item.Id,
                            Nazwa = item.Name,
                            Kod = item.Name,
                            Procent = item.Percentage,
                            Opis = item.OnlineReservationDescription,
                            CzyOnline = item.IsVisibleOnOnlineReservation == 1,
                            DataWaznosci = DateTime.MinValue
                        });
                    }
                }
            }
            else
            {
                await DisplayAlert("Błąd", $"Nie udało się załadować rabatów: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować rabatów: {ex.Message}", "OK");
        }
    }

    private async Task<bool> AddRabatAsync(Rabat rabat)
    {
        try
        {
            // Check if user is authenticated
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji. Zaloguj się ponownie.", "OK");
                return false;
            }

            // Get the token
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Błąd", "Nie znaleziono tokenu.", "OK");
                return false;
            }

            // Prepare the payload matching the API structure
            var payload = JsonSerializer.Serialize(new
            {
                name = rabat.Nazwa,
                percentage = rabat.Procent,
                online_reservation_description = rabat.Opis,
                is_visible_on_online_reservation = rabat.CzyOnline ? 1 : 0,
                organization_id = _authService.CurrentUser?.OrganizationId ?? 0
            });

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.ybook.pl/entity/discount");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Rabat został dodany.", "OK");
                return true;
            }
            else
            {
                await DisplayAlert("Błąd", $"Nie udało się dodać rabatu: {response.StatusCode}", "OK");
                return false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się dodać rabatu: {ex.Message}", "OK");
            return false;
        }
    }

    private class DiscountItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Percentage { get; set; }
        public string OnlineReservationDescription { get; set; }
        public int IsVisibleOnOnlineReservation { get; set; }
    }

    private class DiscountResponse
    {
        public List<DiscountItem> Items { get; set; }
        public int Total { get; set; }
    }

    // ➕ DODAWANIE
    async void OnDodajRabatClicked(object sender, EventArgs e)
    {
        var page = new RabatyFormPage();
        await Navigation.PushModalAsync(page);

        page.Disappearing += async (s, ev) =>
        {
            if (page.Result != null)
            {
                // Add to API
                bool success = await AddRabatAsync(page.Result);
                if (success)
                {
                    // Reload the list from API
                    await LoadRabaty();
                }
            }
        };
    }

    // 👆 TAP → MENU
    async void OnRabatTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var rabat = frame?.BindingContext as Rabat;
        if (rabat == null) return;

        string action = await DisplayActionSheet(
            rabat.Nazwa,
            "Anuluj",
            null,
            "Edytuj",
            "Usuń");

        if (action == "Usuń")
        {
            bool ok = await DisplayAlert("Usuń", "Na pewno usunąć?", "Tak", "Nie");
            if (ok)
                rabaty.Remove(rabat);
        }

        if (action == "Edytuj")
        {
            var page = new RabatyFormPage(rabat);
            await Navigation.PushModalAsync(page);

            page.Disappearing += (s, ev) =>
            {
                if (page.Result != null)
                {
                    rabat.Nazwa = page.Result.Nazwa;
                    rabat.Kod = page.Result.Kod;
                    rabat.Procent = page.Result.Procent;
                    rabat.Opis = page.Result.Opis;
                    rabat.CzyOnline = page.Result.CzyOnline;

                    // odśwież UI
                    RabatyList.ItemsSource = null;
                    RabatyList.ItemsSource = rabaty;
                }
            };
        }
    }
}