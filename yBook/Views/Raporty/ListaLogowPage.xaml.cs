using yBook.Models;

namespace yBook.Views.Raporty
{
    public partial class ListaLogowPage : ContentPage
    {
        List<LogAkcji> _all = new();
        DateTime? _dataOd = null;
        DateTime? _dataDo = null;
        string? _filtrUzytkownik = null;
        TypAkcji? _filtrTyp = null;

        public ListaLogowPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _all = MockLogi.Logi();
            ApplyFilter();
        }

        // ── Filtry ────────────────────────────────────────────────────────────

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

        async void OnFiltrUzytkownikTapped(object? sender, TappedEventArgs e)
        {
            var uzytkownicy = _all.Select(l => l.Uzytkownik).Distinct().OrderBy(u => u).ToArray();
            var wybor = await DisplayActionSheet("Filtruj użytkownika", "Anuluj", "Wszyscy", uzytkownicy);

            if (wybor is null || wybor == "Anuluj") return;

            if (wybor == "Wszyscy")
            {
                _filtrUzytkownik = null;
                LblFiltrUzytkownik.Text = "Użytkownik";
            }
            else
            {
                _filtrUzytkownik = wybor;
                LblFiltrUzytkownik.Text = $"👤 {wybor}";
            }
            ApplyFilter();
        }

        async void OnFiltrTypTapped(object? sender, TappedEventArgs e)
        {
            var typy = Enum.GetValues<TypAkcji>().Select(t => t.ToString()).ToArray();
            var wybor = await DisplayActionSheet("Filtruj typ akcji", "Anuluj", "Wszystkie", typy);

            if (wybor is null || wybor == "Anuluj") return;

            if (wybor == "Wszystkie")
            {
                _filtrTyp = null;
                LblFiltrTyp.Text = "Typ akcji";
            }
            else if (Enum.TryParse<TypAkcji>(wybor, out var typ))
            {
                _filtrTyp = typ;
                LblFiltrTyp.Text = $"🏷 {wybor}";
            }
            ApplyFilter();
        }

        void OnResetFiltrów(object? sender, TappedEventArgs e)
        {
            _dataOd = null;
            _dataDo = null;
            _filtrUzytkownik = null;
            _filtrTyp = null;
            LblDataOd.Text = "Data od";
            LblDataDo.Text = "Data do";
            LblFiltrUzytkownik.Text = "Użytkownik";
            LblFiltrTyp.Text = "Typ akcji";
            ApplyFilter();
        }

        void ApplyFilter()
        {
            var result = _all.Where(l =>
            {
                bool dataOdOk = _dataOd is null || l.Data.Date >= _dataOd.Value.Date;
                bool dataDoOk = _dataDo is null || l.Data.Date <= _dataDo.Value.Date;
                bool uzytOk = _filtrUzytkownik is null || l.Uzytkownik == _filtrUzytkownik;
                bool typOk = _filtrTyp is null || l.Typ == _filtrTyp;
                return dataOdOk && dataDoOk && uzytOk && typOk;
            }).OrderByDescending(l => l.Data).ToList();

            LogiList.ItemsSource = result;
            LblCount.Text = result.Count.ToString();
        }

        void OnBodyScrolled(object? sender, ScrolledEventArgs e)
        {
            // synchronizacja poziomego scrolla nagłówka z ciałem tabeli
            HeaderScroll.ScrollToAsync(e.ScrollX, 0, false);
        }
    }
}