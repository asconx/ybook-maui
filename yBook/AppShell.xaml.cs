using yBook.Services;
using yBook.Views.Blokady;
using yBook.Views.Ceny;
using yBook.Views.Finanse;
using yBook.Views.ICalendar;
using yBook.Views.Klienci;
using yBook.Views.Przyjazdy;
using yBook.Views.Rabaty;
using yBook.Views.Surveys;
using yBook.Views.Uzytkownicy;

namespace yBook
{
    public partial class AppShell : Shell
    {
        private readonly IAuthService _auth;

        public AppShell(IAuthService auth)
        {
            _auth = auth;
            InitializeComponent();
            RegisterRoutes();
            Loaded += OnShellLoaded;
        }

        private async void OnShellLoaded(object? sender, EventArgs e)
        {
            var isLoggedIn = await _auth.IsAuthenticatedAsync();

            if (isLoggedIn)
                await GoToAsync("//MainPage");
            else
                await GoToAsync("//LoginPage");
        }

        private void RegisterRoutes()
        {
            // Finanse
            Routing.RegisterRoute("Dokumenty",        typeof(Views.Finanse.DokumentyPage));
            Routing.RegisterRoute("KontaFinansowe",   typeof(Views.Finanse.KontaFinansowePage));
            Routing.RegisterRoute("RejestrPlatnosci", typeof(Views.Finanse.RejestrPlatnosciPage));
            Routing.RegisterRoute("ImportMT940",      typeof(Views.Finanse.ImportMT940Page));

            // Inne
            Routing.RegisterRoute("ICalendar",          typeof(ICalendarPage));
            Routing.RegisterRoute("UslugiOplaty",       typeof(UslugiOplaty));
            Routing.RegisterRoute("Cenniki",            typeof(CennikPage));
            Routing.RegisterRoute("BlokadyPage",        typeof(BlokadyPage));
            Routing.RegisterRoute("PrzyjazdWyjazdPage", typeof(PrzyjazdWyjazdPage));

            // Klienci
            Routing.RegisterRoute("Klienci", typeof(Views.Klienci.KlienciPage));

            // Użytkownicy
            Routing.RegisterRoute("UzytkownicyLista", typeof(Uzytkownicy1Page));

            // Surveys
            Routing.RegisterRoute("SurveysPage",    typeof(SurveysPage));
            Routing.RegisterRoute("EditSurveyPage", typeof(EditSurveyPage));
        }
    }
}
