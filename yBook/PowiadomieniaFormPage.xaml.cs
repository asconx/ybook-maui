using yBook.Models;

namespace yBook.Views.Powiadomienia;

public partial class PowiadomieniaFormPage : ContentPage
{
    

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}