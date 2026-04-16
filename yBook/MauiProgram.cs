using Microsoft.Extensions.Logging;
using yBook.Services;
using yBook.ViewModels;
using yBook.Views.Auth;
using yBook.Views.Pakiety;
using yBook.Views.Rabaty;
using yBook.Views.Surveys;
using yBook.Views.Ustawienia;

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

            // ── Services ─────────────────────────────────────────────────────
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISurveyService, SurveyService>();
            builder.Services.AddSingleton<IBlockadeService, BlockadeService>();
            builder.Services.AddSingleton<IPanelService, PanelService>();
            builder.Services.AddSingleton<ISmsService, SmsService>(sp => new SmsService(sp.GetRequiredService<IAuthService>(), sp.GetRequiredService<HttpClient>()));

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
            builder.Services.AddTransient<PokojEdycjaPage>();
            builder.Services.AddTransient<SurveysPage>();
            builder.Services.AddTransient<EditSurveyPage>();
            builder.Services.AddTransient<yBook.Views.Kalendarz.KalendarzPage>();
            builder.Services.AddTransient<GrupoweSmsPage>();
            builder.Services.AddTransient<PakietyPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
