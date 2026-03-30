using yBook.Models;

namespace yBook.Views.Blokady;

public partial class BlokadyFormPage : ContentPage
{
    public Blokada Result { get; private set; }

    List<string> wszystkiePokoje = new()
    {
        "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"
    };

    List<string> wybrane = new();

    public BlokadyFormPage(Blokada blokada = null)
    {
        InitializeComponent();

        DataOdPicker.Date = DateTime.Now;
        DataDoPicker.Date = DateTime.Now.AddDays(1);

        GenerujPokoje();

        if (blokada != null)
        {
            NazwaEntry.Text = blokada.Nazwa;
            NotatkaEditor.Text = blokada.Notatka;
            DataOdPicker.Date = blokada.DataOd;
            DataDoPicker.Date = blokada.DataDo;
            WszystkieCheck.IsChecked = blokada.DlaWszystkich;
            wybrane = new List<string>(blokada.Pokoje);
        }
    }

    void GenerujPokoje()
    {
        foreach (var pok in wszystkiePokoje)
        {
            var btn = new Button
            {
                Text = pok,
                CornerRadius = 20,
                BackgroundColor = Colors.LightGray,
                Margin = 4
            };

            btn.Clicked += (s, e) =>
            {
                if (wybrane.Contains(pok))
                {
                    wybrane.Remove(pok);
                    btn.BackgroundColor = Colors.LightGray;
                }
                else
                {
                    wybrane.Add(pok);
                    btn.BackgroundColor = Colors.LightGreen;
                }
            };

            PokojeContainer.Children.Add(btn);
        }
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        Result = new Blokada
        {
            Nazwa = NazwaEntry.Text,
            Notatka = NotatkaEditor.Text,
            DataOd = DataOdPicker.Date,
            DataDo = DataDoPicker.Date,
            DlaWszystkich = WszystkieCheck.IsChecked,
            Pokoje = wybrane
        };

        await Navigation.PopModalAsync();
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}