using yBook.Models;
using yBook.Services;
using Microsoft.Maui.Controls.Shapes;

namespace yBook.Views.Recepcja
{
    public partial class RecepcjaPage : ContentPage
    {
        private List<RezerwacjaOnline> _rezerwacje = new();
        private List<RezerwacjaOnline> _rezerwacjeZameldowane = new();
        private List<RezerwacjaOnline> _rezerwacjeNiezameldowane = new();
        private string _activeTab = "zameldowany";
        private readonly IRezerwacjaService _rezerwacjaService;
        private bool _isLoading = false;

        public RecepcjaPage()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[RecepcjaPage] Constructor called");

            // Pobierz AuthService z DI container'a
            var authService = IPlatformApplication.Current?.Services.GetService<IAuthService>();
            _rezerwacjaService = new RezerwacjaService(authService);

            LoadRezerwacje();
        }

        private async void LoadRezerwacje()
        {
            System.Diagnostics.Debug.WriteLine("[RecepcjaPage] LoadRezerwacje called");
            if (_isLoading) return;

            _isLoading = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("[RecepcjaPage] Fetching zameldowane reservations...");
                _rezerwacjeZameldowane = await _rezerwacjaService.GetRezerwacjeZameldowaneAsync();
                System.Diagnostics.Debug.WriteLine($"[RecepcjaPage] Got {_rezerwacjeZameldowane.Count} zameldowane reservations");

                System.Diagnostics.Debug.WriteLine("[RecepcjaPage] Fetching niezameldowane reservations...");
                _rezerwacjeNiezameldowane = await _rezerwacjaService.GetRezerwacjeNiezameldowaneAsync();
                System.Diagnostics.Debug.WriteLine($"[RecepcjaPage] Got {_rezerwacjeNiezameldowane.Count} niezameldowane reservations");

                _rezerwacje = _rezerwacjeZameldowane;
                System.Diagnostics.Debug.WriteLine($"[RecepcjaPage] Total reservations to display: {_rezerwacje.Count}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    System.Diagnostics.Debug.WriteLine("[RecepcjaPage] Rendering on main thread...");
                    RenderRezerwacje();
                    UpdateTabColors();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RecepcjaPage] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RecepcjaPage] Stack: {ex.StackTrace}");
                await DisplayAlert("Blad", $"Nie udalo sie zaladowac rezerwacji: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void RenderRezerwacje()
        {
            RezerwacjeContainer.Children.Clear();

            if (_rezerwacje.Count == 0)
            {
                LblBrakRezerwacji.IsVisible = true;
                return;
            }

            LblBrakRezerwacji.IsVisible = false;

            foreach (var rez in _rezerwacje)
            {
                var card = CreateRezerwacjaCard(rez);
                RezerwacjeContainer.Children.Add(card);
            }
        }

        private Border CreateRezerwacjaCard(RezerwacjaOnline rez)
        {
            var card = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16),
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                WidthRequest = 240,
                MinimumHeightRequest = 200
            };

            var stack = new VerticalStackLayout { Spacing = 8 };

            // Header: Numer i status
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Margin = new Thickness(0, 0, 0, 8)
            };

            var lblNumer = new Label
            {
                Text = $"Rezerwacja nr. {rez.Id}",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#263238")
            };
            headerGrid.Add(lblNumer, 0);

            var statusKolor = rez.Status switch
            {
                StatusRezerwacji.Potwierdzona => "#2E7D32",
                StatusRezerwacji.Oczekujaca => "#F57C00",
                StatusRezerwacji.Anulowana => "#C62828",
                _ => "#9E9E9E"
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(statusKolor),
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(8, 3),
                Content = new Label
                {
                    Text = rez.Status.ToString(),
                    FontSize = 9,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold
                }
            };
            headerGrid.Add(badge, 1);
            stack.Add(headerGrid);

            var lblGosc = new Label
            {
                Text = rez.PelneNazwisko,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1565C0")
            };
            stack.Add(lblGosc);

            var lblTyp = new Label
            {
                Text = rez.TypPokoju,
                FontSize = 11,
                TextColor = Color.FromArgb("#607D8B")
            };
            stack.Add(lblTyp);

            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#ECEFF1"),
                Margin = new Thickness(0, 4)
            };
            stack.Add(separator);

            var lblTermin = new Label
            {
                Text = $"Termin: {rez.DataPrzyjazdu:yyyy-MM-dd} – {rez.DataWyjazdu:yyyy-MM-dd}",
                FontSize = 11,
                TextColor = Color.FromArgb("#78909C")
            };
            stack.Add(lblTermin);

            var lblNoci = new Label
            {
                Text = $"{rez.LiczbaNoci} noce",
                FontSize = 11,
                TextColor = Color.FromArgb("#78909C")
            };
            stack.Add(lblNoci);

            card.Content = stack;

            var gesture = new TapGestureRecognizer();
            gesture.Tapped += (s, e) => OnRezerwacjaTapped(rez);
            card.GestureRecognizers.Add(gesture);

            return card;
        }

        void OnTabTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not string tab) return;

            _activeTab = tab;
            _rezerwacje = tab == "zameldowany"
                ? _rezerwacjeZameldowane
                : _rezerwacjeNiezameldowane;

            RenderRezerwacje();
            UpdateTabColors();
        }

        void UpdateTabColors()
        {
            if (LblTab1 != null && LblTab2 != null)
            {
                if (_activeTab == "zameldowany")
                {
                    LblTab1.TextColor = Color.FromArgb("#1565C0");
                    LblTab1.FontAttributes = FontAttributes.Bold;
                    LblTab2.TextColor = Color.FromArgb("#9E9E9E");
                    LblTab2.FontAttributes = FontAttributes.None;
                }
                else
                {
                    LblTab1.TextColor = Color.FromArgb("#9E9E9E");
                    LblTab1.FontAttributes = FontAttributes.None;
                    LblTab2.TextColor = Color.FromArgb("#1565C0");
                    LblTab2.FontAttributes = FontAttributes.Bold;
                }
            }
        }

        void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                RenderRezerwacje();
                return;
            }

            var filtered = _rezerwacje
                .Where(r => r.Id.Contains(e.NewTextValue) || 
                            r.PelneNazwisko.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase) ||
                            r.Email.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase))
                .ToList();

            RezerwacjeContainer.Children.Clear();
            foreach (var rez in filtered)
            {
                RezerwacjeContainer.Children.Add(CreateRezerwacjaCard(rez));
            }
        }

        private async void OnRezerwacjaTapped(RezerwacjaOnline rez)
        {
            if (rez == null) return;

            var detailsPage = new RezerwacjaDetailsPage();
            detailsPage.LoadRezerwacja(rez.Id);
            
            await Navigation.PushModalAsync(detailsPage);
        }
    }
}