using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using yBook.Helpers;
using yBook.Models;
using static yBook.Views.Przyjazdy.PrzyjazdWyjazdPage;

namespace yBook.Views.Ustawienia;

public partial class PowiadomieniaFormPage : ContentPage
{
    List<string> Opisy = new List<string>{
        "Powiadomienie dla zapisu nowej rezerwacji",
        "Powiadomienie dla zapisu nowej rezerwacji",
        "Powiadomienie dla zapisu nowej rezerwacji",
        "Powiadomienie dla zapisu edycji istniejącej rezerwacji",
        "Powiadomienie dla zapisu nowej rezerwacji",
        "Powiadomienie dla zapisu edycji istniejącej rezerwacji",
        "Powiadomienie wysyłane po przyjeździe",
        "Powiadomienie wysyłane w związku z planowanym usunięciem rezerwacji",
        "Powiadomienie wysyłane w związku ze zmianą kwatery lub terminu",
        "Niestandardowe tekstowe powiadomienie",
        "Powiadomienie wysyłane przed przyjazdem",
        "Powiadomienie wysyłane po wyjeździe"};


    public PowiadomieniaFormPage()
    {
        InitializeComponent();
  
    }
    private void ShowEntry(object sender, ToggledEventArgs e) => PowNiest.IsVisible = NiestanPow.IsToggled;

    
    async void OnSaveClicked(object sender, EventArgs e)
    {
        PowiadomieniaPage obj = new PowiadomieniaPage();
        string tempOpis;

        if ((NazwaEntry.Text == string.Empty && (TypPow.SelectedIndex == -1 && TypPow.SelectedIndex == 0 )) && (PowiMail.IsChecked == true || PowiSMS.IsChecked == true)) {
            await DisplayAlertAsync("Błąd", "Nie podano prawidłowych danych!", "OK");
        } else
        {
            if (NiestanPow.IsToggled)
            {
                if (PowNiest.Text != string.Empty && PowNiest.Text != "")
                {
                    tempOpis = PowNiest.Text;
                    obj.Powiadomienia.Add( new PowiadomienieItem
                    {
                        Nazwa = NazwaEntry.Text,
                        Typ = TypPow.ItemsSource[TypPow.SelectedIndex].ToString(),
                        Opis = tempOpis,
                        PowMail = PowiMail.IsChecked,
                        PowSMS = PowiSMS.IsChecked
                    });
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlertAsync("Błąd", "Opis nie został wpisany!", "OK");
                }
            }
            else
            {
                obj.Powiadomienia.Add(new PowiadomienieItem
                {
                    Nazwa = NazwaEntry.Text,
                    Typ = TypPow.ItemsSource[TypPow.SelectedIndex].ToString(),
                    Opis = Opisy[TypPow.SelectedIndex],
                    PowMail = PowiMail.IsChecked,
                    PowSMS = PowiSMS.IsChecked
                });
                await Navigation.PopAsync();
            }
        }
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();

    }
}