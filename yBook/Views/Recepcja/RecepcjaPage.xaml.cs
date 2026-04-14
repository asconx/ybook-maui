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

        public RecepcjaPage()
        {
            InitializeComponent();
            _rezerwacjaService = new RezerwacjaService();
            LoadRezerwacje();
        }

        private void LoadRezerwacje()
        {
            _rezerwacjeZameldowane = _rezerwacjaService.GetRezerwacjeZameldowane();
            _rezerwacjeNiezameldowane = _rezerwacjaService.GetRezerwacjeNiezameldowane();
            _rezerwacje = _rezerwacjeZameldowane;

            RenderRezerwacje();
            UpdateTabColors();
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

            // Badge statusu
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

            // Pe³ne imiê i nazwisko
            var lblGosc = new Label
            {
                Text = rez.PelneNazwisko,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1565C0")
            };
            stack.Add(lblGosc);

            // Typ pokoju
            var lblTyp = new Label
            {
                Text = rez.TypPokoju,
                FontSize = 11,
                TextColor = Color.FromArgb("#607D8B")
            };
            stack.Add(lblTyp);

            // Separator
            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#ECEFF1"),
                Margin = new Thickness(0, 4)
            };
            stack.Add(separator);

            // Termin
            var lblTermin = new Label
            {
                Text = $"Termin: {rez.DataPrzyjazdu:yyyy-MM-dd} – {rez.DataWyjazdu:yyyy-MM-dd}",
                FontSize = 11,
                TextColor = Color.FromArgb("#78909C")
            };
            stack.Add(lblTermin);

            // Noce
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
            var searchText = e.NewTextValue?.ToLower() ?? "";

            var filtered = (_activeTab == "zameldowany" ? _rezerwacjeZameldowane : _rezerwacjeNiezameldowane)
                .Where(r => r.Id.Contains(searchText) ||
                           r.PelneNazwisko.ToLower().Contains(searchText))
                .ToList();

            _rezerwacje = filtered;
            RenderRezerwacje();
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