using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls.Hosting;
using yBook.Models;
using yBook.Services;
using System.Collections.ObjectModel;
using MButton = Microsoft.Maui.Controls.Button;
using MScrollView = Microsoft.Maui.Controls.ScrollView;
using MCheckBox = Microsoft.Maui.Controls.CheckBox;

namespace yBook.Views.Ustawienia;

public class PokojEdycjaPage : ContentPage
{
    private Pokoj _pokoj;
    private int _activeTabIndex = 0;
    private bool _isEditMode = false;
    private IPanelService _panelService;

    // UI Controls
    private Entry EntryNazwa;
    private Entry EntrySkroconaNazwa;
    private Entry EntryStandard;
    private Entry EntryKolor;
    private Entry EntryMetraz;
    private Entry EntryMaxOsoby;
    private Entry EntryMinOsoby;
    private Entry EntryCenaPodstawowa;
    private Entry EntrySmartLockId;
    private Entry EntryPozycjaKalendarzu;
    private Picker PickerObiekt;
    private Picker PickerTyp;
    private CollectionView PhotosList;
    private StackLayout TabBasicContent;
    private StackLayout TabOsobyContent;
    private StackLayout TabZdjeciaContent;
    private StackLayout TabUdogodneniaContent;
    private MButton TabBasic;
    private MButton TabOsoby;
    private MButton TabZdjecia;
    private MButton TabUdogodnienia;
    private MScrollView ContentScroll;
    private MButton BtnSave;

    public PokojEdycjaPage()
    {
        _pokoj = new Pokoj();
        _panelService = IPlatformApplication.Current?.Services.GetService<IPanelService>();
        BuildUI();
    }

    public PokojEdycjaPage(Pokoj pokoj)
    {
        _pokoj = pokoj ?? new Pokoj();
        _isEditMode = pokoj != null;
        _panelService = IPlatformApplication.Current?.Services.GetService<IPanelService>();
        BuildUI();
        LoadPokojData();
    }

