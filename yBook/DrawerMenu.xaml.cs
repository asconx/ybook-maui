using yBook.Views.Ceny;
using yBook.Views.Klienci;

using yBook;
using yBook.Views.Ustawienia;
using yBook.Services;

namespace yBook.Controls
{
    public partial class DrawerMenu : ContentView
    {
        const double DrawerWidth = 290;
        private const string V = "Uzytkownicy";
        bool _isOpen = false;
        readonly Dictionary<string, bool> _groupState = new()
            {
                { "Ceny",       false },
                { "Finanse",    false },
                { "Raporty",    false },
                { "Blokady",    false },
                { "Ustawienia", false },
            };

        // Auth service for parsing user data
        private IAuthService? _authService;

        public Action<object, object> HamburgerClicked { get; internal set; }



        public DrawerMenu()
        {
            InitializeComponent();
            InitializeAuthService();
        }

        // ── Initialize Auth Service ──────────────────────────────────────────

        private void InitializeAuthService()
        {
            try
            {
                _authService = IPlatformApplication.Current?.Services
                    .GetService<IAuthService>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawerMenu] Failed to initialize auth service: {ex.Message}");
            }
        }

        // ── Load User Data ────────────────────────────────────────────────────

        public async Task LoadUserDataAsync()
        {
            try
            {
                if (_authService == null)
                {
                    System.Diagnostics.Debug.WriteLine("[DrawerMenu] Auth service is not available");
                    return;
                }

                // Check if user is authenticated
                var isAuthenticated = await _authService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    System.Diagnostics.Debug.WriteLine("[DrawerMenu] User is not authenticated");
                    return;
                }

                // Get authentication token
                var token = await _authService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("[DrawerMenu] No authentication token available");
                    return;
                }

                // Get user profile from secure storage
                var userEmail = await SecureStorage.Default.GetAsync("user_email");
                var userName = await SecureStorage.Default.GetAsync("user_name");
                var organizationId = await SecureStorage.Default.GetAsync("organization_id");

                System.Diagnostics.Debug.WriteLine($"[DrawerMenu] Loaded user data:");
                System.Diagnostics.Debug.WriteLine($"  Email: {userEmail}");
                System.Diagnostics.Debug.WriteLine($"  Name: {userName}");
                System.Diagnostics.Debug.WriteLine($"  Organization ID: {organizationId}");

