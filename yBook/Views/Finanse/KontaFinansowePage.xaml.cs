using yBook.Models;
using yBook.Services;

namespace yBook.Views.Finanse
{
    public partial class KontaFinansowePage : ContentPage
    {
        private readonly IFinanseService _svc;
        private List<KontoFinansowe> _all = new();
        private TypKonta? _filterTyp = null;

        static readonly double[] ColWidths = { 180, 120, 200, 130 };

        public KontaFinansowePage(IFinanseService finanseService)
        {
            InitializeComponent();
            _svc = finanseService;
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        // Fallback bez DI
        public KontaFinansowePage()
        {
            InitializeComponent();
            _svc = IPlatformApplication.Current!.Services.GetService<IFinanseService>()
                   ?? new FinanseService(
                          IPlatformApplication.Current.Services.GetRequiredService<IAuthService>());
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

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
                _all = await _svc.GetKontaAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KontaFinansowePage] {ex.Message}");
                _all = MockFinanse.Konta();
                ApplyFilter();
            }
            finally
            {
                ShowLoader(false);
            }
        }

        async void OnTypDropdownTapped(object? sender, TappedEventArgs e)
        {
            var typy = new[] { "Wszystkie", "Bankowe", "Gotówkowe", "Karta", "Inne" };
            var wynik = await DisplayActionSheet("Typ konta", "Anuluj", null, typy);

            if (wynik is null || wynik == "Anuluj") return;

            if (wynik == "Wszystkie")
            {
                _filterTyp = null;
                LblTypFilter.Text = "Wszystkie typy";
            }
            else
            {
                _filterTyp = wynik switch
                {
                    "Bankowe"   => TypKonta.Bankowe,
                    "Gotówkowe" => TypKonta.Gotowkowe,
                    "Karta"     => TypKonta.Karta,
                    _           => TypKonta.Inne
                };
                LblTypFilter.Text = wynik;
            }
            ApplyFilter();
        }

        void ApplyFilter()
        {
            var result = _all
                .Where(k => _filterTyp is null || k.Typ == _filterTyp)
                .ToList();

            KontaList.ItemsSource = result;
            LblCount.Text = result.Count.ToString();
        }

        async void OnDodajKontoClicked(object? sender, TappedEventArgs e)
        {
            await DisplayAlert("Konta finansowe", "Formularz dodawania konta — wkrótce.", "OK");
        }

        void OnRowScrolled(object? sender, ScrolledEventArgs e) { /* reserved */ }

        private void ShowLoader(bool show)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Loader is not null)
                {
                    Loader.IsRunning = show;
                    Loader.IsVisible = show;
                }
            });
        }
    }
}
