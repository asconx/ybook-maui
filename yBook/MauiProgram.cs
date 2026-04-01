using Microsoft.Extensions.Logging;
using yBook.Services;
using yBook.ViewModels;
using yBook.Views.Auth;
using yBook.Views.Rabaty;
using yBook.Views.Surveys;
using yBook.Views.Ustawienia;
using yBook.Views.Ceny;

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
            // General HttpClient for other services
            builder.Services.AddSingleton<HttpClient>();

            // Register IPriceService with its own HttpClient instance (avoid relying on AddHttpClient extension)
            builder.Services.AddSingleton<IPriceService>(sp =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://api.ybook.pl"),
                    Timeout = TimeSpan.FromSeconds(30)
                };
                return new PriceService(client);
            });

            // ── Services ─────────────────────────────────────────────────────
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISurveyService, SurveyService>();
            // IPriceService registered above

            // ── Shell ─────────────────────────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

            // ── ViewModels ────────────────────────────────────────────────────
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SurveysViewModel>();
            builder.Services.AddTransient<EditSurveyViewModel>();

            // ── Pages ─────────────────────────────────────────────────────────
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<RabatyPage>();
            builder.Services.AddTransient<PokojePage>();
            builder.Services.AddTransient<SurveysPage>();
            builder.Services.AddTransient<EditSurveyPage>();
            builder.Services.AddTransient<CennikiListaPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