                // Update UI with user information if needed
                await UpdateUserUIAsync(userName, userEmail, organizationId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawerMenu] Error loading user data: {ex.Message}");
            }
        }

        // ── Update User UI ────────────────────────────────────────────────────

        private async Task UpdateUserUIAsync(string? userName, string? userEmail, string? organizationId)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Update drawer header or other UI elements with user data
                // This is a placeholder - adjust based on your actual XAML structure
                if (!string.IsNullOrEmpty(userName))
                {
                    System.Diagnostics.Debug.WriteLine($"[DrawerMenu] Updating UI with user: {userName}");
                    // Example: YourUserNameLabel.Text = userName;
                }
            });
        }

        // ── Publiczne API ──────────────────────────────────────────────────────

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            DrawerRoot.IsVisible = true;
            DrawerOverlay.Opacity = 0;
            DrawerPanel.TranslationX = -DrawerWidth;

            var anim = new Animation();
            anim.Add(0, 1, new Animation(v => DrawerPanel.TranslationX = v,
                                         -DrawerWidth, 0, Easing.CubicOut));
            anim.Add(0, 1, new Animation(v => DrawerOverlay.Opacity = v,
                                         0, 1, Easing.Linear));
            anim.Commit(this, "Open", length: 260);

            // Load user data when drawer opens (fire and forget safely)
            MainThread.BeginInvokeOnMainThread(async () => await LoadUserDataAsync());
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            var anim = new Animation();
            anim.Add(0, 1, new Animation(v => DrawerPanel.TranslationX = v,
                                         0, -DrawerWidth, Easing.CubicIn));
            anim.Add(0, 1, new Animation(v => DrawerOverlay.Opacity = v,
                                         1, 0, Easing.Linear));
            anim.Commit(this, "Close", length: 220,
                        finished: (_, _) => DrawerRoot.IsVisible = false);
        }

        // ── Overlay tap ───────────────────────────────────────────────────────

        void OnOverlayTapped(object? sender, TappedEventArgs e) => Close();

        // ── Rozwijanie grup ───────────────────────────────────────────────────

        async void OnGroupToggle(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string group) return;
            _groupState[group] = !_groupState[group];
            bool open = _groupState[group];

            var (submenu, arrow) = group switch
            {
                "Ceny" => (MenuCenyGroup, LblCenyArrow),
                "Finanse" => (MenuFinanseGroup, LblFinanseArrow),
                "Raporty" => (MenuRaportyGroup, LblRaportyArrow),
                "Blokady" => (MenuBlokadyGroup, LblBlokadyArrow),
                "Ustawienia" => (MenuUstawieniaGroup, LblUstawieniaArrow),
                _ => (null, null)
            };
            if (submenu is null || arrow is null) return;

            await arrow.RotateTo(open ? 90 : 0, 180, Easing.CubicOut);

            if (open)
            {
                submenu.IsVisible = true;
                submenu.Opacity = 0;
                await submenu.FadeTo(1, 160);
            }
            else
            {
                await submenu.FadeTo(0, 130);
                submenu.IsVisible = false;
            }
        }

        // ── Nawigacja ─────────────────────────────────────────────────────────

        async void OnItemTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string page) return;
            Close();

            switch (page)
            {
                // ── Strony finansów — gotowe ──────────────────────────────────
                case "UslugiOplaty":
                    await Shell.Current.GoToAsync("UslugiOplaty");
                    break;
                case "Dokumenty":
                case "KontaFinansowe":
                case "RejestrPlatnosci":
                case "ImportMT940":
                case "Kalendarz":
                    await Shell.Current.GoToAsync("KalendarzPage");
                    break;

                case "ICalendar":
                    await Shell.Current.GoToAsync(page);
                    break;
                case "Cenniki":
                    await Shell.Current.GoToAsync(page);
                    break;
                // —— Strona Rabaty —————————————————————————————————————————————
                case "Rabaty":
                    await Shell.Current.GoToAsync("//RabatyPage");
                    break;
                case "DaneObiektu":
                    await Shell.Current.GoToAsync("//DaneObiektu");
                    break;

                // —— Strona Pokoje —————————————————————————————————————————————
                case "Pokoje":
                    await Shell.Current.GoToAsync("//PokojePage");
                    break;

                // —— Strona Użytkownicy i Role —————————————————————————————————
                case "UzytkownicyIRolePage":
                    await Shell.Current.GoToAsync("UzytkownicyIRolePage");
                    break;

                // —— Strona Powiadomienia —————————————————————————————————————————————
                case "Powiadomienia":
                    await Shell.Current.GoToAsync(nameof(PowiadomieniaPage));
                    break;
                // —— Strona Statusy —————————————————————————————————————————————
                case "Statusy":
                    await Shell.Current.GoToAsync(nameof(StatusyPage));
                    break;

                case "Uzytkownicy":
                    await Shell.Current.GoToAsync("UzytkownicyLista");
                    break;

                // —— Strona Blokady —————————————————————————————————————————————
                case "ZbiorczeBlokady":
                    await Shell.Current.GoToAsync("BlokadyPage");
                    break;
                case "PrzyjazdWyjazd":
                    await Shell.Current.GoToAsync("PrzyjazdWyjazdPage");
                    break;
                case "Kasa":
                    // Otwórz stronę Kasa
                    await Shell.Current.Navigation.PushAsync(new Kasa());
                    break;
                // ── Rezerwacje Online ─────────────────────────────────────────
                case "RezerwacjeOnline":
                    await Shell.Current.Navigation.PushAsync(new Views.Blokady.RezerwacjeOnlinePage());
                    break;
                // ── Pulpit: wróć do roota ──────────────────────────────────────
                case "Pulpit":
                    await Shell.Current.GoToAsync("//MainPage");
                    break;
                // —— Strona Klientow —————————————————————————————————————————————
                case "Klienci":
                    await Shell.Current.GoToAsync(nameof(KlienciPage));
                    break;
                // ── Grupowe SMS ───────────────────────────────────────────────
                case "GrupoweSms":
                    await Shell.Current.Navigation.PushAsync(new GrupoweSmsPage());
                    break;
                case "Recepcja":
                    await Shell.Current.GoToAsync("///Recepcja");
                    break;


                // ── Logout ────────────────────────────────────────────────────
                case "Logout":
                    bool ok = await Shell.Current.CurrentPage.DisplayAlert(
                        "Wylogowanie", "Czy na pewno chcesz się wylogować?", "Tak", "Anuluj");
                    if (!ok) return;

                    var auth = IPlatformApplication.Current!.Services
                                   .GetRequiredService<yBook.Services.IAuthService>();
                    await auth.LogoutAsync();
                    await Shell.Current.GoToAsync("//LoginPage");
                    break;

                // ── Pozostałe trasy — do podpięcia w kolejnych etapach ─────────
                default:
                    await Shell.Current.CurrentPage
                               .DisplayAlert("yBook", $"Wkrótce: {page}", "OK");
                    break;

                // ── Rejestr akcji ─────────
                case "RejestrAkcji":
                    await Shell.Current.GoToAsync("ListaLogow");
                    break;


            }
        }
    }
}
