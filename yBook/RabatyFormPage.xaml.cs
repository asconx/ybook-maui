using yBook.Models;

namespace yBook.Views.Rabaty;

public partial class RabatyFormPage : ContentPage
{
    public Rabat Result { get; private set; }

    public RabatyFormPage(Rabat rabat = null)
    {
        InitializeComponent();

        if (rabat != null)
        {
            NazwaEntry.Text = rabat.Nazwa;
            KodEntry.Text = rabat.Kod;
            ProcentEntry.Text = rabat.Procent.ToString();
            OpisEditor.Text = rabat.Opis;
            OnlineCheck.IsChecked = rabat.CzyOnline;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        double.TryParse(ProcentEntry.Text, out double procent);

        Result = new Rabat
        {
            Nazwa = NazwaEntry.Text,
            Kod = KodEntry.Text,
            Procent = (int)procent,
            Opis = OpisEditor.Text,
            CzyOnline = OnlineCheck.IsChecked,
            DataWaznosci = DateTime.Now.AddDays(7)
        };

        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}