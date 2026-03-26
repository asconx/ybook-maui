using Microsoft.Extensions.Logging;
using yBook.Services;
using yBook.ViewModels;
using yBook.Views.Auth;
using yBook.Views.Surveys;

namespace yBook
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── HTTP Client ───────────────────────────────────────────────────
            builder.Services.AddSingleton<HttpClient>();

            // ── Services (Singleton — żyją przez cały czas działania aplikacji)
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISurveyService, SurveyService>();

            // ── Dodaj tę linię:
            builder.Services.AddSingleton<AppShell>();

            // ── ViewModels (Transient — nowa instancja przy każdej nawigacji)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SurveysViewModel>();
            builder.Services.AddTransient<EditSurveyViewModel>();

            // ── Pages ─────────────────────────────────────────────────────────
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SurveysPage>();
            builder.Services.AddTransient<EditSurveyPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
