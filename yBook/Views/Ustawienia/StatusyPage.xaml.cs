using yBook.ViewModels;

namespace yBook.Views.Ustawienia;

public partial class StatusyPage : ContentPage
{
    public StatusyPage()
    {
        InitializeComponent();
        BindingContext = new StatusyViewModel();   // ← jedyna zmiana
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is StatusyViewModel vm)
        {
            // Fire-and-await loading once; ViewModel will guard IsBusy to avoid double requests
            await vm.LoadStatusesAsync();
        }
    }
}