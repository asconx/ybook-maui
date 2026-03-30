namespace Powiadomienia;

public partial class PowiadomieniaPush : ContentPage
{
    ObservableCollection<Notification> notif = new ObeservableCollection();

    public PowiadomieniaPush()
    {
        InitializeComponent();
    }

    async void ShowNotifForm(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ProductPage());
    }

    public void DeleteNotif(object sender, EventArgs e)
    {

    }
    public void EditNotif(object sender, EventArgs e)
    {

    }
}
