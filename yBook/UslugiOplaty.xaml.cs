using yBook.Models;

namespace yBook.Views.Ceny
{
    public partial class UslugiOplaty : ContentPage
    {
        List<Dokument> _all = new();
        string _activeFilter = "All";

        public UslugiOplaty()
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

        void OnFilterTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string filter) return;
            _activeFilter = filter;
            UpdateFilterUI();
            ApplyFilter();
        }

        void OnSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

        void ApplyFilter()
        {
            var query = SearchBar.Text?.Trim().ToLower() ?? "";

            var result = _all.Where(d =>
            {
                bool statusOk = _activeFilter switch
                {
                    "Oplacony"   => d.Status == StatusDokumentu.Oplacony,
                    "Oczekujacy" => d.Status == StatusDokumentu.Oczekujacy,
                    "Wystawiony" => d.Status == StatusDokumentu.Wystawiony,
                    "Anulowany"  => d.Status == StatusDokumentu.Anulowany,
                    _            => true
                };

                bool searchOk = string.IsNullOrEmpty(query) ||
                                d.Numer.ToLower().Contains(query) ||
                                d.Klient.ToLower().Contains(query);

                return statusOk && searchOk;
            }).ToList();

            DokumentyList.ItemsSource = result;
        }

        void UpdateFilterUI()
        {
            var allFilters = new Dictionary<string, Frame>
            {
                { "All",        FilterAll       },
                { "Oplacony",   FilterOplacony  },
                { "Oczekujacy", FilterOczekujacy},
                { "Wystawiony", FilterWystawiony},
                { "Anulowany",  FilterAnulowany },
            };

            foreach (var (key, frame) in allFilters)
            {
                bool active = key == _activeFilter;
                frame.BackgroundColor = active
                    ? Color.FromArgb("#1565C0")
                    : AppInfo.RequestedTheme == AppTheme.Dark
                        ? Color.FromArgb("#2A2A2A")
                        : Color.FromArgb("#F0F0F0");

                if (frame.Content is Label lbl)
                    lbl.TextColor = active ? Colors.White
                        : AppInfo.RequestedTheme == AppTheme.Dark
                            ? Color.FromArgb("#CCCCCC")
                            : Color.FromArgb("#555555");
            }
        }

        // ── Szczegóły dokumentu ───────────────────────────────────────────────

        async void OnDokumentSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Dokument dok) return;
            DokumentyList.SelectedItem = null;

            await DisplayAlert(
                $"{dok.TypLabel} — {dok.Numer}",
                $"Klient:  {dok.Klient}\n" +
                $"Data:    {dok.DataStr}\n" +
                $"Kwota:   {dok.KwotaStr}\n" +
                $"Status:  {dok.StatusLabel}",
                "Zamknij");
        }
    }
}
