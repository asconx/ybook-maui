using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Ustawienia;

public partial class PokojEdycjaPage : ContentPage
{
    private Pokoj _pokoj;
    private bool _isEditMode = false;
    private IPanelService _panelService;
    private int _activeTabIndex = 0;

    // Kontrolki UI
    private Entry EntryNazwa, EntryCena, EntrySkrot, EntryPozycja, EntrySmartLock, EntryMaxOsoby, EntryMetraz;
    private Picker PickerObiekt, PickerTyp, PickerStandard, BedPicker;
    private Editor EditorOpis;
    private StackLayout Tab1, Tab2, Tab3, Tab4;
    private Grid StepperContainer;

    // Stylizacja
    private Color AccentColor = Color.FromArgb("#e8c08d");
    private Color BlueGray = Color.FromArgb("#8ca7ad");
    private Color GrayText = Color.FromArgb("#7a869a");
    private Color BorderColor = Color.FromArgb("#dee2e6");

    public PokojEdycjaPage() => InitializePage(new Pokoj(), false);
    public PokojEdycjaPage(Pokoj pokoj) => InitializePage(pokoj, true);

    private void InitializePage(Pokoj pokoj, bool isEdit)
    {
        _pokoj = pokoj ?? new Pokoj();
        _isEditMode = isEdit;
        _panelService = IPlatformApplication.Current?.Services.GetService<IPanelService>();

        Shell.SetNavBarIsVisible(this, false);
        BuildMainLayout();
        SwitchTab(0);
        if (_isEditMode) LoadDataToUI();
    }

    private void BuildMainLayout()
    {
        BackgroundColor = Colors.White;
        var mainGrid = new Grid
        {
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto }, // Stepper
                new RowDefinition { Height = GridLength.Star }, // Content
                new RowDefinition { Height = GridLength.Auto }  // Footer
            }
        };

        StepperContainer = new Grid
        {
            Padding = new Thickness(20, 20),
            ColumnDefinitions = {
            new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Auto)
        }
        };
        mainGrid.Add(StepperContainer, 0, 0);

        Tab1 = BuildTabPodstawowe();
        Tab2 = BuildTabOsoby();
        Tab3 = BuildTabZdjecia();
        Tab4 = BuildTabUdogodnienia();

        mainGrid.Add(new ScrollView { Content = new Grid { Children = { Tab1, Tab2, Tab3, Tab4 } } }, 0, 1);

        var btnSave = new Button
        {
            Text = "zapisz kwaterę",
            BackgroundColor = AccentColor,
            TextColor = Colors.White,
            CornerRadius = 20,
            Margin = 20,
            HorizontalOptions = LayoutOptions.End,
            HeightRequest = 45,
            WidthRequest = 200
        };
        btnSave.Clicked += OnSaveClicked;
        mainGrid.Add(btnSave, 0, 2);

        Content = mainGrid;
    }

    private StackLayout BuildTabPodstawowe() => new StackLayout
    {
        Padding = 30,
        Spacing = 10,
        Children = {
        CreateField("Wybierz obiekt", PickerObiekt = new Picker { Title = "Wybierz..." }),
        CreateField("Typ kwatery", PickerTyp = new Picker { Title = "Pokój" }),
        CreateField("Standard", PickerStandard = new Picker { Title = "Standard" }),
        CreateField("Nazwa", EntryNazwa = new Entry { Placeholder = "Mały pokój 1" }),
        CreateField("Skrócona nazwa", EntrySkrot = new Entry { Placeholder = "1" }, "Nazwa skrócona i kolor są wyświetlane w kalendarzu rezerwacji."),
        CreateField("Pozycja na kalendarzu", EntryPozycja = new Entry { Placeholder = "68" }),
        CreateField("Smart Lock Id", EntrySmartLock = new Entry { Placeholder = "Smart Lock Id" }),
        CreateField("Cena podstawowa", EntryCena = new Entry { Placeholder = "0" }),
        new Button { Text = "dalej", BackgroundColor = AccentColor, CornerRadius = 15, WidthRequest = 100 }.WithClick((s,e) => SwitchTab(1))
    }
    };

    private StackLayout BuildTabOsoby() => new StackLayout
    {
        Padding = 30,
        Spacing = 15,
        Children = {
        new HorizontalStackLayout { Spacing = 10, Children = {
            new Border { Stroke = BorderColor, Content = BedPicker = new Picker { Title = "Wybierz łóżka", WidthRequest = 200 } },
            new Button { Text = "+ dodaj łóżko", BackgroundColor = Colors.Black, CornerRadius = 20 }
        }},
        new Border { BackgroundColor = Color.FromArgb("#ffe8d1"), Stroke = AccentColor, Padding = 5, HorizontalOptions = LayoutOptions.Start, StrokeShape = new RoundRectangle{CornerRadius=15},
            Content = new Label { Text = "✓ Rozkładana sofa  ✕", TextColor = AccentColor } },
        new HorizontalStackLayout { Spacing = 10, Children = {
            new Label { Text = "Maksymalna liczba osób", VerticalOptions = LayoutOptions.Center },
            new Border { Stroke = BorderColor, Content = EntryMaxOsoby = new Entry { Text = "2", WidthRequest = 60 } }
        }},
        CreateField("Metraż", EntryMetraz = new Entry { Placeholder = "6" }),
        CreateField("Opis", EditorOpis = new Editor { HeightRequest = 120 }),
        new HorizontalStackLayout { Spacing = 10, Children = {
            new Button { Text = "wstecz", BackgroundColor = BlueGray, CornerRadius = 15 }.WithClick((s,e) => SwitchTab(0)),
            new Button { Text = "dalej", BackgroundColor = AccentColor, CornerRadius = 15 }.WithClick((s,e) => SwitchTab(2))
        }}
    }
    };
}
public partial class PokojEdycjaPage
{
    private StackLayout BuildTabZdjecia() => new StackLayout
    {
        Padding = 30,
        Spacing = 20,
        Children = {
        new FlexLayout { Wrap = FlexWrap.Wrap, Children = {
            CreatePhotoThumb("photo1.jpg"), CreatePhotoThumb("photo2.jpg")
        }},
        new Border { Stroke = BorderColor, Padding = 15, Content = new Label { Text = "Wybierz zdjęcia lub je przeciągnij.", TextColor = GrayText } },
        new HorizontalStackLayout { Spacing = 10, Children = {
            new Button { Text = "wstecz", BackgroundColor = BlueGray, CornerRadius = 15 }.WithClick((s,e) => SwitchTab(1)),
            new Button { Text = "dalej", BackgroundColor = AccentColor, CornerRadius = 15 }.WithClick((s,e) => SwitchTab(3))
        }}
    }
    };

