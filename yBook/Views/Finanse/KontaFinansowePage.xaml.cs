using yBook.Models;

namespace yBook.Views.Finanse
{
    public partial class KontaFinansowePage : ContentPage
    {
        List<KontoFinansowe> _all = new();
        TypKonta? _filterTyp = null;

        static readonly double[] ColWidths = { 180, 120, 200, 130 };

        public KontaFinansowePage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _all = MockFinanse.Konta();
            ApplyFilter();
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
                    "Bankowe"    => TypKonta.Bankowe,
                    "Gotówkowe"  => TypKonta.Gotowkowe,
                    "Karta"      => TypKonta.Karta,
                    _            => TypKonta.Inne
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
    }
}
