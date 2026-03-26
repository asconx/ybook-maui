using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Views.Blokady;

public partial class BlokadyPage : ContentPage
{
    ObservableCollection<Blokada> blokady = new();

    public BlokadyPage()
    {
        InitializeComponent();

        BlokadyList.ItemsSource = blokady;
    }

    async void OnDodajClicked(object sender, EventArgs e)
    {
        var page = new BlokadyFormPage();
        await Navigation.PushModalAsync(page);

        page.Disappearing += (s, ev) =>
        {
            if (page.Result != null)
                blokady.Add(page.Result);
        };
    }

    async void OnBlokadaTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var blokada = frame?.BindingContext as Blokada;
        if (blokada == null) return;

        string action = await DisplayActionSheet(
            blokada.Nazwa,
            "Anuluj",
            null,
            "Edytuj",
            "Usuń");

        if (action == "Usuń")
            blokady.Remove(blokada);

        if (action == "Edytuj")
        {
            var page = new BlokadyFormPage(blokada);
            await Navigation.PushModalAsync(page);

            page.Disappearing += (s, ev) =>
            {
                if (page.Result != null)
                {
                    blokada.Nazwa = page.Result.Nazwa;
                    blokada.Notatka = page.Result.Notatka;
                    blokada.DataOd = page.Result.DataOd;
                    blokada.DataDo = page.Result.DataDo;
                    blokada.DlaWszystkich = page.Result.DlaWszystkich;
                    blokada.Pokoje = page.Result.Pokoje;

                    BlokadyList.ItemsSource = null;
                    BlokadyList.ItemsSource = blokady;
                }
            };
        }
    }
}