using yBook.Views.Blokady;
using yBook.Views.Ceny;
using yBook.Services;
using yBook.Views.Finanse;
using yBook.Views.ICalendar;
using yBook.Views.Klienci;
using yBook.Views.Przyjazdy;
using yBook.Views.Rabaty;
using yBook.Views.Surveys;
using yBook.Views.Ustawienia;
using yBook.Views.Kalendarz;
using yBook.Views.Uzytkownicy;
using yBook.Views.Rabaty;
using yBook.Views.Raporty;
using yBook.Views.Przyjazdy;

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
            // Synchronizacja dostępności przy starcie
            Loaded += async (s, e) =>
            {
                // Przykład: automatyczna synchronizacja przy starcie
                // Możesz wywołać synchronizację globalnie lub tylko dla wybranych stron
                // await new PrzyjazdWyjazdPage().SyncFromApi();
            };
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

			// ceny
			Routing.RegisterRoute("UslugiOplaty", typeof(UslugiOplaty));
			Routing.RegisterRoute("UslugiOplatyDodawanie", typeof(UslugiOplatyDodawanie)); //UslugiOplaty helper page

			// Inne
			Routing.RegisterRoute("ICalendar", typeof(ICalendarPage));
            Routing.RegisterRoute("Cenniki", typeof(CennikPage));
            Routing.RegisterRoute("RabatyPage", typeof(RabatyPage));
            Routing.RegisterRoute("BlokadyPage", typeof(BlokadyPage));
            Routing.RegisterRoute("PrzyjazdWyjazdPage", typeof(PrzyjazdWyjazdPage));
            Routing.RegisterRoute("GrupoweSms", typeof(GrupoweSmsPage));

            //Powiadomienia
            Routing.RegisterRoute(nameof(PowiadomieniaPage), typeof(PowiadomieniaPage));

            //Statusy
            Routing.RegisterRoute(nameof(StatusyPage), typeof(StatusyPage));

            // Klienci
            Routing.RegisterRoute("Klienci", typeof(Views.Klienci.KlienciPage));
            // Klienci — zarejestruj nazwę zgodną z nameof(KlienciPage) używanym w GoToAsync
            Routing.RegisterRoute("KlienciPage", typeof(KlienciPage));

            // Użytkownicy
            Routing.RegisterRoute("UzytkownicyLista", typeof(Uzytkownicy1Page));

            // Kalendarz
            Routing.RegisterRoute("KalendarzPage", typeof(Views.Kalendarz.KalendarzPage));

            // Surveys
            Routing.RegisterRoute("SurveysPage",    typeof(SurveysPage));
            Routing.RegisterRoute("EditSurveyPage", typeof(EditSurveyPage));


            // Raporty
            Routing.RegisterRoute("ListaLogow", typeof(ListaLogowPage));

        }
    }
}