    private StackLayout BuildTabUdogodnienia() => new StackLayout
    {
        Padding = 30,
        Spacing = 20,
        Children = {
        new FlexLayout { Wrap = FlexWrap.Wrap, Children = {
            CreateTag("Basen"), CreateTag("Balkon"), CreateTag("Biurko", true), CreateTag("Czajnik"),
            CreateTag("Mydło", true), CreateTag("TV", true), CreateTag("Suszarka", true)
        }},
        new Button { Text = "wstecz", BackgroundColor = BlueGray, CornerRadius = 15, HorizontalOptions = LayoutOptions.Start }.WithClick((s,e) => SwitchTab(2))
    }
    };

    private void SwitchTab(int index)
    {
        _activeTabIndex = index;
        Tab1.IsVisible = (index == 0);
        Tab2.IsVisible = (index == 1);
        Tab3.IsVisible = (index == 2);
        Tab4.IsVisible = (index == 3);
        UpdateStepper(index);
    }

    private void UpdateStepper(int activeIndex)
    {
        StepperContainer.Children.Clear();
        string[] titles = { "Podstawowe informacje", "Liczba osób", "Załaduj zdjęcia", "Udogodnienia" };
        string[] icons = { "✓", "✎", "📷", "✚" };

        for (int i = 0; i < titles.Length; i++)
        {
            bool isActive = i <= activeIndex;
            var step = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children = {
                new Border { HeightRequest = 30, WidthRequest = 30, StrokeShape = new RoundRectangle{CornerRadius=15},
                             BackgroundColor = isActive ? BlueGray : Color.FromArgb("#e0e0e0"),
                             Content = new Label { Text = icons[i], TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } },
                new Label { Text = titles[i], FontSize = 8, TextColor = isActive ? BlueGray : GrayText, HorizontalTextAlignment = TextAlignment.Center }
            }
            };
            StepperContainer.Add(step, i * 2, 0);
            if (i < 3) StepperContainer.Add(new BoxView { Color = BorderColor, HeightRequest = 1, VerticalOptions = LayoutOptions.Center }, (i * 2) + 1, 0);
        }
    }

    private View CreateField(string label, View control, string hint = null)
    {
        var stack = new VerticalStackLayout { Spacing = 2, Margin = new Thickness(0, 0, 0, 10) };
        stack.Add(new Label { Text = label, FontSize = 12, TextColor = GrayText });
        stack.Add(new Border { Stroke = BorderColor, Padding = new Thickness(10, 0), Content = control });
        if (hint != null) stack.Add(new Label { Text = hint, FontSize = 10, TextColor = GrayText });
        return stack;
    }

    private View CreateTag(string text, bool isSelected = false) => new Border
    {
        Margin = new Thickness(3),
        Padding = new Thickness(12, 6),
        BackgroundColor = isSelected ? AccentColor : Color.FromArgb("#f0f0f0"),
        StrokeShape = new RoundRectangle { CornerRadius = 15 },
        Content = new Label { Text = (isSelected ? "✓ " : "") + text, TextColor = isSelected ? Colors.White : Colors.Black, FontSize = 12 }
    };

    private View CreatePhotoThumb(string src) => new Grid
    {
        Margin = new Thickness(5),
        Children = {
            new Border { StrokeShape = new RoundRectangle{CornerRadius=5}, Content = new Image { Source = src, WidthRequest = 120, HeightRequest = 80, Aspect = Aspect.AspectFill } },
            new Button { Text = "✕", BackgroundColor = BlueGray, HeightRequest = 20, WidthRequest = 20, CornerRadius = 10, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start }
        }
    };

    private void LoadDataToUI()
    {
        EntryNazwa.Text = _pokoj.Nazwa;
        EntryMaxOsoby.Text = _pokoj.MaxOsobLiczbą.ToString();
        EntryMetraz.Text = _pokoj.Powierzchnia;
    }

    private async void OnSaveClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}

public static class ButtonExtensions
{
    public static Button WithClick(this Button b, EventHandler h) { b.Clicked += h; return b; }
}