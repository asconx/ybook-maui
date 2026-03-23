using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Views.Rabaty;

public partial class RabatyPage : ContentPage
{
    ObservableCollection<Rabat> rabaty = new();

    public RabatyPage()
    {
        InitializeComponent();

        rabaty.Add(new Rabat
        {
            Nazwa = "Zniżka Allegro",
            Kod = "ALLEGRO10",
            Opis = "10% na elektronikę",
            Procent = 10,
            CzyOnline = true,
            DataWaznosci = DateTime.Now.AddDays(7)
        });

        RabatyList.ItemsSource = rabaty;
    }

    // ➕ DODAWANIE
    async void OnDodajRabatClicked(object sender, EventArgs e)
    {
        var page = new RabatyFormPage();
        await Navigation.PushModalAsync(page);

        page.Disappearing += (s, ev) =>
        {
            if (page.Result != null)
                rabaty.Add(page.Result);
        };
    }

    // 👆 TAP → MENU
    async void OnRabatTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var rabat = frame?.BindingContext as Rabat;
        if (rabat == null) return;

        string action = await DisplayActionSheet(
            rabat.Nazwa,
            "Anuluj",
            null,
            "Edytuj",
            "Usuń");

        if (action == "Usuń")
        {
            bool ok = await DisplayAlert("Usuń", "Na pewno usunąć?", "Tak", "Nie");
            if (ok)
                rabaty.Remove(rabat);
        }

        if (action == "Edytuj")
        {
            var page = new RabatyFormPage(rabat);
            await Navigation.PushModalAsync(page);

            page.Disappearing += (s, ev) =>
            {
                if (page.Result != null)
                {
                    rabat.Nazwa = page.Result.Nazwa;
                    rabat.Kod = page.Result.Kod;
                    rabat.Procent = page.Result.Procent;
                    rabat.Opis = page.Result.Opis;
                    rabat.CzyOnline = page.Result.CzyOnline;

                    // odśwież UI
                    RabatyList.ItemsSource = null;
                    RabatyList.ItemsSource = rabaty;
                }
            };
        }
    }
}