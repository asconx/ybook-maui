using Microsoft.Extensions.Logging;
using yBook.Application.Ports;
using yBook.Application.UseCases;
using yBook.Infrastructure.Repositories;
using yBook.Services;
using yBook.ViewModels;
using yBook.Views.Auth;
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
            builder.Services.AddSingleton<IRoomPhotoService, RoomPhotoService>();
            builder.Services.AddSingleton<IAuthenticatedImageService, AuthenticatedImageService>();
            builder.Services.AddSingleton<IAuthRepository, AuthRepositoryAdapter>();
            builder.Services.AddSingleton<IRoomRepository, ApiRoomRepository>();
            builder.Services.AddSingleton<IRoomPhotoRepository, ApiRoomPhotoRepository>();
            builder.Services.AddSingleton<IRoomBedRepository, ApiRoomBedRepository>();
            builder.Services.AddSingleton<IPropertyRepository, ApiPropertyRepository>();
            builder.Services.AddSingleton<IRoomPriceModifierRepository, ApiRoomPriceModifierRepository>();
            builder.Services.AddSingleton<IFileRepository, ApiFileRepository>();
            builder.Services.AddTransient<LoginUserUseCase>();
            builder.Services.AddTransient<GetRoomsUseCase>();
            builder.Services.AddTransient<GetRoomPhotoUseCase>();
            builder.Services.AddTransient<GetRoomBedsUseCase>();
            builder.Services.AddTransient<GetPropertiesUseCase>();
            builder.Services.AddTransient<GetRoomPriceModifiersUseCase>();
            builder.Services.AddTransient<DownloadRoomImageUseCase>();
            builder.Services.AddTransient<GetRoomsWithImagesUseCase>();

            // ── Shell ─────────────────────────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

            // ── ViewModels ────────────────────────────────────────────────────
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SurveysViewModel>();
            builder.Services.AddTransient<EditSurveyViewModel>();
            builder.Services.AddTransient<PokojeViewModel>();

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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
