using yBook.Models;

namespace yBook.Views.Finanse
{
    public partial class RejestrPlatnosciPage : ContentPage
    {
        List<Platnosc> _all = new();
        string _activeFilter = "All";

        public RejestrPlatnosciPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _all = MockFinanse.Platnosci();

            LblWplywy.Text  = $"+{_all.Where(p => p.Przychod).Sum(p => p.Kwota):N2} zł";
            LblWydatki.Text = $"-{_all.Where(p => !p.Przychod).Sum(p => p.Kwota):N2} zł";

            ApplyFilter();
        }

        void OnFilterTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string f) return;
            _activeFilter = f;
            UpdateFilterUI();
            ApplyFilter();
        }

        void ApplyFilter()
        {
            var result = _all.Where(p => _activeFilter switch
            {
                "Przychod" => p.Przychod,
                "Rozchod"  => !p.Przychod,
                "Karta"    => p.Typ == TypPlatnosci.Karta,
                "Gotowka"  => p.Typ == TypPlatnosci.Gotowka,
                _          => true
            }).OrderByDescending(p => p.Data).ToList();

            PlatnosciList.ItemsSource = result;
        }

        void UpdateFilterUI()
        {
            var map = new Dictionary<string, Frame>
            {
                { "All",      FAll      },
                { "Przychod", FPrzychod },
                { "Rozchod",  FRozchod  },
                { "Karta",    FKarta    },
                { "Gotowka",  FGotowka  },
            };

            foreach (var (key, frame) in map)
            {
                bool active = key == _activeFilter;
                frame.BackgroundColor = active ? Color.FromArgb("#1565C0")
                    : AppInfo.RequestedTheme == AppTheme.Dark
                        ? Color.FromArgb("#2A2A2A") : Color.FromArgb("#F0F0F0");

                if (frame.Content is Label lbl)
                    lbl.TextColor = active ? Colors.White
                        : AppInfo.RequestedTheme == AppTheme.Dark
                            ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#555555");
            }
        }

        async void OnPlatnoscSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Platnosc p) return;
            PlatnosciList.SelectedItem = null;

            await DisplayAlert(
                $"{p.TypEmoji} {p.Tytul}",
                $"Klient:  {p.Klient}\n" +
                $"Konto:   {p.KontoNazwa}\n" +
                $"Kwota:   {p.KwotaStr}\n" +
                $"Data:    {p.DataStr}\n" +
                $"Kierunek: {(p.Przychod ? "Wpływ ⬇️" : "Wypływ ⬆️")}",
                "Zamknij");
        }
    }
}
