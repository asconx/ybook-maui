using yBook.Models;

namespace yBook.Views.Finanse
{
    public partial class KontaFinansowePage : ContentPage
    {
        public KontaFinansowePage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var konta = MockFinanse.Konta();
            KontaList.ItemsSource = konta;

            var aktywne     = konta.Where(k => k.Aktywne).ToList();
            var saldoLaczne = aktywne.Sum(k => k.Saldo);

            LblSaldoLaczne.Text = $"{saldoLaczne:N2} PLN";
            LblLiczbaKont.Text  = $"{aktywne.Count} aktywnych kont";
            LblSaldoLaczne.TextColor = saldoLaczne >= 0 ? Colors.White : Color.FromArgb("#EF9A9A");
        }

        async void OnKontoSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not KontoFinansowe konto) return;
            KontaList.SelectedItem = null;

            await DisplayAlert(
                $"{konto.TypEmoji} {konto.Nazwa}",
                $"Numer:  {konto.Numer}\n" +
                $"Saldo:  {konto.SaldoStr}\n" +
                $"Waluta: {konto.Waluta}\n" +
                $"Status: {(konto.Aktywne ? "Aktywne" : "Nieaktywne")}",
                "Zamknij");
        }

        async void OnDodajKontoClicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Konta finansowe", "Formularz dodawania konta – wkrótce.", "OK");
        }
    }
}
