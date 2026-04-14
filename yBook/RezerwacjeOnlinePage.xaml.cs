using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Views.Blokady;

public partial class RezerwacjeOnlinePage : ContentPage
{
    // ── Statyczne repozytorium dostępne dla innych stron ──
    public static ObservableCollection<RezerwacjaOnline> StaticRezerwacje { get; } = new();

    // ── Dane ──────────────────────────────────────────────────────────────────

    readonly ObservableCollection<RezerwacjaOnline> _wszystkie = new();
    int _naStronie = 15;

    // ── Init ──────────────────────────────────────────────────────────────────

    public RezerwacjeOnlinePage()
    {
        InitializeComponent();
        WierszePicker.SelectedIndex = 0;
        ZaladujPrzykladoweDane();
        OdswiezListe();
    }

    void ZaladujPrzykladoweDane()
    {
        var rez1 = new RezerwacjaOnline
        {
            Slug                = "villaorlowska",
            Grupowanie          = false,
            Czcionka            = "DM Sans",
            PoczatkowyTerminOd  = new DateTime(2024, 8, 5),
            PoczatkowyTerminDo  = new DateTime(2024, 8, 11),
            NazwaPrzedplaty     = "Zadatek",
            OpcjaFaktury        = false,
            Rozliczenie         = "7 dni przed przyjazdem",
            Imie = "Jan", Nazwisko = "Kowalski",
            Email = "jan@example.com", Telefon = "+48 600 100 200",
            DataPrzyjazdu = new DateTime(2024, 8, 5),
            DataWyjazdu   = new DateTime(2024, 8, 11),
            TypPokoju = "Mały pokój 1"  // Mapuj do istniejącej nazwy pokoju
        };
        _wszystkie.Add(rez1);
        StaticRezerwacje.Add(rez1);

        var rez2 = new RezerwacjaOnline
        {
            Slug                = "villaorlowska2",
            Grupowanie          = false,
            Czcionka            = "DM Sans",
            PoczatkowyTerminOd  = new DateTime(2024, 8, 12),
            PoczatkowyTerminDo  = new DateTime(2024, 8, 18),
            NazwaPrzedplaty     = "50%",
            OpcjaFaktury        = true,
            Rozliczenie         = "3 dni przed przyjazdem",
            Imie = "Anna", Nazwisko = "Nowak",
            Email = "anna@example.com", Telefon = "+48 700 200 300",
            DataPrzyjazdu = new DateTime(2024, 8, 12),
            DataWyjazdu   = new DateTime(2024, 8, 18),
            TypPokoju = "Pokój dwuosobowy typu Standard 2"  // Inna rezerwacja
        };
        _wszystkie.Add(rez2);
        StaticRezerwacje.Add(rez2);
    }

    // ── Odświeżanie ───────────────────────────────────────────────────────────

    void OdswiezListe()
    {
        var lista = _wszystkie.Take(_naStronie).ToList();
        RezerwacjeList.ItemsSource = lista;

        int n = _wszystkie.Count;
        int pokazane = Math.Min(_naStronie, n);
        LblLicznik.Text  = n.ToString();
        LblStopka.Text   = n == 0 ? "0 z 0" : $"1–{pokazane} z {n}";
        EmptyState.IsVisible = n == 0;
    }

    void OnWierszeChanged(object sender, EventArgs e)
    {
        _naStronie = WierszePicker.SelectedIndex switch
        {
            1 => 30,
            2 => 50,
            _ => 15
        };
        OdswiezListe();
    }

    // ── Dodaj ─────────────────────────────────────────────────────────────────

    async void OnDodajClicked(object sender, EventArgs e)
    {
        var form = new RezerwacjaOnlineFormPage();
        await Navigation.PushModalAsync(form);
        await form.WaitForResultAsync();

        if (form.Wynik is not null)
        {
            _wszystkie.Add(form.Wynik);
            StaticRezerwacje.Add(form.Wynik);
            OdswiezListe();
        }
    }

    // ── Akcje wiersza ─────────────────────────────────────────────────────────

    async void OnEdytujTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not RezerwacjaOnline rez) return;

        var form = new RezerwacjaOnlineFormPage(rez);
        await Navigation.PushModalAsync(form);
        await form.WaitForResultAsync();

        if (form.Wynik is not null)
        {
            var idx = _wszystkie.IndexOf(rez);
            if (idx >= 0)
            {
                _wszystkie[idx] = form.Wynik;

                // Aktualizuj statyczne repozytorium
                var staticIdx = StaticRezerwacje.IndexOf(rez);
                if (staticIdx >= 0)
                    StaticRezerwacje[staticIdx] = form.Wynik;
            }
            OdswiezListe();
        }
    }

    async void OnUsunTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not RezerwacjaOnline rez) return;

        bool ok = await Shell.Current.DisplayAlert(
            "Usuń rezerwację",
            $"Czy na pewno usunąć „{rez.Slug}?",
            "Usuń", "Anuluj");

        if (!ok) return;
        _wszystkie.Remove(rez);
        StaticRezerwacje.Remove(rez);
        OdswiezListe();
    }
}
