using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Views.Blokady;

public partial class RezerwacjeOnlinePage : ContentPage
{
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
        _wszystkie.Add(new RezerwacjaOnline
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
        });
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
            if (idx >= 0) _wszystkie[idx] = form.Wynik;
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
        OdswiezListe();
    }
}
