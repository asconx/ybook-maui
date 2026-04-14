using Microsoft.Extensions.Logging;
using yBook.Services;
using yBook.ViewModels;
using yBook.Views.Auth;
using yBook.Views.Rabaty;
using yBook.Views.Surveys;
using yBook.Views.Ustawienia;
using CommunityToolkit.Maui;

namespace yBook
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── HTTP Client ───────────────────────────────────────────────────
            // Rejestruj HttpClient jako singleton, aby można było wstrzykiwać go do serwisów
            builder.Services.AddSingleton<HttpClient>();

            // Rejestruj klient API RoomApiService
            builder.Services.AddTransient<Services.Api.RoomApiService>();
            // Rejestruj ActiveReservationService
            builder.Services.AddTransient<Services.Api.ActiveReservationService>();

            // Reception ViewModel can be requested by pages
            builder.Services.AddTransient<ViewModels.ReceptionViewModel>();

            // ── Services ─────────────────────────────────────────────────────
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISurveyService, SurveyService>();
            builder.Services.AddSingleton<IPanelService, PanelService>();

            // ── Shell ─────────────────────────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

            // ── ViewModels ────────────────────────────────────────────────────
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SurveysViewModel>();
            builder.Services.AddTransient<EditSurveyViewModel>();
            builder.Services.AddTransient<RoomsViewModel>();

            // ── Pages ─────────────────────────────────────────────────────────
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<RabatyPage>();
            builder.Services.AddTransient<PokojePage>();
            builder.Services.AddTransient<PokojEdycjaPage>();
            builder.Services.AddTransient<SurveysPage>();
            builder.Services.AddTransient<EditSurveyPage>();
            builder.Services.AddTransient<yBook.Views.Kalendarz.KalendarzPage>();
            // Recepcja page (booking list)
            builder.Services.AddTransient<yBook.Views.RecepcjaPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // set service provider for ServiceLocator
            Services.ServiceLocator.ServiceProvider = app.Services;

            return app;
        }
    }
}
