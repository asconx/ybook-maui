using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using yBook.Services;
using yBook.Models;

namespace yBook.Views.Cenniki
{
    public partial class CennikiListaPage : ContentPage
    {
        private ObservableCollection<PriceModifier> _items = new();
        private readonly IAuthService _authService;

        public CennikiListaPage()
        {
            InitializeComponent();
            _authService = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CennikiList.ItemsSource = _items;
            UpdateEmpty();
            _items.CollectionChanged += Items_CollectionChanged;

            _ = FetchPriceModifiersAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _items.CollectionChanged -= Items_CollectionChanged;
        }

        void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateEmpty();
        }

        void UpdateEmpty()
        {
            EmptyLabel.IsVisible = _items.Count == 0;
        }

        async void OnAddClicked(object sender, EventArgs e)
        {
            // var page = new CennikiPage();
            // await Navigation.PushModalAsync(page);
            await DisplayAlert("Info", "Formularz dodawania – do implementacji.", "OK");
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PriceModifier modifier)
            {
                // var page = new CennikiPage(modifier);
                // await Navigation.PushModalAsync(page);
                await DisplayAlert("Edytuj", $"Edycja: {modifier.Name} – do implementacji.", "OK");
            }
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not PriceModifier modifier)
                return;

            bool ok = await DisplayAlert("Usuń modyfikator", $"Czy na pewno chcesz usunąć {modifier.Name}?", "Tak", "Anuluj");
            if (!ok) return;

            if (modifier.Id > 0)
            {
                try
                {
                    var token = await _authService.GetTokenAsync();
                    if (string.IsNullOrEmpty(token))
                    {
                        await DisplayAlert("Błąd", "Brak tokena autoryzacji.", "OK");
                        return;
                    }

                    using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var resp = await http.DeleteAsync($"/entity/priceModifier/{modifier.Id}");
                    if (!resp.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Błąd", $"Nie udało się usunąć modyfikatora (kod {(int)resp.StatusCode}).", "OK");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Cenniki] Delete error: {ex.Message}");
                    await DisplayAlert("Błąd", "Wystąpił problem podczas usuwania (połączenie).", "OK");
                    return;
                }
            }

            _items.Remove(modifier);
        }

        async System.Threading.Tasks.Task FetchPriceModifiersAsync()
        {
            try
            {
                var token = await _authService.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return;

                using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await http.GetAsync("/entity/priceModifier");
                if (!resp.IsSuccessStatusCode) return;

                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("items", out var items)) return;

                var existingIds = new HashSet<int>(_items.Select(m => m.Id));

                foreach (var it in items.EnumerateArray())
                {
                    var id = it.TryGetProperty("id", out var pId) ? pId.GetInt32() : 0;

                    if (existingIds.Contains(id)) continue;

                    var modifier = new PriceModifier
                    {
                        Id             = id,
                        OrganizationId = it.TryGetProperty("organization_id",      out var v) ? v.GetInt32()    : 0,
                        Name           = it.TryGetProperty("name",                 out v)     ? v.GetString() ?? string.Empty : string.Empty,
                        Description    = it.TryGetProperty("description",           out v)     ? v.GetString() ?? string.Empty : string.Empty,
                        Amount         = it.TryGetProperty("amount",               out v)     ? v.GetString() ?? "0.00" : "0.00",
                        AmountAdult    = it.TryGetProperty("amount_adult",          out v)     ? v.GetString() ?? "0.00" : "0.00",
                        AmountKid      = it.TryGetProperty("amount_kid",            out v)     ? v.GetString() ?? "0.00" : "0.00",
                        AmountInfant   = it.TryGetProperty("amount_infant",         out v)     ? v.GetString() ?? "0.00" : "0.00",
                        ApplyFromDate  = it.TryGetProperty("apply_from_date",       out v)     ? v.GetString() ?? string.Empty : string.Empty,
                        ApplyToDate    = it.TryGetProperty("apply_to_date",         out v)     ? v.GetString() ?? string.Empty : string.Empty,
                        MinDays        = it.TryGetProperty("min_days",              out v)     ? v.GetInt32()    : 0,
                        MaxDays        = it.TryGetProperty("max_days",              out v)     ? v.GetInt32()    : 0,
                        Type           = it.TryGetProperty("type",                  out v)     ? v.GetInt32()    : 0,
                        Priority       = it.TryGetProperty("priority",              out v)     ? v.GetInt32()    : 0,
                        IsMandatory    = it.TryGetProperty("is_mandatory",          out v)     && v.GetInt32() == 1,
                        IsService      = it.TryGetProperty("is_service",            out v)     && v.GetInt32() == 1,
                        IsPercentAmount= it.TryGetProperty("is_percent_amount",     out v)     && v.GetInt32() == 1,
                        IsPerPerson    = it.TryGetProperty("is_per_person",         out v)     && v.GetInt32() == 1,
                        IsDaily        = it.TryGetProperty("is_daily",              out v)     && v.GetInt32() == 1,
                        IsForRoom      = it.TryGetProperty("is_for_room",           out v)     && v.GetInt32() == 1,
                        IsForPeriod    = it.TryGetProperty("is_for_period",         out v)     && v.GetInt32() == 1,
                        IsMonday       = it.TryGetProperty("is_monday",             out v)     && v.GetInt32() == 1,
                        IsTuesday      = it.TryGetProperty("is_tuesday",            out v)     && v.GetInt32() == 1,
                        IsWednesday    = it.TryGetProperty("is_wednesday",          out v)     && v.GetInt32() == 1,
                        IsThursday     = it.TryGetProperty("is_thursday",           out v)     && v.GetInt32() == 1,
                        IsFriday       = it.TryGetProperty("is_friday",             out v)     && v.GetInt32() == 1,
                        IsSaturday     = it.TryGetProperty("is_saturday",           out v)     && v.GetInt32() == 1,
                        IsSunday       = it.TryGetProperty("is_sunday",             out v)     && v.GetInt32() == 1,
                        HideOnOnlineReservation = it.TryGetProperty("hide_on_online_reservation", out v) && v.GetInt32() == 1,
                        IncludeDiscount         = it.TryGetProperty("include_discount",           out v) && v.GetInt32() == 1,
                        IsCost                  = it.TryGetProperty("is_cost",                    out v) && v.GetInt32() == 1,
                        IsFiscal                = it.TryGetProperty("is_fiscal",                  out v) && v.GetInt32() == 1,
                    };

                    _items.Add(modifier);
                    existingIds.Add(id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cenniki] FetchPriceModifiersAsync error: {ex.Message}");
            }
        }
    }
}
