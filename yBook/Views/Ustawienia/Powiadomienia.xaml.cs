using System.Collections.ObjectModel;

namespace yBook.Views.Ustawienia;

public partial class PowiadomieniaPage : ContentPage
{
    public ObservableCollection<PowiadomienieItem> Items { get; set; }

    public PowiadomieniaPage()
    {
        InitializeComponent();

        Items = new ObservableCollection<PowiadomienieItem>
        {
            new() {
                Nazwa = "Rezerwacja wstępna",
                Typ = "Rezerwacja wstępna",
                Opis = "Powiadomienie dla zapisu nowej rezerwacji."
            },
            new() {
                Nazwa = "Oferta",
                Typ = "Oferta z przekierowaniem do płatności",
                Opis = "Powiadomienie dla zapisu nowej rezerwacji."
            },
            new() {
                Nazwa = "Potwierdzenie",
                Typ = "Potwierdzenie wpłaty",
                Opis = "Powiadomienie dla zapisu edycji istniejącej rezerwacji."
            },
            new() {
                Nazwa = "Rezerwacja rozliczona",
                Typ = "Rozliczenie rezerwacji",
                Opis = "Powiadomienie dla zapisu edycji istniejącej rezerwacji."
            },
            new() {
                Nazwa = "Anulowanie rezerwacji",
                Typ = "Anulowanie rezerwacji",
                Opis = "Powiadomienie związane z usunięciem rezerwacji."
            }
        };

        ListaPowiadomien.ItemsSource = Items;
    }
}

public class PowiadomienieItem
{
    public string Nazwa { get; set; }
    public string Typ { get; set; }
    public string Opis { get; set; }
}