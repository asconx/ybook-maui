namespace yBook.Controls
{
    public partial class DrawerMenu : ContentView
    {
        const double DrawerWidth = 290;
        bool _isOpen = false;

        readonly Dictionary<string, bool> _groupState = new()
        {
            { "Ceny",       false },
            { "Finanse",    false },
            { "Raporty",    false },
            { "Blokady",    false },
            { "Ustawienia", false },
        };

        public DrawerMenu()
        {
            InitializeComponent();
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
                "Ceny"       => (MenuCenyGroup,      LblCenyArrow),
                "Finanse"    => (MenuFinanseGroup,    LblFinanseArrow),
                "Raporty"    => (MenuRaportyGroup,    LblRaportyArrow),
                "Blokady"    => (MenuBlokadyGroup,    LblBlokadyArrow),
                "Ustawienia" => (MenuUstawieniaGroup, LblUstawieniaArrow),
                _            => (null, null)
            };
            if (submenu is null || arrow is null) return;

            await arrow.RotateTo(open ? 90 : 0, 180, Easing.CubicOut);

            if (open)
            {
                submenu.IsVisible = true;
                submenu.Opacity   = 0;
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

            if (sender is View v)
            {
                await v.ScaleTo(0.96, 60);
                await v.ScaleTo(1.00, 60);
            }

            Close();
            await Task.Delay(230);

            switch (page)
            {
                // ── Strony finansów — gotowe ──────────────────────────────────
                case "Dokumenty":
                case "KontaFinansowe":
                case "RejestrPlatnosci":
                case "ImportMT940":
                    await Shell.Current.GoToAsync(page);
                    break;
                // —— Strona Rabaty —————————————————————————————————————————————
                case "Rabaty":
                    await Shell.Current.GoToAsync("RabatyPage");
                    break;
                // —— Strona Blokady —————————————————————————————————————————————
                case "ZbiorczeBlokady":
                    await Shell.Current.GoToAsync("BlokadyPage");
                    break;

                // ── Pulpit: wróć do roota ──────────────────────────────────────
                case "Pulpit":
                    await Shell.Current.GoToAsync("//MainPage");
                    break;

                // ── Logout ────────────────────────────────────────────────────
                case "Logout":
                    var page_ = Shell.Current.CurrentPage;
                    bool ok = await page_.DisplayAlert(
                        "Wylogowanie", "Czy na pewno chcesz się wylogować?", "Tak", "Anuluj");
                    if (ok)
                        await page_.DisplayAlert("yBook", "Wylogowano pomyślnie.", "OK");
                    break;

                // ── Pozostałe trasy — do podpięcia w kolejnych etapach ─────────
                default:
                    await Shell.Current.CurrentPage
                               .DisplayAlert("yBook", $"Wkrótce: {page}", "OK");
                    break;
            }
        }
    }
}
