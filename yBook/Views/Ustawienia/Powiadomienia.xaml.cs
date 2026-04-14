using System.Collections.ObjectModel;
using yBook.Models;
using yBook.Views.Blokady;
using yBook.Views.Rabaty;

namespace yBook.Views.Ustawienia;

public partial class PowiadomieniaPage : ContentPage
{
    public ObservableCollection<PowiadomienieItem> Powiadomienia = new ObservableCollection<PowiadomienieItem>();

    public PowiadomieniaPage()
    {
        InitializeComponent();

        amountList.Text = Powiadomienia.Count.ToString();
        Powiadomienia = new ObservableCollection<PowiadomienieItem>
        {
            new() {
                Nazwa = "Rezerwacja wstępna",
                Typ = "Rezerwacja wstępna",
                Opis = "Powiadomienie dla zapisu nowej rezerwacji.",
                PowMail = true,
                PowSMS = true
            },
            new() {
                Nazwa = "Oferta",
                Typ = "Oferta z przekierowaniem do płatności",
                Opis = "Powiadomienie dla zapisu nowej rezerwacji.",
                PowMail = true,
                PowSMS = true
            },
            new() {
                Nazwa = "Potwierdzenie",
                Typ = "Potwierdzenie wpłaty",
                Opis = "Powiadomienie dla zapisu edycji istniejącej rezerwacji.",
                PowMail = true,
                PowSMS = true
            },
            new() {
                Nazwa = "Rezerwacja rozliczona",
                Typ = "Rozliczenie rezerwacji",
                Opis = "Powiadomienie dla zapisu edycji istniejącej rezerwacji.",
                PowMail = true,
                PowSMS = false
            },
            new() {
                Nazwa = "Anulowanie rezerwacji",
                Typ = "Anulowanie rezerwacji",
                Opis = "Powiadomienie związane z usunięciem rezerwacji.",
                PowMail = true,
                PowSMS = false
            }
        };

        ListaPowiadomien.ItemsSource = Powiadomienia;
    }

    async void Dodaj(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PowiadomieniaFormPage());
    }

    private void Edyt(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var powiad = frame?.BindingContext as PowiadomienieItem;
        var page = new PowiadomieniaFormPage();
        
    }

    private void Usun(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var powiad = frame?.BindingContext as PowiadomienieItem;
        if (powiad == null) return;
        Powiadomienia.Remove(powiad);
    }
}

// KLASA POWIADOMIEŃ - przechowuje informacje (nazwa, typ, opis i jakie mają być). W zależności czy jest mail czy sms lub oba powinno wyświetlać przycisk
public class PowiadomienieItem
{
    public string Nazwa { get; set; }
    public string Typ { get; set; }
    public string Opis { get; set; }
    public bool PowMail { get; set; }
    public bool PowSMS { get; set; }
}