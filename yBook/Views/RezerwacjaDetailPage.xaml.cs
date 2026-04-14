using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace yBook.Views
{
    public class RezerwacjaDetailPage : ContentPage
    {
        DatePicker _from = new DatePicker { IsVisible = false };
        DatePicker _to = new DatePicker { IsVisible = false };

        // UI fields used across methods
        Button BtnDodajTermin;
        Button BtnAdultMinus;
        Button BtnAdultPlus;
        Label  LblAdultCount;
        Button BtnChildMinus;
        Button BtnChildPlus;
        Label  LblChildCount;
        Button BtnInfantMinus;
        Button BtnInfantPlus;
        Label  LblInfantCount;
        Button BtnWybierzTermin;
        Label  LblWybranyTermin;
        Label  LblPrice;

        int adults = 1, children = 0, infants = 0;
        const int pricePerPersonPerDay = 100;

        public RezerwacjaDetailPage()
        {
            // build UI in code (no XAML)
            BuildUi();

            // wire events to named controls from code-built UI
            BtnAdultMinus.Clicked += (s, e) => ChangeCount(ref adults, -1, LblAdultCount);
            BtnAdultPlus.Clicked += (s, e) => ChangeCount(ref adults, +1, LblAdultCount);

            BtnChildMinus.Clicked += (s, e) => ChangeCount(ref children, -1, LblChildCount);
            BtnChildPlus.Clicked += (s, e) => ChangeCount(ref children, +1, LblChildCount);

            BtnInfantMinus.Clicked += (s, e) => ChangeCount(ref infants, -1, LblInfantCount);
            BtnInfantPlus.Clicked += (s, e) => ChangeCount(ref infants, +1, LblInfantCount);

            BtnWybierzTermin.Clicked += OnChooseDate;
            BtnDodajTermin.Clicked += (s, e) => DisplayAlert("Dodaj termin", "Funkcja dodaj termin (placeholder)", "OK");

            _from.DateSelected += (_, __) => UpdateSelectedTerm();
            _to.DateSelected += (_, __) => UpdateSelectedTerm();

            // add hidden datepickers into the existing layout from code-built UI
            if (this.Content is ScrollView sv && sv.Content is Layout layout)
            {
                layout.Children.Add(_from);
                layout.Children.Add(_to);
            }

            UpdatePrice();
        }

        void BuildUi()
        {
            BtnDodajTermin = new Button { Text = "dodaj termin" };

            BtnAdultMinus = new Button { Text = "-", WidthRequest = 36 };
            BtnAdultPlus = new Button { Text = "+", WidthRequest = 36 };
            LblAdultCount = new Label { Text = "1", WidthRequest = 36, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };

            BtnChildMinus = new Button { Text = "-", WidthRequest = 36 };
            BtnChildPlus = new Button { Text = "+", WidthRequest = 36 };
            LblChildCount = new Label { Text = "0", WidthRequest = 36, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };

            BtnInfantMinus = new Button { Text = "-", WidthRequest = 36 };
            BtnInfantPlus = new Button { Text = "+", WidthRequest = 36 };
            LblInfantCount = new Label { Text = "0", WidthRequest = 36, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };

            BtnWybierzTermin = new Button { Text = "Wybierz termin" };
            LblWybranyTermin = new Label { Text = "Brak" };
            LblPrice = new Label { Text = "Opłata: 0 PLN", FontAttributes = FontAttributes.Bold };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() }
            };

            var col1 = new VerticalStackLayout { Spacing = 8 };
            col1.Children.Add(new Label { Text = "termin 1", FontAttributes = FontAttributes.Bold });
            col1.Children.Add(BtnDodajTermin);

            var col2 = new VerticalStackLayout { Spacing = 8 };
            var row = new HorizontalStackLayout { Spacing = 10 };

            var vAdult = new VerticalStackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand };
            vAdult.Children.Add(new Label { Text = "Dorośli" });
            vAdult.Children.Add(new HorizontalStackLayout { Spacing = 6, Children = { BtnAdultMinus, LblAdultCount, BtnAdultPlus } });

            var vChild = new VerticalStackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand };
            vChild.Children.Add(new Label { Text = "Dzieci" });
            vChild.Children.Add(new HorizontalStackLayout { Spacing = 6, Children = { BtnChildMinus, LblChildCount, BtnChildPlus } });

            var vInfant = new VerticalStackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand };
            vInfant.Children.Add(new Label { Text = "0-3 lat" });
            vInfant.Children.Add(new HorizontalStackLayout { Spacing = 6, Children = { BtnInfantMinus, LblInfantCount, BtnInfantPlus } });

            row.Children.Add(vAdult);
            row.Children.Add(vChild);
            row.Children.Add(vInfant);
            col2.Children.Add(row);

            var col3 = new VerticalStackLayout { Spacing = 8 };
            col3.Children.Add(BtnWybierzTermin);
            col3.Children.Add(LblWybranyTermin);
            col3.Children.Add(LblPrice);

            grid.Add(col1, 0, 0);
            grid.Add(col2, 1, 0);
            grid.Add(col3, 2, 0);

            var main = new ScrollView { Content = new VerticalStackLayout { Padding = 12, Spacing = 12, Children = { grid } } };

            // add hidden datepickers to page
            var root = new AbsoluteLayout();
            root.Children.Add(main);
            root.Children.Add(_from);
            root.Children.Add(_to);

            Content = root;
        }

        void ChangeCount(ref int field, int delta, Label lbl)
        {
            field = Math.Max(0, field + delta);
            if (lbl == LblAdultCount && field == 0) field = 1; // at least 1 adult
            lbl.Text = field.ToString();
            UpdatePrice();
        }

        async void OnChooseDate(object? s, EventArgs e)
        {
            // show simple date selection sequence
            await DisplayAlert("Wybierz od", "Wybierz datę początkową", "OK");
            _from.Focus();
            await Task.Delay(10);
            await DisplayAlert("Wybierz do", "Wybierz datę końcową", "OK");
            _to.Focus();
        }

        void UpdateSelectedTerm()
        {
            if (_to.Date < _from.Date)
            {
                LblWybranyTermin.Text = "Nieprawidłowy zakres";
                LblPrice.Text = "Opłata: 0 PLN";
                return;
            }
            var diff = _to.Date - _from.Date;
            int days = diff.HasValue ? diff.Value.Days + 1 : 0;
            LblWybranyTermin.Text = $"{_from.Date:yyyy-MM-dd} - {_to.Date:yyyy-MM-dd} ({days} dni)";
            UpdatePrice();
        }

        void UpdatePrice()
        {
            if (_to.Date < _from.Date) {
                LblPrice.Text = "Opłata: 0 PLN";
                return;
            }
            var diff = _to.Date - _from.Date;
            int days = diff.HasValue ? diff.Value.Days + 1 : 0;
            if (days <= 0) days = 0;
            var persons = adults + children + infants;
            var total = days * persons * pricePerPersonPerDay;
            LblPrice.Text = $"Opłata: {total} PLN";
        }
    }
}
