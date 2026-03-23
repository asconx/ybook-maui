using yBook.Models;

namespace yBook.Views.Rabaty;

public partial class RabatyPage : ContentPage
{
    List<Rabat> rabaty = new();

    public RabatyPage()
    {
        InitializeComponent();

        // przykładowe dane
        rabaty.Add(new Rabat
        {
            Nazwa = "Zniżka Allegro",
            Kod = "ALLEGRO10",
            Opis = "10% na elektronikę",
            Procent = 10,
            DataWaznosci = DateTime.Now.AddDays(7)
        });

        RabatyList.ItemsSource = rabaty;
    }

    private void OnDodajRabatClicked(object sender, EventArgs e)
    {
        DisplayAlert("Info", "Dodawanie rabatu (do zrobienia)", "OK");
    }
}