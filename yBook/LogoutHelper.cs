// ── Jak wylogować z DrawerMenu.xaml.cs ───────────────────────────────────────
//
// 1. Wstrzyknij IAuthService przez DI (DrawerMenu musi być zarejestrowane w DI)
//    LUB pobierz z ServiceProvider jeśli DrawerMenu jest tworzony jako ContentView w XAML.
//
// Opcja A — przez Handler/ServiceProvider (najprostsza dla ContentView):

using yBook.Services;

namespace yBook.Controls
{
    public partial class DrawerMenu : ContentView
    {
        // Dodaj tę metodę do istniejącego DrawerMenu.xaml.cs

        private async void OnLogoutTapped(object? sender, TappedEventArgs e)
        {
            var confirm = await Application.Current!.Windows[0].Page!
                .DisplayAlert("Wylogowanie", "Czy na pewno chcesz się wylogować?", "Tak", "Anuluj");

            if (!confirm) return;

            // Pobierz AuthService z DI
            var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
            await auth.LogoutAsync();

            // Wróć do ekranu logowania
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}

// ── Jak dodać przycisk wylogowania w DrawerMenu.xaml ─────────────────────────
//
// Wyszukaj sekcję z profilem użytkownika lub dolnym menu i dodaj:
//
// <TapGestureRecognizer Tapped="OnLogoutTapped"/>
//
// Przykład przycisku wylogowania:
//
// <Border Margin="16,8" Padding="14,12" BackgroundColor="#FEF2F2"
//         StrokeShape="RoundRectangle 10" StrokeThickness="0">
//     <Grid ColumnDefinitions="Auto,*">
//         <Label Text="🚪" FontSize="18" VerticalOptions="Center"/>
//         <Label Grid.Column="1" Text="Wyloguj się" FontSize="14"
//                TextColor="#DC2626" VerticalOptions="Center" Margin="12,0,0,0"/>
//     </Grid>
//     <Border.GestureRecognizers>
//         <TapGestureRecognizer Tapped="OnLogoutTapped"/>
//     </Border.GestureRecognizers>
// </Border>
