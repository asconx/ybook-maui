using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Ceny
{
    public partial class CennikiListaPage : ContentPage
    {
        public CennikiListaPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CennikiList.ItemsSource = MockCennik.Cenniki();
            UpdateEmpty();

            // Pobierz z API (async, fire-and-forget)
            _ = FetchCennikiFromApiAsync();
        }

        void UpdateEmpty()
        {
            EmptyLabel.IsVisible = ((System.Collections.ICollection)CennikiList.ItemsSource)?.Count == 0;
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is CennikItem item)
            {
                // TODO: Otwórz modal edycji
                await DisplayAlert("Edytuj", $"Edytuj {item.Nazwa}", "OK");
            }
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not CennikItem item) return;
            bool ok = await DisplayAlert("Usuń cennik", $"Czy na pewno chcesz usunąć {item.Nazwa}?", "Usuń", "Anuluj");
            if (!ok) return;
            // Lokalnie: usuń z listy
            var list = ((System.Collections.IList)CennikiList.ItemsSource);
            list.Remove(item);
            UpdateEmpty();
        }

        async void OnDodajClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Dodaj", "Otwórz formularz dodawania", "OK");
        }

        // Pobierz listę cenników z API i dopisz nowe (nie nadpisuj istniejących)
        async System.Threading.Tasks.Task FetchCennikiFromApiAsync()
        {
            try
            {
                var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                var token = await auth.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return;

                using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await http.GetAsync("/entity/priceModifier");
                if (!resp.IsSuccessStatusCode) return;

                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("items", out var items)) return;

                // existing names to avoid duplicates
                var existing = new HashSet<string>(
                    ((System.Collections.IEnumerable)CennikiList.ItemsSource!).Cast<CennikItem>()
                        .Select(c => c.Nazwa.Trim().ToLowerInvariant())
                );

                var list = ((System.Collections.IList)CennikiList.ItemsSource!);

                foreach (var it in items.EnumerateArray())
                {
                    var id = it.TryGetProperty("id", out var pId) ? pId.GetString() ?? string.Empty : string.Empty;
                    var name = it.TryGetProperty("name", out var pName) ? pName.GetString() ?? string.Empty : string.Empty;
                    var price = it.TryGetProperty("price", out var pPrice) ? pPrice.GetDecimal() : 0m;
                    var priority = it.TryGetProperty("priority", out var pPr) ? pPr.GetInt32() : 1;
                    var dateFrom = it.TryGetProperty("dateFrom", out var pDf) && pDf.ValueKind != JsonValueKind.Null ? pDf.GetDateTime() : DateTime.Today;
                    var dateTo = it.TryGetProperty("dateTo", out var pDt) && pDt.ValueKind != JsonValueKind.Null ? pDt.GetDateTime() : DateTime.Today;

                    var rooms = new System.Collections.Generic.List<string>();
                    if (it.TryGetProperty("rooms", out var pRooms) && pRooms.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var r in pRooms.EnumerateArray())
                        {
                            if (r.ValueKind == JsonValueKind.String) rooms.Add(r.GetString()!);
                        }
                    }

                    var normalized = name.Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(normalized) || existing.Contains(normalized)) continue;

                    var ci = new CennikItem
                    {
                        Id = id,
                        Nazwa = name,
                        Cena = price,
                        Priorytet = priority,
                        Od = dateFrom,
                        Do = dateTo,
                        Pokoje = rooms
                    };

                    list.Add(ci);
                    existing.Add(normalized);
                }

                // Refresh UI on main thread
                MainThread.BeginInvokeOnMainThread(() => CennikiList.ItemsSource = list);
                System.Diagnostics.Debug.WriteLine("[CennikiLista] Pobrano cenniki z API");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CennikiLista] Fetch error: {ex.Message}");
            }
        }
    }
}
