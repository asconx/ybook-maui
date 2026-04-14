using yBook.Models;
using yBook.Services;

namespace yBook.Views.Finanse
{
    public partial class RejestrPlatnosciPage : ContentPage
    {
        private readonly IFinanseService _svc;

        private List<Platnosc> _all       = [];
        private List<string>   _kontoIds  = [];   // id kont pobrane z API
        private List<string>   _kontoNames = [];  // wyświetlane nazwy

        private string?  _selectedKontoId = null;
        private DateTime? _dataOd          = null;
        private DateTime? _dataDo          = null;

        // ── Konstruktor ───────────────────────────────────────────────────────

        // DI — preferowane
        public RejestrPlatnosciPage(IFinanseService finanseService)
        {
            InitializeComponent();
            _svc = finanseService;
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        // Fallback bez DI (jeśli nie zarejestrowano w MauiProgram)
        public RejestrPlatnosciPage()
        {
            InitializeComponent();
            _svc = IPlatformApplication.Current!.Services.GetService<IFinanseService>()
                   ?? new FinanseService(
                          IPlatformApplication.Current.Services.GetRequiredService<IAuthService>());
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            ShowLoader(true);
            try
            {
                // Pobierz płatności z API (z aktywnymi filtrami)
                _all = await _svc.GetPlatnosciAsync(_dataOd, _dataDo, _selectedKontoId);

                // Załaduj nazwy kont do dropdownu (tylko raz)
                if (_kontoNames.Count == 0)
                    await LoadKontaAsync();

                ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RejestrPlatnosci] {ex.Message}");
                await DisplayAlert("Błąd", "Nie udało się załadować płatności.", "OK");
            }
            finally
            {
                ShowLoader(false);
            }
        }

        private async Task LoadKontaAsync()
        {
            try
            {
                var konta = await _svc.GetKontaAsync();
                _kontoIds   = ["", ..konta.Select(k => k.Id)];
                _kontoNames = ["Wybierz konto", ..konta.Select(k => k.Nazwa)];
            }
            catch
            {
                _kontoIds   = [""];
                _kontoNames = ["Wybierz konto"];
            }
        }

        // ── Filtry ────────────────────────────────────────────────────────────

        async void OnKontoFilterTapped(object? sender, TappedEventArgs e)
        {
            if (_kontoNames.Count <= 1)
                await LoadKontaAsync();

            var wynik = await DisplayActionSheet("Wybierz konto", "Anuluj", null,
                                                 _kontoNames.ToArray());
            if (wynik is null || wynik == "Anuluj") return;

            var idx = _kontoNames.IndexOf(wynik);
            _selectedKontoId = idx > 0 ? _kontoIds[idx] : null;
            LblKontoFilter.Text = wynik;

            await LoadAsync();   // przeładuj z nowym filtrem konta
        }

        async void OnDataOdTapped(object? sender, TappedEventArgs e)
        {
            var result = await DisplayPromptAsync(
                "Data od", "Podaj datę (dd.MM.yyyy):",
                placeholder: "np. 01.03.2024",
                initialValue: _dataOd?.ToString("dd.MM.yyyy") ?? "");

            if (result is null) return;

            if (DateTime.TryParseExact(result, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
            {
                _dataOd = dt;
                LblDataOd.Text = $"Od: {result}";
            }
            else if (string.IsNullOrWhiteSpace(result))
            {
                _dataOd = null;
                LblDataOd.Text = "Data od";
            }
            await LoadAsync();
        }

        async void OnDataDoTapped(object? sender, TappedEventArgs e)
        {
            var result = await DisplayPromptAsync(
                "Data do", "Podaj datę (dd.MM.yyyy):",
                placeholder: "np. 31.03.2024",
                initialValue: _dataDo?.ToString("dd.MM.yyyy") ?? "");

            if (result is null) return;

            if (DateTime.TryParseExact(result, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
            {
                _dataDo = dt;
                LblDataDo.Text = $"Do: {result}";
            }
            else if (string.IsNullOrWhiteSpace(result))
            {
                _dataDo = null;
                LblDataDo.Text = "Data do";
            }
            await LoadAsync();
        }

        // Lokalne filtrowanie gdy dane już pobrane
        private void ApplyFilter()
        {
            var result = _all
                .OrderByDescending(p => p.Data)
                .ToList();

            PlatnosciList.ItemsSource = result;
            LblCount.Text = result.Count.ToString();
        }

        // ── Akcje ─────────────────────────────────────────────────────────────

        async void OnDodajClicked(object? sender, TappedEventArgs e)
        {
            // TODO: nawigacja do formularza nowej płatności
            await DisplayAlert("Rejestr płatności", "Formularz nowej płatności — wkrótce.", "OK");
        }

        async void OnItemSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Platnosc p) return;
            PlatnosciList.SelectedItem = null;

            // TODO: nawigacja do szczegółów płatności
            var info = $"Data:    {p.DataStr}\n" +
                       $"Klient:  {p.Klient}\n" +
                       $"Typ:     {p.TypEmoji} {p.Typ}\n" +
                       $"Kwota:   {p.KwotaStr}\n" +
                       $"Opis:    {p.Tytul}";

            await DisplayAlert("Szczegóły płatności", info, "Zamknij");
        }

        void OnRowScrolled(object? sender, ScrolledEventArgs e) { /* reserved */ }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowLoader(bool show)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Loader.IsRunning = show;
                Loader.IsVisible = show;
            });
        }
    }
}
