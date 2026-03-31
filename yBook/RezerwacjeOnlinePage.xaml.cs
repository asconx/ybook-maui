using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Views.RezerwacjeOnline;

public partial class RezerwacjeOnlinePage : ContentPage
{
    // ── Dane ──────────────────────────────────────────────────────────────────

    readonly ObservableCollection<RezerwacjaOnline> _wszystkie = new();
    ObservableCollection<RezerwacjaOnline> _widoczne = new();

    string _szukaj = string.Empty;
    int    _filtrStatus = 0;   // 0=Wszystkie, 1=Oczekujące, 2=Potwierdzone, 3=Anulowane

    // ── Inicjalizacja ─────────────────────────────────────────────────────────

    public RezerwacjeOnlinePage()
    {
        InitializeComponent();
        ZaladujPrzykladoweDane();
        OdswiezListe();
    }

    void ZaladujPrzykladoweDane()
    {
        _wszystkie.Add(new RezerwacjaOnline
        {
            Imie          = "Jan",
            Nazwisko      = "Kowalski",
            Email         = "jan@example.com",
            Telefon       = "+48 600 100 200",
            DataPrzyjazdu = DateTime.Today.AddDays(3),
            DataWyjazdu   = DateTime.Today.AddDays(6),
            TypPokoju     = "Pokój 101",
            LiczbaGosci   = 2,
            Status        = StatusRezerwacji.Oczekujaca
        });
        _wszystkie.Add(new RezerwacjaOnline
        {
            Imie          = "Anna",
            Nazwisko      = "Nowak",
            Email         = "anna@example.com",
            Telefon       = "+48 700 200 300",
            DataPrzyjazdu = DateTime.Today.AddDays(7),
            DataWyjazdu   = DateTime.Today.AddDays(10),
            TypPokoju     = "Apartament A",
            LiczbaGosci   = 2,
            Status        = StatusRezerwacji.Potwierdzona
        });
        _wszystkie.Add(new RezerwacjaOnline
        {
            Imie          = "Piotr",
            Nazwisko      = "Wiśniewski",
            Email         = "piotr@example.com",
            Telefon       = "+48 500 300 400",
            DataPrzyjazdu = DateTime.Today.AddDays(1),
            DataWyjazdu   = DateTime.Today.AddDays(3),
            TypPokoju     = "Pokój 205",
            LiczbaGosci   = 1,
            Status        = StatusRezerwacji.Anulowana
        });
    }

    // ── Filtrowanie ───────────────────────────────────────────────────────────

    void OdswiezListe()
    {
        var wynik = _wszystkie.AsEnumerable();

        // Filtr tekstu
        if (!string.IsNullOrWhiteSpace(_szukaj))
        {
            var q = _szukaj.ToLower();
            wynik = wynik.Where(r =>
                r.PelneNazwisko.ToLower().Contains(q) ||
                r.Email.ToLower().Contains(q)         ||
                r.Telefon.Contains(q)                 ||
                r.TypPokoju.ToLower().Contains(q));
        }

        // Filtr statusu
        wynik = _filtrStatus switch
        {
            1 => wynik.Where(r => r.Status == StatusRezerwacji.Oczekujaca),
            2 => wynik.Where(r => r.Status == StatusRezerwacji.Potwierdzona),
            3 => wynik.Where(r => r.Status == StatusRezerwacji.Anulowana),
            _ => wynik
        };

        // Sortuj: najpierw oczekujące, potem wg daty przyjazdu
        var lista = wynik
            .OrderBy(r => r.Status == StatusRezerwacji.Oczekujaca ? 0 : 1)
            .ThenBy(r => r.DataPrzyjazdu)
            .ToList();

        _widoczne = new ObservableCollection<RezerwacjaOnline>(lista);
        RezerwacjeList.ItemsSource = _widoczne;

        EmptyState.IsVisible = _widoczne.Count == 0;
    }

    void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        _szukaj = e.NewTextValue ?? string.Empty;
        OdswiezListe();
    }

    void OnStatusFilterChanged(object sender, EventArgs e)
    {
        _filtrStatus = StatusPicker.SelectedIndex < 0 ? 0 : StatusPicker.SelectedIndex;
        OdswiezListe();
    }

    // ── Nowa rezerwacja ───────────────────────────────────────────────────────

    async void OnNowaRezerwacjaClicked(object sender, EventArgs e)
    {
        var formPage = new RezerwacjaOnlineFormPage();
        await Navigation.PushModalAsync(formPage);

        if (formPage.Wynik is not null)
        {
            _wszystkie.Add(formPage.Wynik);
            OdswiezListe();
        }
    }

    // ── Akcje na karcie ───────────────────────────────────────────────────────

    async void OnKartaTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not RezerwacjaOnline rez) return;
        await Shell.Current.DisplayAlert(
            $"Rezerwacja #{rez.Id}",
            $"Gość:     {rez.PelneNazwisko}\n" +
            $"Pokój:    {rez.TypPokoju}\n" +
            $"Przyjazd: {rez.DataPrzyjazdu:dd.MM.yyyy}\n" +
            $"Wyjazd:   {rez.DataWyjazdu:dd.MM.yyyy}\n" +
            $"Noce:     {rez.LiczbaNoci}\n" +
            $"Goście:   {rez.LiczbaGosci}\n" +
            $"Email:    {rez.Email}\n" +
            $"Telefon:  {rez.Telefon}\n" +
            (string.IsNullOrWhiteSpace(rez.Uwagi) ? "" : $"Uwagi:    {rez.Uwagi}"),
            "Zamknij");
    }

    async void OnEdytujClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not RezerwacjaOnline rez) return;

        var formPage = new RezerwacjaOnlineFormPage(rez);
        await Navigation.PushModalAsync(formPage);

        if (formPage.Wynik is not null)
        {
            var idx = _wszystkie.IndexOf(rez);
            if (idx >= 0) _wszystkie[idx] = formPage.Wynik;
            OdswiezListe();
        }
    }

    async void OnPotwierdzClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not RezerwacjaOnline rez) return;
        if (rez.Status == StatusRezerwacji.Potwierdzona) return;

        bool ok = await Shell.Current.DisplayAlert(
            "Potwierdź rezerwację",
            $"Czy potwierdzić rezerwację gościa {rez.PelneNazwisko}?",
            "Tak, potwierdź", "Anuluj");

        if (!ok) return;

        rez.Status = StatusRezerwacji.Potwierdzona;
        OdswiezListe();
    }

    async void OnAnulujClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not RezerwacjaOnline rez) return;
        if (rez.Status == StatusRezerwacji.Anulowana) return;

        bool ok = await Shell.Current.DisplayAlert(
            "Anuluj rezerwację",
            $"Czy na pewno anulować rezerwację gościa {rez.PelneNazwisko}?",
            "Tak, anuluj", "Nie");

        if (!ok) return;

        rez.Status = StatusRezerwacji.Anulowana;
        OdswiezListe();
    }
}
