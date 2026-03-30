using yBook.Views.Blokady;
using yBook.Views.Ceny;
using yBook.Services;
using yBook.Views.Finanse;
using yBook.Views.ICalendar;
using yBook.Views.Klienci;
using yBook.Views.Przyjazdy;
using yBook.Views.Uzytkownicy;
using yBook.Views.Rabaty;

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

            // Sprawdź sesję asynchronicznie po załadowaniu Shell
            Loaded += OnShellLoaded;
        }

        // ── Sprawdzenie sesji przy starcie ─────────────────────────────────────

        private async void OnShellLoaded(object? sender, EventArgs e)
        {
            var isLoggedIn = await _auth.IsAuthenticatedAsync();

            if (isLoggedIn)
                await GoToAsync("//MainPage");
            else
                await GoToAsync("//LoginPage");
        }

        // ── Rejestracja tras ───────────────────────────────────────────────────

        private void RegisterRoutes()
        {
            // Auth
            Routing.RegisterRoute("LoginPage", typeof(Views.Auth.LoginPage));

            // Finanse
            Routing.RegisterRoute("Dokumenty", typeof(DokumentyPage));
            Routing.RegisterRoute("KontaFinansowe", typeof(KontaFinansowePage));
            Routing.RegisterRoute("RejestrPlatnosci", typeof(RejestrPlatnosciPage));
            Routing.RegisterRoute("ImportMT940", typeof(ImportMT940Page));

            // Inne
            Routing.RegisterRoute("ICalendar", typeof(ICalendarPage));
            Routing.RegisterRoute("UslugiOplaty", typeof(UslugiOplaty));
            Routing.RegisterRoute("Cenniki", typeof(CennikPage));
            Routing.RegisterRoute("RabatyPage", typeof(RabatyPage));
            Routing.RegisterRoute("BlokadyPage", typeof(BlokadyPage));
            Routing.RegisterRoute("PrzyjazdWyjazdPage", typeof(PrzyjazdWyjazdPage));

            // Klienci — zarejestruj nazwę zgodną z nameof(KlienciPage) używanym w GoToAsync
            Routing.RegisterRoute("KlienciPage", typeof(KlienciPage));

            // Użytkownicy
            Routing.RegisterRoute("UzytkownicyLista", typeof(Uzytkownicy1Page));
        }
    }
}
