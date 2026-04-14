using yBook.ViewModels;

namespace yBook.Views.Ustawienia;

public partial class StatusyPage : ContentPage
{
    public StatusyPage()
    {
        InitializeComponent();
        BindingContext = new StatusyViewModel();   // ← jedyna zmiana
    }
}