using yBook.Models;
using yBook.Services;

namespace yBook.Views.Raporty
{
    public partial class ListaLogowPage : ContentPage
    {
        private readonly ILogiService _logiService;

        private const int PageSize = 20;
        private int _currentPage = 0;
        private int _total = 0;
        private string _searchText = "";
        private List<LogAkcji> _all = new();

        DateTime? _dataOd = null;
        DateTime? _dataDo = null;
        string? _filtrUzytkownik = null;
        TypAkcji? _filtrTyp = null;

        public ListaLogowPage(ILogiService logiService)
        {
            InitializeComponent();
            _logiService = logiService;
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            LblCount.Text = "…";
            LblPaginacja.Text = "…";
            try
            {
                var start = _currentPage * PageSize;
                var (items, total) = await _logiService.GetLogiAsync(start, PageSize);
                _total = total;
                _all = items;
                ApplyFilter();
                UpdatePaginacja();
                LblCount.Text = total.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", ex.Message, "OK");
            }
        }

        private void UpdatePaginacja()
        {
            var from = _currentPage * PageSize + 1;
            var to = Math.Min((_currentPage + 1) * PageSize, _total);
            if (_total == 0) { LblPaginacja.Text = "0 z 0"; return; }
            LblPaginacja.Text = $"{from}-{to} z {_total}";
            BtnFirst.IsEnabled = _currentPage > 0;
            BtnPrev.IsEnabled = _currentPage > 0;
            BtnNext.IsEnabled = to < _total;
            BtnLast.IsEnabled = to < _total;
        }

        private async void OnFirstPage(object? sender, EventArgs e) { _currentPage = 0; await LoadAsync(); }
        private async void OnPrevPage(object? sender, EventArgs e) { if (_currentPage > 0) { _currentPage--; await LoadAsync(); } }
        private async void OnNextPage(object? sender, EventArgs e) { if ((_currentPage + 1) * PageSize < _total) { _currentPage++; await LoadAsync(); } }
        private async void OnLastPage(object? sender, EventArgs e) { _currentPage = (_total - 1) / PageSize; await LoadAsync(); }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            _searchText = e.NewTextValue ?? "";
            ApplyFilter();
        }

        async void OnDataOdTapped(object? sender, TappedEventArgs e)
        {
            var result = await DisplayPromptAsync("Data od", "Podaj datę (dd.MM.yyyy):",
                placeholder: "np. 01.03.2024", initialValue: _dataOd?.ToString("dd.MM.yyyy") ?? "");
            if (result is null) return;
            if (DateTime.TryParseExact(result, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
            { _dataOd = dt; LblDataOd.Text = $"Od: {result}"; }
            else if (string.IsNullOrWhiteSpace(result))
            { _dataOd = null; LblDataOd.Text = "Data od"; }
            ApplyFilter();
        }

        async void OnDataDoTapped(object? sender, TappedEventArgs e)
        {
            var result = await DisplayPromptAsync("Data do", "Podaj datę (dd.MM.yyyy):",
                placeholder: "np. 31.03.2024", initialValue: _dataDo?.ToString("dd.MM.yyyy") ?? "");
            if (result is null) return;
            if (DateTime.TryParseExact(result, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
            { _dataDo = dt; LblDataDo.Text = $"Do: {result}"; }
            else if (string.IsNullOrWhiteSpace(result))
            { _dataDo = null; LblDataDo.Text = "Data do"; }
            ApplyFilter();
        }

        async void OnFiltrUzytkownikTapped(object? sender, TappedEventArgs e)
        {
            var uzytkownicy = _all.Select(l => l.Uzytkownik).Distinct().OrderBy(u => u).ToArray();
            var wybor = await DisplayActionSheet("Filtruj użytkownika", "Anuluj", "Wszyscy", uzytkownicy);
            if (wybor is null || wybor == "Anuluj") return;
            if (wybor == "Wszyscy") { _filtrUzytkownik = null; LblFiltrUzytkownik.Text = "Użytkownik"; }
            else { _filtrUzytkownik = wybor; LblFiltrUzytkownik.Text = $"👤 {wybor}"; }
            ApplyFilter();
        }

        async void OnFiltrTypTapped(object? sender, TappedEventArgs e)
        {
            var typy = Enum.GetValues<TypAkcji>().Select(t => t.ToString()).ToArray();
            var wybor = await DisplayActionSheet("Filtruj typ akcji", "Anuluj", "Wszystkie", typy);
            if (wybor is null || wybor == "Anuluj") return;
            if (wybor == "Wszystkie") { _filtrTyp = null; LblFiltrTyp.Text = "Typ akcji"; }
            else if (Enum.TryParse<TypAkcji>(wybor, out var typ)) { _filtrTyp = typ; LblFiltrTyp.Text = $"🏷 {wybor}"; }
            ApplyFilter();
        }

        void OnResetFiltrów(object? sender, TappedEventArgs e)
        {
            _dataOd = null; _dataDo = null; _filtrUzytkownik = null; _filtrTyp = null;
            _searchText = ""; EntrySearch.Text = "";
            LblDataOd.Text = "Data od"; LblDataDo.Text = "Data do";
            LblFiltrUzytkownik.Text = "Użytkownik"; LblFiltrTyp.Text = "Typ akcji";
            ApplyFilter();
        }

        void ApplyFilter()
        {
            var result = _all.Where(l =>
            {
                bool searchOk = string.IsNullOrWhiteSpace(_searchText) ||
                                l.ItemId.ToString().Contains(_searchText) ||
                                l.Id.ToString().Contains(_searchText);
                bool dataOdOk = _dataOd is null || l.Data.Date >= _dataOd.Value.Date;
                bool dataDoOk = _dataDo is null || l.Data.Date <= _dataDo.Value.Date;
                bool uzytOk = _filtrUzytkownik is null || l.Uzytkownik == _filtrUzytkownik;
                bool typOk = _filtrTyp is null || l.Typ == _filtrTyp;
                return searchOk && dataOdOk && dataDoOk && uzytOk && typOk;
            }).ToList();

            LogiList.ItemsSource = result;
        }

        void OnBodyScrolled(object? sender, ScrolledEventArgs e)
            => HeaderScroll.ScrollToAsync(e.ScrollX, 0, false);
    }
}