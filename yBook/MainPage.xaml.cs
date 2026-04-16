namespace yBook
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            SetGreeting();

            // Podpięcie hamburgera z headera do DrawerMenu
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

        void SetGreeting()
        {
            var hour = DateTime.Now.Hour;
            LblGreeting.Text = hour switch
            {
                < 12 => "Dzień dobry! 👋",
                < 18 => "Witaj ponownie! 👋",
                _    => "Dobry wieczór! 🌙"
            };
            LblDate.Text = DateTime.Now.ToString("dddd, d MMMM yyyy",
                new System.Globalization.CultureInfo("pl-PL"));
        }

        async void OnQuickAction(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string page) return;

            switch (page)
            {
                case "Recepcja":
                    await Shell.Current.GoToAsync("Recepcja");
                    break;

                case "RejestrPlatnosci":
                case "Dokumenty":
                case "KontaFinansowe":
                case "ImportMT940":
                    await Shell.Current.GoToAsync(page);
                    break;

                case "Ankiety":
                    await Shell.Current.GoToAsync("SurveysPage");
                    break;

                default:
                    await DisplayAlert("yBook", $"Wkrotce: {page}", "OK");
                    break;
            }
        }

        void RefreshDashboard()
        {
            LblArrivals.Text   = "–";
            LblDepartures.Text = "–";
            LblOccupancy.Text  = "–";
            LblRevenue.Text    = "–";
        }
    }
}
