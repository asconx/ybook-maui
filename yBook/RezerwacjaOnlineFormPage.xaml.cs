using yBook.Models;

namespace yBook.Views.RezerwacjeOnline;

public partial class RezerwacjaOnlineFormPage : ContentPage
{
    public RezerwacjaOnline? Wynik { get; private set; }

    int _liczbaGosci = 1;
    readonly RezerwacjaOnline? _edytowana;

    // ── Konstruktor ───────────────────────────────────────────────────────────

    public RezerwacjaOnlineFormPage(RezerwacjaOnline? rez = null)
    {
        InitializeComponent();

        _edytowana = rez;

        // Domyślne daty
        DataPrzyjazduPicker.Date = rez?.DataPrzyjazdu ?? DateTime.Today.AddDays(1);
        DataPrzyjazduPicker.MinimumDate = DateTime.Today;

        DataWyjazdPicker.Date = rez?.DataWyjazdu ?? DateTime.Today.AddDays(2);
        DataWyjazdPicker.MinimumDate = DateTime.Today.AddDays(1);

        AktualizujNoce();

        // Tryb edycji
        if (rez is not null)
        {
            TytulLabel.Text    = "Edycja rezerwacji";
            ImieEntry.Text     = rez.Imie;
            NazwiskoEntry.Text = rez.Nazwisko;
            EmailEntry.Text    = rez.Email;
            TelefonEntry.Text  = rez.Telefon;
            UwagiEditor.Text   = rez.Uwagi;

            // Pokój
            int idx = TypPokojuPicker.Items.IndexOf(rez.TypPokoju);
            if (idx >= 0) TypPokojuPicker.SelectedIndex = idx;

            // Goście
            _liczbaGosci = Math.Max(1, rez.LiczbaGosci);
            LiczbaGosciLabel.Text = _liczbaGosci.ToString();
        }
    }

    // ── Daty ─────────────────────────────────────────────────────────────────

    void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // Wymuś: wyjazd >= przyjazd + 1 dzień
        if (DataWyjazdPicker.Date <= DataPrzyjazduPicker.Date)
            DataWyjazdPicker.Date = DataPrzyjazduPicker.Date.AddDays(1);

        AktualizujNoce();
    }

    void AktualizujNoce()
    {
        int noce = Math.Max(0, (DataWyjazdPicker.Date - DataPrzyjazduPicker.Date).Days);
        NoceLabel.Text = noce switch
        {
            0 => "🌙  —",
            1 => "🌙  1 noc",
            _ => $"🌙  {noce} noce/nocy"
        };
    }

    // ── Liczba gości ──────────────────────────────────────────────────────────

    void OnMinusGosc(object sender, EventArgs e)
    {
        if (_liczbaGosci <= 1) return;
        _liczbaGosci--;
        LiczbaGosciLabel.Text = _liczbaGosci.ToString();
    }

    void OnPlusGosc(object sender, EventArgs e)
    {
        if (_liczbaGosci >= 10) return;
        _liczbaGosci++;
        LiczbaGosciLabel.Text = _liczbaGosci.ToString();
    }

    // ── Zapis ─────────────────────────────────────────────────────────────────

    async void OnZapiszClicked(object sender, EventArgs e)
    {
        // Walidacja
        if (string.IsNullOrWhiteSpace(ImieEntry.Text))
        { await WyswietlBlad("Podaj imię gościa."); return; }

        if (string.IsNullOrWhiteSpace(NazwiskoEntry.Text))
        { await WyswietlBlad("Podaj nazwisko gościa."); return; }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !EmailEntry.Text.Contains('@'))
        { await WyswietlBlad("Podaj poprawny adres e-mail."); return; }

        if (string.IsNullOrWhiteSpace(TelefonEntry.Text))
        { await WyswietlBlad("Podaj numer telefonu."); return; }

        if (TypPokojuPicker.SelectedIndex < 0)
        { await WyswietlBlad("Wybierz typ / numer pokoju."); return; }

        if (DataWyjazdPicker.Date <= DataPrzyjazduPicker.Date)
        { await WyswietlBlad("Data wyjazdu musi być późniejsza niż data przyjazdu."); return; }

        // Buduj obiekt (zachowaj ID i datę złożenia przy edycji)
        Wynik = new RezerwacjaOnline
        {
            Id             = _edytowana?.Id             ?? Guid.NewGuid().ToString("N")[..8].ToUpper(),
            DataZlozenia   = _edytowana?.DataZlozenia   ?? DateTime.Now,
            Status         = _edytowana?.Status         ?? StatusRezerwacji.Oczekujaca,

            Imie           = ImieEntry.Text.Trim(),
            Nazwisko       = NazwiskoEntry.Text.Trim(),
            Email          = EmailEntry.Text.Trim(),
            Telefon        = TelefonEntry.Text.Trim(),
            DataPrzyjazdu  = DataPrzyjazduPicker.Date,
            DataWyjazdu    = DataWyjazdPicker.Date,
            TypPokoju      = TypPokojuPicker.SelectedItem?.ToString() ?? string.Empty,
            LiczbaGosci    = _liczbaGosci,
            Uwagi          = UwagiEditor.Text?.Trim() ?? string.Empty
        };

        await Navigation.PopModalAsync();
    }

    async void OnAnulujClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    Task WyswietlBlad(string msg) =>
        Shell.Current.DisplayAlert("Błąd walidacji", msg, "OK");
}
