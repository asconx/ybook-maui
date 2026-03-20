namespace yBook
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        bool _isDrawerOpen = false;

        // Szerokość panelu bocznego — musi zgadzać się z WidthRequest w XAML
        const double DrawerWidth = 280;

        public MainPage()
        {
            InitializeComponent();
        }

        // ─── Hamburger / zamknięcie drawera ────────────────────────────────────

        private void OnHamburgerClicked(object? sender, EventArgs e)
        {
            if (_isDrawerOpen)
                CloseDrawer();
            else
                OpenDrawer();
        }

        private void OnOverlayTapped(object? sender, TappedEventArgs e)
            => CloseDrawer();

        // ─── Animacje ──────────────────────────────────────────────────────────

        private void OpenDrawer()
        {
            _isDrawerOpen = true;

            // Pokaż overlay
            DrawerOverlay.IsVisible = true;
            DrawerOverlay.Opacity = 0;

            // Panel wjedzie z lewej (TranslationX: -280 → 0)
            DrawerPanel.TranslationX = -DrawerWidth;

            var animPanel = new Animation(v => DrawerPanel.TranslationX = v,
                                          -DrawerWidth, 0,
                                          Easing.CubicOut);

            var animOverlay = new Animation(v => DrawerOverlay.Opacity = v,
                                            0, 1,
                                            Easing.Linear);

            var parent = new Animation();
            parent.Add(0, 1, animPanel);
            parent.Add(0, 1, animOverlay);
            parent.Commit(this, "OpenDrawer", length: 260);
        }

        private void CloseDrawer()
        {
            _isDrawerOpen = false;

            var animPanel = new Animation(v => DrawerPanel.TranslationX = v,
                                          0, -DrawerWidth,
                                          Easing.CubicIn);

            var animOverlay = new Animation(v => DrawerOverlay.Opacity = v,
                                            1, 0,
                                            Easing.Linear);

            var parent = new Animation();
            parent.Add(0, 1, animPanel);
            parent.Add(0, 1, animOverlay);
            parent.Commit(this, "CloseDrawer", length: 220,
                          finished: (_, _) => DrawerOverlay.IsVisible = false);
        }

        // ─── Obsługa pozycji menu ──────────────────────────────────────────────

        private async void OnMenuItemTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string page) return;

            // Subtelna animacja wciśnięcia elementu
            if (sender is View tapped)
            {
                await tapped.ScaleTo(0.97, 80);
                await tapped.ScaleTo(1.0, 80);
            }

            CloseDrawer();

            // Tutaj podpinasz nawigację do konkretnych stron
            await Task.Delay(260); // poczekaj na zamknięcie drawera
            switch (page)
            {
                case "Home":
                    // await Navigation.PushAsync(new HomePage());
                    await DisplayAlert("Nawigacja", "Strona główna", "OK");
                    break;
                case "Library":
                    await DisplayAlert("Nawigacja", "Moja biblioteka", "OK");
                    break;
                case "Favourites":
                    await DisplayAlert("Nawigacja", "Ulubione", "OK");
                    break;
                case "Search":
                    await DisplayAlert("Nawigacja", "Szukaj", "OK");
                    break;
                case "Settings":
                    await DisplayAlert("Nawigacja", "Ustawienia", "OK");
                    break;
            }
        }

        // ─── Przykładowy licznik (zostawiony z oryginału) ──────────────────────

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;
            CounterBtn.Text = count == 1
                ? $"Kliknięto {count} raz"
                : $"Kliknięto {count} razy";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
