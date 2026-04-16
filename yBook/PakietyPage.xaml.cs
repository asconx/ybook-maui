using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using yBook.Services;

namespace yBook.Views.Pakiety;

public partial class PakietyPage : ContentPage
{
    // =========================================
    //                VARIABLES
    // =========================================
    private List<Pakiet> _all = new();
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    // Fallbackowe pola kontrolek (jeśli XAML nie został poprawnie wygenerowany)
    // Dzięki nim klasa skompiluje się nawet bez wygenerowanego pliku .g.cs
    internal Entry Search;
    internal CollectionView Lista;
    internal Label LblCount;
    internal View EmptyView;

    // =========================================
    //                   START
    // =========================================
    public PakietyPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _httpClient  = new HttpClient();
    }

    // Proste tworzenie UI gdy XAML nie został zlinkowany / wygenerowany.
    // Jeśli XAML jest poprawny, ta metoda i pola nie będą przeszkadzać.
    private void InitializeComponent()
    {
        // jeśli XAML wygenerował pola, nie nadpisuj ich
        if (Search != null && Lista != null && LblCount != null && EmptyView != null)
            return;

        Search = new Entry { Placeholder = "Szukaj..." };
        Search.TextChanged += OnSearchChanged;

        LblCount = new Label { VerticalOptions = LayoutOptions.Center };

        EmptyView = new Label { Text = "Brak wyników", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

        Lista = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
            SelectionMode = SelectionMode.None,
            IsVisible = false
        };

        // minimalny układ strony (zastępuje XAML gdy go brak)
        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(10)
        };
        header.Add(Search);
        header.Add(LblCount, 1, 0);

        var main = new Grid();
        main.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        main.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        main.Children.Add(header);
        main.Add(EmptyView, 0, 1);
        main.Add(Lista, 0, 1);

        Content = new StackLayout
        {
            Children = { main }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPakiety();
    }

    // =========================================
    //                  EVENTS
    // =========================================
    void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    async void OnDodajClicked(object sender, EventArgs e)
    {
        var form = new PakietFormPage(_authService, pakiet: null, modif: false, refresh: ApplyFilter);
        form.OnSaved += _ => MainThread.BeginInvokeOnMainThread(async () => await LoadPakiety());
        await Shell.Current.Navigation.PushAsync(form);
    }

    async void OnModifClicked(object sender, TappedEventArgs e)
    {
        var item = e.Parameter as Pakiet;
        if (item == null) return;

        var form = new PakietFormPage(_authService, pakiet: item, modif: true, refresh: ApplyFilter);
        form.OnSaved += updated =>
        {
            var idx = _all.IndexOf(item);
            if (idx >= 0) _all[idx] = updated;
            MainThread.BeginInvokeOnMainThread(ApplyFilter);
        };
        await Shell.Current.Navigation.PushAsync(form);
    }

    async void OnDeleteClicked(object sender, TappedEventArgs e)
    {
        var item = e.Parameter as Pakiet;
        if (item == null) return;

        bool ok = await DisplayAlert("Usuń pakiet", $"Na pewno usunąć pakiet \"{item.Nazwa}\"?\nOperacji nie można cofnąć.", "Tak, usuń", "Anuluj");
        if (!ok) return;

        bool success = await DeletePakietAsync(item.Id);
        if (success)
        {
            _all.Remove(item);
            ApplyFilter();
        }
    }

    // =========================================
    //              API — LOAD
    // =========================================
    private async Task LoadPakiety()
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji. Zaloguj się ponownie.", "OK");
                return;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Błąd", "Nie znaleziono tokenu.", "OK");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/entity/package");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json    = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result  = JsonSerializer.Deserialize<PackageResponse>(json, options);

                _all.Clear();
                if (result?.Items != null)
                    foreach (var item in result.Items)
                        _all.Add(new Pakiet { Id = item.Id, Nazwa = item.Name, Cena = item.Price, ZdjecieUrl = item.ImageUrl, DataOd = item.DateFrom, DataDo = item.DateTo, Opis = item.Description });

                ApplyFilter();
            }
            else
            {
                await DisplayAlert("Błąd", $"Nie udało się załadować pakietów: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Błąd: {ex.Message}", "OK");
        }
    }

    // =========================================
    //              API — DELETE
    // =========================================
    private async Task<bool> DeletePakietAsync(int id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.ybook.pl/entity/package/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return true;

            await DisplayAlert("Błąd", $"Nie udało się usunąć: {response.StatusCode}", "OK");
            return false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", ex.Message, "OK");
            return false;
        }
    }

    // =========================================
    //             PRIVATE METHODS
    // =========================================
    private void ApplyFilter()
    {
        var q = Search?.Text?.ToLower() ?? "";
        var result = _all.Where(x => string.IsNullOrEmpty(q) || x.Nazwa.ToLower().Contains(q)).ToList();
        Lista.ItemsSource   = result;
        LblCount.Text       = result.Count.ToString();
        EmptyView.IsVisible = result.Count == 0;
        Lista.IsVisible     = result.Count > 0;
    }

    // ── API response models ──
    private class PackageItem { public int Id { get; set; } public string Name { get; set; } = ""; public decimal Price { get; set; } public string? ImageUrl { get; set; } public string? DateFrom { get; set; } public string? DateTo { get; set; } public string? Description { get; set; } }
    private class PackageResponse { public List<PackageItem> Items { get; set; } = new(); public int Total { get; set; } }
}

// =========================================
//                  MODEL
// =========================================
public class Pakiet
{
    public int      Id         { get; set; }
    public string   Nazwa      { get; set; } = "";
    public decimal  Cena       { get; set; }
    public string?  ZdjecieUrl { get; set; }
    public string?  DataOd     { get; set; }
    public string?  DataDo     { get; set; }
    public string?  Opis       { get; set; }

    public string CenaStr   => Cena > 0 ? $"{Cena:N2} zł" : "—";
    public string ZakresStr
    {
        get
        {
            if (!string.IsNullOrEmpty(DataOd) && !string.IsNullOrEmpty(DataDo)) return $"{DataOd} – {DataDo}";
            if (!string.IsNullOrEmpty(DataOd)) return $"od {DataOd}";
            return "—";
        }
    }
}