    private void BuildUI()
    {
        BackgroundColor = Colors.White;
        Padding = 0;

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection 
            { 
                new RowDefinition { Height = new GridLength(60, GridUnitType.Absolute) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 0,
            RowSpacing = 0
        };

        // Row 0: Header
        var headerLabel = new Label 
        { 
            Text = "Edycja kwatery",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(20, 0),
            VerticalOptions = LayoutOptions.Center
        };
        mainGrid.Add(headerLabel, 0, 0);

        // Row 1: Tabs
        var tabsGrid = new Grid { ColumnSpacing = 0, BackgroundColor = Colors.White, Padding = 0 };

        TabBasic = new MButton { Text = "Podstawowe", BackgroundColor = Colors.Transparent, TextColor = Colors.Black, FontAttributes = FontAttributes.Bold, FontSize = 13, Padding = new Thickness(20, 15), BorderWidth = 0 };
        TabBasic.Clicked += (s, e) => SwitchTab(0);

        TabOsoby = new MButton { Text = "Liczba osób", BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#999"), FontSize = 13, Padding = new Thickness(20, 15), BorderWidth = 0 };
        TabOsoby.Clicked += (s, e) => SwitchTab(1);

        TabZdjecia = new MButton { Text = "Zdjęcia", BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#999"), FontSize = 13, Padding = new Thickness(20, 15), BorderWidth = 0 };
        TabZdjecia.Clicked += (s, e) => SwitchTab(2);

        TabUdogodnienia = new MButton { Text = "Udogodnienia", BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#999"), FontSize = 13, Padding = new Thickness(20, 15), BorderWidth = 0 };
        TabUdogodnienia.Clicked += (s, e) => SwitchTab(3);

        var tabsStack = new HorizontalStackLayout 
        { 
            Spacing = 0,
            Padding = 0,
            Children = { TabBasic, TabOsoby, TabZdjecia, TabUdogodnienia }
        };
        var tabsScroll = new MScrollView { Content = tabsStack, Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Never };

        mainGrid.Add(tabsScroll, 0, 1);

        // Row 2: Content
        TabBasicContent = BuildBasicTab();
        TabOsobyContent = BuildOsobyTab();
        TabZdjeciaContent = BuildZdjeciaTab();
        TabUdogodneniaContent = BuildUdogodneniaTab();

        TabOsobyContent.IsVisible = false;
        TabZdjeciaContent.IsVisible = false;
        TabUdogodneniaContent.IsVisible = false;

        var contentStack = new StackLayout 
        { 
            Spacing = 0,
            Padding = 0,
            Children = { TabBasicContent, TabOsobyContent, TabZdjeciaContent, TabUdogodneniaContent }
        };

        ContentScroll = new MScrollView 
        { 
            Content = contentStack, 
            Orientation = ScrollOrientation.Vertical,
            VerticalOptions = LayoutOptions.Fill,
            VerticalScrollBarVisibility = ScrollBarVisibility.Default,
            Padding = 0
        };

        var contentGrid = new Grid 
        { 
            RowDefinitions = new RowDefinitionCollection 
            { 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, 
                new RowDefinition { Height = GridLength.Auto } 
            },
            Padding = 0,
            ColumnSpacing = 0,
            RowSpacing = 0
        };
        contentGrid.Add(ContentScroll, 0, 0);

        BtnSave = new MButton 
        { 
            Text = _isEditMode ? "Zapisz zmiany" : "Dodaj kwaterę",
            BackgroundColor = Color.FromArgb("#D4A574"),
            TextColor = Colors.White,
            Margin = new Thickness(20, 10, 20, 20),
            Padding = new Thickness(15, 12),
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 25
        };
        BtnSave.Clicked += OnSaveClicked;
        contentGrid.Add(BtnSave, 0, 1);

        mainGrid.Add(contentGrid, 0, 2);

        Content = mainGrid;

        InitializeData();
    }

    private StackLayout BuildBasicTab()
    {
        PickerObiekt = new Picker { Title = "Wybierz obiekt" };
        PickerObiekt.Items.Add("Czujnik temperatury — recepcja");
        PickerObiekt.SelectedIndex = 0;

        PickerTyp = new Picker { Title = "Typ kwatery" };
        PickerTyp.Items.Add("Pokój");
        PickerTyp.Items.Add("Apartament");
        PickerTyp.Items.Add("Willa");

        EntryStandard = new Entry { Placeholder = "Standard" };
        EntryNazwa = new Entry { Placeholder = "Nazwa pokoju" };
        EntrySkroconaNazwa = new Entry { Placeholder = "Skrócona nazwa" };
        EntryPozycjaKalendarzu = new Entry { Placeholder = "0", Keyboard = Keyboard.Numeric };
        EntryKolor = new Entry { Placeholder = "#fff", Text = "#fff" };
        EntrySmartLockId = new Entry { Placeholder = "Smart Lock Id" };
        EntryCenaPodstawowa = new Entry { Placeholder = "0", Keyboard = Keyboard.Numeric };

        return new StackLayout 
        { 
            Padding = new Thickness(15, 10),
            Spacing = 10,
            Children = 
            {
                new Label { Text = "Wybierz obiekt", FontSize = 12, TextColor = Color.FromArgb("#888") },
                PickerObiekt,
                new Label { Text = "Typ kwatery", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                PickerTyp,
                new Label { Text = "Standard", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryStandard,
                new Label { Text = "Nazwa", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryNazwa,
                new Label { Text = "Skrócona nazwa", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntrySkroconaNazwa,
                new Label { Text = "Pozycja na kalendarzu", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryPozycjaKalendarzu,
                new Label { Text = "Kolor", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryKolor,
                new Label { Text = "Smart Lock Id", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntrySmartLockId,
                new Label { Text = "Cena podstawowa", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryCenaPodstawowa
            }
        };
    }

    private StackLayout BuildOsobyTab()
    {
        EntryMinOsoby = new Entry { Placeholder = "1", Keyboard = Keyboard.Numeric };
        EntryMaxOsoby = new Entry { Placeholder = "4", Keyboard = Keyboard.Numeric };
        EntryMetraz = new Entry { Placeholder = "Metraż w m²", Keyboard = Keyboard.Numeric };

        return new StackLayout 
        { 
            Padding = new Thickness(15, 10),
            Spacing = 10,
            Children = 
            {
                new Label { Text = "Minimalna liczba osób", FontSize = 12, TextColor = Color.FromArgb("#888") },
                EntryMinOsoby,
                new Label { Text = "Maksymalna liczba osób", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryMaxOsoby,
                new Label { Text = "Metraż", FontSize = 12, TextColor = Color.FromArgb("#888"), Margin = new Thickness(0, 5, 0, 0) },
                EntryMetraz
            }
        };
    }

    private StackLayout BuildZdjeciaTab()
    {
        var btnAddPhoto = new MButton { Text = "Dodaj zdjęcie", BackgroundColor = Colors.Black, TextColor = Colors.White, Padding = new Thickness(15, 10), FontAttributes = FontAttributes.Bold };
        btnAddPhoto.Clicked += OnAddPhotoClicked;

        PhotosList = new CollectionView { ItemsSource = new ObservableCollection<string>() };

        return new StackLayout 
        { 
            Padding = new Thickness(15, 10),
            Spacing = 10,
            Children = 
            {
                new Label { Text = "Dodaj zdjęcia pokoju", FontSize = 14, FontAttributes = FontAttributes.Bold },
                btnAddPhoto,
                PhotosList
            }
        };
    }

    private StackLayout BuildUdogodneniaTab()
    {
        var stack = new StackLayout 
        { 
            Padding = new Thickness(15, 10),
            Spacing = 8
        };

        stack.Add(new Label { Text = "Zaznacz dostępne udogodnienia", FontSize = 14, FontAttributes = FontAttributes.Bold });

        var items = new[] { "Biurko", "Mydło", "Ogrzewanie", "Internet", "Telewizor", "Szczotka do zębów" };
        foreach (var item in items)
        {
            var checkBox = new MCheckBox { };
            var label = new Label { Text = item, FontSize = 13, VerticalOptions = LayoutOptions.Center };
            var checkBoxStack = new HorizontalStackLayout { Spacing = 10 };
            checkBoxStack.Add(checkBox);
            checkBoxStack.Add(label);
            stack.Add(checkBoxStack);
        }

        return stack;
    }

    private void InitializeData()
    {
    }

    private void LoadPokojData()
    {
        if (!_isEditMode) return;

        EntryNazwa.Text = _pokoj.Nazwa;
        EntrySkroconaNazwa.Text = _pokoj.ShortName;
        EntryStandard.Text = _pokoj.Standard;
        EntryKolor.Text = _pokoj.Kolor;
        EntryCenaPodstawowa.Text = _pokoj.DefaultPrice.ToString();
        EntryMetraz.Text = _pokoj.Powierzchnia;
        EntryMaxOsoby.Text = _pokoj.MaxOsobLiczbą.ToString();
        EntryMinOsoby.Text = _pokoj.MinOsobLiczbą.ToString();
        EntrySmartLockId.Text = _pokoj.LockId.ToString();
        EntryPozycjaKalendarzu.Text = _pokoj.CalendarPosition.ToString();
    }

    private void SwitchTab(int tabIndex)
    {
        _activeTabIndex = tabIndex;

        TabBasicContent.IsVisible = (tabIndex == 0);
        TabOsobyContent.IsVisible = (tabIndex == 1);
        TabZdjeciaContent.IsVisible = (tabIndex == 2);
        TabUdogodneniaContent.IsVisible = (tabIndex == 3);

        TabBasic.TextColor = tabIndex == 0 ? Colors.Black : Color.FromArgb("#999");
        TabBasic.FontAttributes = tabIndex == 0 ? FontAttributes.Bold : FontAttributes.None;

        TabOsoby.TextColor = tabIndex == 1 ? Colors.Black : Color.FromArgb("#999");
        TabOsoby.FontAttributes = tabIndex == 1 ? FontAttributes.Bold : FontAttributes.None;

        TabZdjecia.TextColor = tabIndex == 2 ? Colors.Black : Color.FromArgb("#999");
        TabZdjecia.FontAttributes = tabIndex == 2 ? FontAttributes.Bold : FontAttributes.None;

        TabUdogodnienia.TextColor = tabIndex == 3 ? Colors.Black : Color.FromArgb("#999");
        TabUdogodnienia.FontAttributes = tabIndex == 3 ? FontAttributes.Bold : FontAttributes.None;

        // Resetuj scroll na top
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await ContentScroll.ScrollToAsync(0, 0, false);
        });
    }

    private async void OnAddPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Wybierz zdjęcie"
            });

            if (result != null)
            {
                var filePath = result.FullPath;
                if (PhotosList.ItemsSource is ObservableCollection<string> photos)
                {
                    photos.Add(filePath);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się wybrać zdjęcia: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            _pokoj.Nazwa = EntryNazwa.Text;
            _pokoj.ShortName = EntrySkroconaNazwa.Text;
            _pokoj.Standard = EntryStandard.Text;
            _pokoj.Kolor = EntryKolor.Text;
            _pokoj.Powierzchnia = EntryMetraz.Text;

            if (int.TryParse(EntryCenaPodstawowa.Text, out int price))
                _pokoj.DefaultPrice = price;

            if (int.TryParse(EntryMaxOsoby.Text, out int maxOsoby))
                _pokoj.MaxOsobLiczbą = maxOsoby;

            if (int.TryParse(EntryMinOsoby.Text, out int minOsoby))
                _pokoj.MinOsobLiczbą = minOsoby;

            if (int.TryParse(EntrySmartLockId.Text, out int lockId))
                _pokoj.LockId = lockId;

            if (int.TryParse(EntryPozycjaKalendarzu.Text, out int calendarPos))
                _pokoj.CalendarPosition = calendarPos;

            if (_panelService == null)
                throw new InvalidOperationException("Panel Service nie jest dostępny");

            if (_isEditMode)
            {
                var success = await _panelService.UpdatePokoj(_pokoj.Id, _pokoj);
                if (success)
                {
                    await DisplayAlert("Sukces", "Pokój został zaktualizowany", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie udało się zaktualizować pokoju", "OK");
                }
            }
            else
            {
                var createdPokoj = await _panelService.CreatePokoj(_pokoj);
                if (createdPokoj != null)
                {
                    await DisplayAlert("Sukces", "Pokój został dodany", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie udało się dodać pokoju", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się zapisać pokoju: {ex.Message}", "OK");
        }
    }
}
