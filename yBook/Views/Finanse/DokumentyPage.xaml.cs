using yBook.Models;

namespace yBook.Views.Finanse
{
    public partial class DokumentyPage : ContentPage
    {
        List<Dokument> _all = new();
        string? _filterTyp = null;
        DateTime? _dataOd  = null;
        DateTime? _dataDo  = null;

        // Szerokości kolumn — identyczne z XAML (sumują się do szerokości tabeli)
        static readonly double[] ColWidths = { 130, 110, 140, 130, 105, 120, 110, 110, 105 };

        public DokumentyPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _all = MockFinanse.Dokumenty();
            ApplyFilter();
        }

        // ── Filtry ────────────────────────────────────────────────────────────

        void OnSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

        async void OnTypDropdownTapped(object? sender, TappedEventArgs e)
        {
            var typy = new[] { "Wszystkie", "Faktura VAT", "Paragon", "Korekta", "Nota" };
            var wynik = await DisplayActionSheet("Typ dokumentu", "Anuluj", null, typy);

            if (wynik is null || wynik == "Anuluj") return;

            _filterTyp = wynik == "Wszystkie" ? null : wynik;
            LblTypFilter.Text = wynik;
            ApplyFilter();
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
            ApplyFilter();
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
            ApplyFilter();
        }

        void ApplyFilter()
        {
            var query = SearchKlient.Text?.Trim().ToLower() ?? "";

            var result = _all.Where(d =>
            {
                bool typOk    = _filterTyp is null || d.TypLabel == _filterTyp;
                bool dataOdOk = _dataOd is null    || d.DataWystawienia.Date >= _dataOd.Value.Date;
                bool dataDoOk = _dataDo is null    || d.DataWystawienia.Date <= _dataDo.Value.Date;
                bool klientOk = string.IsNullOrEmpty(query) || d.Klient.ToLower().Contains(query);
                return typOk && dataOdOk && dataDoOk && klientOk;
            }).ToList();

            DokumentyList.ItemsSource = result;
            LblCount.Text = result.Count.ToString();
        }

        // ── Dodaj ─────────────────────────────────────────────────────────────

        async void OnDodajClicked(object? sender, TappedEventArgs e)
        {
            await DisplayAlert("Dokumenty", "Formularz nowego dokumentu — wkrótce.", "OK");
        }

        // ── Synchronizacja poziomego scrolla nagłówka z wierszami ─────────────
        // Nagłówek scrollowany przez OnHeaderScrolled gdy użytkownik przeciągnie
        // nagłówek. Wiersze CollectionView mają własny ScrollView z Scrolled=OnRowScrolled
        // — nie można zsynchronizować odwrotnie bez custom renderera, więc
        // przyjęty pattern: nagłówek scrolluje się samodzielnie, wiersze osobno.
        // W praktyce użytkownik przewija wiersze, nagłówek ręcznie lub zostawiamy
        // etykiety kolumn zamrożone jako tekst "pinned" po lewej.

        void OnHeaderScrolled(object? sender, ScrolledEventArgs e) { /* reserved */ }
        void OnRowScrolled(object? sender, ScrolledEventArgs e)     { /* reserved */ }
    }
}
