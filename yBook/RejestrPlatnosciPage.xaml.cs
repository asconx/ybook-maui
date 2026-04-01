using yBook.Models;

namespace yBook.Views.Finanse
{
    public partial class RejestrPlatnosciPage : ContentPage
    {
        List<Platnosc> _all = new();
        DateTime? _dataOd = null;
        DateTime? _dataDo = null;

        static readonly double[] ColWidths = { 100, 130, 130, 120, 100, 100, 120, 130 };

        public RejestrPlatnosciPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _all = MockFinanse.Platnosci();
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
            var result = _all.Where(p =>
            {
                bool dataOdOk = _dataOd is null || p.Data.Date >= _dataOd.Value.Date;
                bool dataDoOk = _dataDo is null || p.Data.Date <= _dataDo.Value.Date;
                return dataOdOk && dataDoOk;
            }).OrderByDescending(p => p.Data).ToList();

            PlatnosciList.ItemsSource = result;
            LblCount.Text = result.Count.ToString();
        }

        async void OnDodajClicked(object? sender, TappedEventArgs e)
        {
            await DisplayAlert("Rejestr płatności", "Formularz nowej płatności — wkrótce.", "OK");
        }

        void OnRowScrolled(object? sender, ScrolledEventArgs e) { /* reserved */ }
    }
}
