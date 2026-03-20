namespace yBook;

public partial class StatusPage : ContentPage
{
    public StatusPage()
    {
        InitializeComponent();
    }

    // Przycisk "Dodaj status"
    private async void OnAddStatusClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Nowy status", "Tu otworzy siê formularz dodawania statusu.", "OK");
    }

    // Przycisk "Edytuj"
    private async void OnEditStatusClicked(object sender, EventArgs e)
    {
        var btn = sender as ImageButton;
        var nazwa = btn?.CommandParameter?.ToString();
        await DisplayAlert("Edytuj status", $"Edytujesz status: {nazwa}", "OK");
    }

    // Przycisk "Usuñ"
    private async void OnDeleteStatusClicked(object sender, EventArgs e)
    {
        var btn = sender as ImageButton;
        var nazwa = btn?.CommandParameter?.ToString();

        bool confirm = await DisplayAlert("Usuñ status",
            $"Czy na pewno chcesz usun¹æ status '{nazwa}'?",
            "Usuñ", "Anuluj");

        if (confirm)
        {
            await DisplayAlert("Usuniêto", $"Status '{nazwa}' zosta³ usuniêty.", "OK");
        }
    }
}