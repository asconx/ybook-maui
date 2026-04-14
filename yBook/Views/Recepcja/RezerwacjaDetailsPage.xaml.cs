using yBook.Models;
using yBook.Services;

namespace yBook.Views.Recepcja
{
    public partial class RezerwacjaDetailsPage : ContentPage
    {
        private RezerwacjaOnline _rezerwacja;
        private readonly IRezerwacjaService _rezerwacjaService;

        public RezerwacjaDetailsPage()
        {
            InitializeComponent();
            _rezerwacjaService = new RezerwacjaService();
            
            // Ustawienie tekstu przycisku strza³ki
            this.Loaded += (s, e) =>
            {
                var backButton = this.FindByName<Button>("BackButton");
                if (backButton != null)
                    backButton.Text = "\u2190"; // ?
            };
        }

        public void LoadRezerwacja(string rezerwacjaId)
        {
            _rezerwacja = _rezerwacjaService.GetRezerwacjaById(rezerwacjaId);
            
            if (_rezerwacja == null) return;

            // Wype³nianie danych
            LblPelneNazwisko.Text = _rezerwacja.PelneNazwisko;
            LblId.Text = $"Rezerwacja nr. {_rezerwacja.Id}";
            LblEmail.Text = _rezerwacja.Email;
            LblTelefon.Text = _rezerwacja.Telefon;
            LblDataPrzyjazdu.Text = _rezerwacja.DataPrzyjazdu.ToString("yyyy-MM-dd");
            LblDataWyjazdu.Text = _rezerwacja.DataWyjazdu.ToString("yyyy-MM-dd");
            LblLiczbaNoci.Text = $"{_rezerwacja.LiczbaNoci} nocy";
            LblLiczbaGosci.Text = $"{_rezerwacja.LiczbaGosci} osob";
            LblTypPokoju.Text = _rezerwacja.TypPokoju;
            EditorUwagi.Text = _rezerwacja.Uwagi ?? string.Empty;
            LblRozliczenie.Text = _rezerwacja.Rozliczenie;
            PkrStatus.SelectedItem = _rezerwacja.Status.ToString();

            // Dodatkowe informacje
            LblPowiadomienia.Text = "Email potwierdzajacy: 14-04-2026";
            LblRejestr.Text = $"Rezerwacja utworzona 14-04-2026";
        }

        async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (_rezerwacja == null) return;

            // Otworz dialog edycji danych goscia
            var newImie = await DisplayPromptAsync(
                "Edycja danych",
                "Podaj imie:",
                "Zapisz",
                "Anuluj",
                initialValue: _rezerwacja.Imie);

            if (newImie != null && newImie != _rezerwacja.Imie)
            {
                _rezerwacja.Imie = newImie;
                LblPelneNazwisko.Text = _rezerwacja.PelneNazwisko;
            }

            var newNazwisko = await DisplayPromptAsync(
                "Edycja danych",
                "Podaj nazwisko:",
                "Zapisz",
                "Anuluj",
                initialValue: _rezerwacja.Nazwisko);

            if (newNazwisko != null && newNazwisko != _rezerwacja.Nazwisko)
            {
                _rezerwacja.Nazwisko = newNazwisko;
                LblPelneNazwisko.Text = _rezerwacja.PelneNazwisko;
            }

            var newEmail = await DisplayPromptAsync(
                "Edycja danych",
                "Podaj email:",
                "Zapisz",
                "Anuluj",
                initialValue: _rezerwacja.Email);

            if (newEmail != null && newEmail != _rezerwacja.Email)
            {
                _rezerwacja.Email = newEmail;
                LblEmail.Text = newEmail;
            }

            var newTelefon = await DisplayPromptAsync(
                "Edycja danych",
                "Podaj telefon:",
                "Zapisz",
                "Anuluj",
                initialValue: _rezerwacja.Telefon);

            if (newTelefon != null && newTelefon != _rezerwacja.Telefon)
            {
                _rezerwacja.Telefon = newTelefon;
                LblTelefon.Text = newTelefon;
            }

            await DisplayAlert("yBook", "Dane goscia zaktualizowane!", "OK");
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_rezerwacja == null) return;

            // Aktualizacja danych
            _rezerwacja.Uwagi = EditorUwagi.Text;
            
            if (PkrStatus.SelectedItem is string status)
            {
                if (Enum.TryParse<StatusRezerwacji>(status, out var newStatus))
                {
                    _rezerwacja.Status = newStatus;
                }
            }

            await DisplayAlert("yBook", "Rezerwacja zapisana pomyslnie!", "OK");
            await Navigation.PopModalAsync();
        }
    }
}