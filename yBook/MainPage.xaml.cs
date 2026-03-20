namespace yBook
{
    public partial class MainPage : ContentPage
    {
        // ── Drawer ─────────────────────────────────────────────────────────────
        bool _isDrawerOpen = false;
        const double DrawerWidth = 290;

        // ── Stan rozwijanych grup ──────────────────────────────────────────────
        readonly Dictionary<string, bool> _groupState = new()
        {
            { "Ceny",       false },
            { "Finanse",    false },
            { "Raporty",    false },
            { "Blokady",    false },
            { "Ustawienia", false },
        };

        public MainPage()
        {
            InitializeComponent();
            SetGreeting();
        }

        // ─── Powitanie z datą ──────────────────────────────────────────────────

        void SetGreeting()
        {
            var hour = DateTime.Now.Hour;
            LblGreeting.Text = hour switch
            {
                < 12 => "Dzień dobry! 👋",
                < 18 => "Witaj ponownie! 👋",
                _ => "Dobry wieczór! 🌙"
            };

            LblDate.Text = DateTime.Now.ToString("dddd, d MMMM yyyy",
                               new System.Globalization.CultureInfo("pl-PL"));
        }

        // ─── Hamburger ─────────────────────────────────────────────────────────

        void OnHamburgerClicked(object? sender, EventArgs e)
        {
            if (_isDrawerOpen) CloseDrawer();
            else OpenDrawer();
        }

        void OnOverlayTapped(object? sender, TappedEventArgs e)
            => CloseDrawer();

        // ─── Animacje drawera ──────────────────────────────────────────────────

        void OpenDrawer()
        {
            _isDrawerOpen = true;
            DrawerOverlay.IsVisible = true;
            DrawerOverlay.Opacity = 0;
            DrawerPanel.TranslationX = -DrawerWidth;

            var anim = new Animation();
            anim.Add(0, 1, new Animation(v => DrawerPanel.TranslationX = v,
                                         -DrawerWidth, 0, Easing.CubicOut));
            anim.Add(0, 1, new Animation(v => DrawerOverlay.Opacity = v,
                                         0, 1, Easing.Linear));
            anim.Commit(this, "OpenDrawer", length: 260);
        }

        void CloseDrawer()
        {
            _isDrawerOpen = false;

            var anim = new Animation();
            anim.Add(0, 1, new Animation(v => DrawerPanel.TranslationX = v,
                                         0, -DrawerWidth, Easing.CubicIn));
            anim.Add(0, 1, new Animation(v => DrawerOverlay.Opacity = v,
                                         1, 0, Easing.Linear));
            anim.Commit(this, "CloseDrawer", length: 220,
                        finished: (_, _) => DrawerOverlay.IsVisible = false);
        }

        // ─── Rozwijanie / zwijanie grup ────────────────────────────────────────

        async void OnGroupToggle(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string group) return;

            _groupState[group] = !_groupState[group];
            bool isOpen = _groupState[group];

            // Odwzorowanie: nazwa grupy → (VerticalStackLayout podmenu, Label strzałki)
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

            // Animacja strzałki
            await arrow.RotateTo(isOpen ? 90 : 0, 180, Easing.CubicOut);

            if (isOpen)
            {
                submenu.IsVisible = true;
                submenu.Opacity = 0;
                await submenu.FadeTo(1, 160, Easing.Linear);
            }
            else
            {
                await submenu.FadeTo(0, 130, Easing.Linear);
                submenu.IsVisible = false;
            }
        }

        // ─── Nawigacja po kliknięciu pozycji menu ──────────────────────────────

        async void OnMenuItemTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string page) return;

            // Efekt wciśnięcia
            if (sender is View tapped)
            {
                await tapped.ScaleTo(0.96, 70);
                await tapped.ScaleTo(1.00, 70);
            }

            CloseDrawer();
            await Task.Delay(240); // poczekaj na zamknięcie drawera

            // ── Tutaj podpinasz właściwe strony ──
            // Przykład: await Navigation.PushAsync(new RecepcjaPage());
            switch (page)
            {
                case "Pulpit":
                    // Już na pulpicie — odśwież dane
                    RefreshDashboard();
                    break;

                case "Recepcja":
                case "Kalendarz":
                case "Terminarz":
                case "Kasa":
                case "Cenniki":
                case "UslugiOplaty":
                case "Pakiety":
                case "Rabaty":
                case "Dokumenty":
                case "KontaFinansowe":
                case "RejestrPlatnosci":
                case "ImportMT940":
                case "RaportUslug":
                case "RaportFinansowy":
                case "RaportNiedojazdow":
                case "Rezerwacje":
                case "Klienci":
                case "GrupowySMS":
                case "Archiwum":
                case "RezerwacjeOnline":
                case "RejestrAkcji":
                case "ZbiorczeBlokady":
                case "PrzyjazdWyjazd":
                case "DaneObiektu":
                case "Pokoje":
                case "SyncICal":
                case "Uzytkownicy":
                case "KonfFaktur":
                case "Powiadomienia":
                    await DisplayAlert("yBook", $"Nawigacja do: {page}", "OK");
                    break;
                case "Status":
                    await Navigation.PushAsync(new StatusPage());
                    break;
                case "Logout":
                    bool confirm = await DisplayAlert(
                        "Wylogowanie",
                        "Czy na pewno chcesz się wylogować?",
                        "Tak", "Anuluj");
                    if (confirm)
                        await DisplayAlert("yBook", "Wylogowano pomyślnie.", "OK");
                    break;
            }
        }

        // ─── Odświeżanie danych pulpitu ────────────────────────────────────────

        void RefreshDashboard()
        {
            // Tu podepniesz prawdziwe dane z API yBook
            LblArrivals.Text = "–";
            LblDepartures.Text = "–";
            LblOccupancy.Text = "–";
            LblRevenue.Text = "–";
        }
    }
}
