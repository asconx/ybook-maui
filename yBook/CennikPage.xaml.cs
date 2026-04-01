using yBook.Models;
using yBook.Services;

namespace yBook.Views.Ceny
{
    public partial class CennikPage : ContentPage
    {
        // ── Stan ──────────────────────────────────────────────────────────────
        List<CennikItem> _all  = new();
        bool             _sortAsc   = true;
        string?          _editId    = null;   // null = tryb dodawania
        private readonly IPriceService _priceService;

        public CennikPage(IPriceService priceService)
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
            _priceService = priceService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Try fetch from API, otherwise use mock data
            try
            {
                var items = await _priceService.FetchPriceModifiersAsync();
                if (items != null && items.Count > 0)
                {
                    _all = items;
                }
                else
                {
                    _all = MockCennik.Cenniki();
                }
            }
            catch
            {
                _all = MockCennik.Cenniki();
            }

            OdswiezListe();
        }

        // ── Odświeżanie widoku ────────────────────────────────────────────────

        void OdswiezListe()
        {
            var query = SearchBar.Text?.Trim().ToLower() ?? "";

            var wynik = _all
                .Where(c =>
                    string.IsNullOrEmpty(query) ||
                    c.Nazwa.ToLower().Contains(query) ||
                    c.Pokoje.Any(p => p.ToLower().Contains(query)))
                .OrderBy(c => _sortAsc ? c.Priorytet : -c.Priorytet)
                .ToList();

            CennikList.ItemsSource = wynik;
            LblLicznik.Text = _all.Count.ToString();
        }

        // ── Wyszukiwanie ─────────────────────────────────────────────────────

        void OnSearchChanged(object? sender, TextChangedEventArgs e) => OdswiezListe();

        // ── Sortowanie ───────────────────────────────────────────────────────

        void OnSortToggle(object? sender, TappedEventArgs e)
        {
            _sortAsc = !_sortAsc;
            LblSort.Text = _sortAsc ? "Priorytet ↑" : "Priorytet ↓";
            OdswiezListe();
        }

        // ── Dodawanie ────────────────────────────────────────────────────────

        void OnDodajClicked(object? sender, TappedEventArgs e)
        {
            _editId = null;
            LblModalTytul.Text = "Dodaj cennik";
            WyczyscFormularz();
            OtworzModal();
        }

        // ── Edycja ───────────────────────────────────────────────────────────

        void OnEdytujClicked(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string id) return;
            var item = _all.FirstOrDefault(c => c.Id == id);
            if (item is null) return;

            _editId = id;
            LblModalTytul.Text = "Edytuj cennik";

            EntNazwa.Text      = item.Nazwa;
            EntCena.Text       = item.Cena.ToString("0.##");
            EntPriorytet.Text  = item.Priorytet.ToString();
            DpOd.Date          = item.Od; // DateTime -> DateTime? assignment OK
            DpDo.Date          = item.Do; // DateTime -> DateTime? assignment OK
            EdtPokoje.Text     = string.Join(", ", item.Pokoje);

            OtworzModal();
        }

        // ── Usuwanie ─────────────────────────────────────────────────────────

        async void OnUsunClicked(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string id) return;
            var item = _all.FirstOrDefault(c => c.Id == id);
            if (item is null) return;

            bool ok = await DisplayAlert(
                "Usuń cennik",
                $"Czy na pewno chcesz usunąć cennik {item.Nazwa}?",
                "Usuń", "Anuluj");

            if (!ok) return;

            _all.RemoveAll(c => c.Id == id);
            OdswiezListe();
        }

        // ── Zapis (dodaj lub edytuj) ─────────────────────────────────────────

        void OnZapiszClicked(object? sender, TappedEventArgs e)
        {
            // Walidacja
            if (string.IsNullOrWhiteSpace(EntNazwa.Text))
            {
                PokazBlad("Podaj nazwę cennika.");
                return;
            }
            if (!decimal.TryParse(EntCena.Text?.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cena) || cena < 0)
            {
                PokazBlad("Podaj prawidłową cenę.");
                return;
            }
            if (!int.TryParse(EntPriorytet.Text, out int priorytet) || priorytet < 1)
            {
                PokazBlad("Priorytet musi być liczbą >= 1.");
                return;
            }
            // Use null-coalescing to compare non-nullable values safely
            if ((DpDo.Date ?? DateTime.MinValue) < (DpOd.Date ?? DateTime.MinValue))
            {
                PokazBlad("Data do nie może być wcześniejsza niż data od.");
                return;
            }
            var pokoje = (EdtPokoje.Text ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            if (pokoje.Count == 0)
            {
                PokazBlad("Dodaj co najmniej jeden pokój.");
                return;
            }

            // Zapis
            if (_editId is null)
            {
                // Nowy rekord
                _all.Add(new CennikItem
                {
                    Id        = Guid.NewGuid().ToString(),
                    Nazwa     = EntNazwa.Text.Trim(),
                    Cena      = cena,
                    Priorytet = priorytet,
                    Od        = DpOd.Date ?? DateTime.Today,
                    Do        = DpDo.Date ?? DateTime.Today,
                    Pokoje    = pokoje
                });
            }
            else
            {
                // Edycja istniejącego
                var idx = _all.FindIndex(c => c.Id == _editId);
                if (idx >= 0)
                {
                    _all[idx] = _all[idx] with
                    {
                        Nazwa     = EntNazwa.Text.Trim(),
                        Cena      = cena,
                        Priorytet = priorytet,
                        Od        = DpOd.Date ?? _all[idx].Od,
                        Do        = DpDo.Date ?? _all[idx].Do,
                        Pokoje    = pokoje
                    };
                }
            }

            ZamknijModal();
            OdswiezListe();
        }

        // ── Modal: helpers ───────────────────────────────────────────────────

        void OtworzModal()
        {
            LblBlad.IsVisible = false;
            ModalOverlay.IsVisible = true;
        }

        void ZamknijModal()
        {
            ModalOverlay.IsVisible = false;
            _editId = null;
        }

        void WyczyscFormularz()
        {
            EntNazwa.Text     = "";
            EntCena.Text      = "";
            EntPriorytet.Text = "1";
            DpOd.Date         = DateTime.Today;
            DpDo.Date         = DateTime.Today;
            EdtPokoje.Text    = "";
        }

        void PokazBlad(string msg)
        {
            LblBlad.Text      = msg;
            LblBlad.IsVisible = true;
        }

        void OnModalZamknijClicked(object? sender, TappedEventArgs e) => ZamknijModal();
        void OnModalOverlayTapped(object? sender, TappedEventArgs e)  => ZamknijModal();
    }
}
