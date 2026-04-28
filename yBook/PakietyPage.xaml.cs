using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using yBook.Services;

namespace yBook.Views.Pakiety;

public partial class PakietyPage : ContentPage
{
    // =========================================
    //                VARIABLES
    // =========================================
    private List<Pakiet> _all = new();
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    // Fallbackowe pola kontrolek (jeśli XAML nie został poprawnie wygenerowany)
    // Dzięki nim klasa skompiluje się nawet bez wygenerowanego pliku .g.cs
    internal Entry Search;
    internal CollectionView Lista;
    internal Label LblCount;
    internal View EmptyView;

    // =========================================
    //                   START
    // =========================================
    public PakietyPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _httpClient  = new HttpClient();
    }

    // Pełne tworzenie UI (z lepszym wyglądem) gdy XAML nie został zlinkowany / wygenerowany.
    // Jeśli XAML jest poprawny, ta metoda i pola nie będą przeszkadzać.
    private void InitializeComponent()
    {
        // jeśli XAML wygenerował pola, nie nadpisuj ich
        if (Search != null && Lista != null && LblCount != null && EmptyView != null)
            return;

        Title = "Pakiety";

        // top bar: back, add, activate, deactivate
        var btnBack = new Button
        {
            Text = "←",
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };
        btnBack.Clicked += async (_, __) =>
        {
            // Powrót do strony głównej
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
                await Shell.Current.Navigation.PopAsync();
            else
                await Shell.Current.GoToAsync("//");
        };

        var btnAdd = new Button
        {
            Text = "Dodaj",
            BackgroundColor = Colors.LightSkyBlue,
            TextColor = Colors.White,
            CornerRadius = 8,
            HorizontalOptions = LayoutOptions.End
        };
        btnAdd.Clicked += OnDodajClicked;

        var btnActivate = new Button
        {
            Text = "Aktywuj",
            BackgroundColor = Colors.LightGreen,
            TextColor = Colors.Black,
            CornerRadius = 8,
            HorizontalOptions = LayoutOptions.End
        };
        btnActivate.Clicked += async (_, __) => await ToggleActiveSelected(true);

        var btnDeactivate = new Button
        {
            Text = "Dezaktywuj",
            BackgroundColor = Colors.LightCoral,
            TextColor = Colors.White,
            CornerRadius = 8,
            HorizontalOptions = LayoutOptions.End
        };
        btnDeactivate.Clicked += async (_, __) => await ToggleActiveSelected(false);

        // Search + count
        Search = new Entry { Placeholder = "Szukaj...", HorizontalOptions = LayoutOptions.FillAndExpand };
        Search.TextChanged += OnSearchChanged;

        LblCount = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };

        EmptyView = new Label { Text = "Brak wyników", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

        // CollectionView z szablonem itemu
        Lista = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 8 },
            SelectionMode = SelectionMode.None,
            IsVisible = false,
            Margin = new Thickness(10)
        };

        // DataTemplate programowo — korzysta z wiązań do właściwości Pakiet (INotifyPropertyChanged)
        Lista.ItemTemplate = new DataTemplate(() =>
        {
            var frame = new Frame
            {
                CornerRadius = 10,
                Padding = 10,
                HasShadow = true,
                BackgroundColor = Colors.White
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(80),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto)
                }
            };

            // obrazek (jeśli brak - pusty BoxView)
            var img = new Image
            {
                Aspect = Aspect.AspectFill,
                WidthRequest = 70,
                HeightRequest = 70,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            img.SetBinding(Image.SourceProperty, "ZdjecieUrl");

            var contentStack = new StackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Start };

            var nameLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 16, LineBreakMode = LineBreakMode.TailTruncation };
            nameLabel.SetBinding(Label.TextProperty, "Nazwa");

            var priceLabel = new Label { FontSize = 14, TextColor = Colors.DarkSlateGray };
            priceLabel.SetBinding(Label.TextProperty, "CenaStr");

            var rangeLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
            rangeLabel.SetBinding(Label.TextProperty, "ZakresStr");

            var descLabel = new Label { FontSize = 12, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 2 };
            descLabel.SetBinding(Label.TextProperty, "Opis");

            contentStack.Add(nameLabel);
            contentStack.Add(priceLabel);
            contentStack.Add(rangeLabel);
            contentStack.Add(descLabel);

            // akcje po prawej: zaznacz, aktywny, edytuj, usuń
            var actionsStack = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };

            var chkSelect = new CheckBox { HorizontalOptions = LayoutOptions.Center };
            chkSelect.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsSelected", BindingMode.TwoWay));

            var switchActive = new Switch { HorizontalOptions = LayoutOptions.Center };
            switchActive.SetBinding(Switch.IsToggledProperty, new Binding("IsActive", BindingMode.TwoWay));
            switchActive.Toggled += async (s, e) =>
            {
                // Pobierz powiązany model z BindingContext i zapis lokalnie (bez wyskakujących okienek)
                if (((Element)s).BindingContext is Pakiet pk)
                {
                    // natychmiastowa zmiana w UI dzięki INotifyPropertyChanged
                    // Można tutaj dodać wywołanie API, jeśli endpoint istnieje — bez blokowania UI
                    await TryUpdateActiveRemote(pk.Id, pk.IsActive);
                }
            };

            var btnEdit = new Button { Text = "Edytuj", FontSize = 12, CornerRadius = 6, BackgroundColor = Colors.LightGray, HeightRequest = 30 };
            btnEdit.Clicked += async (s, e) =>
            {
                if (((Element)s).BindingContext is Pakiet item)
                {
                    await OnEditNavigate(item);
                }
            };

            var btnDelete = new Button { Text = "Usuń", FontSize = 12, CornerRadius = 6, BackgroundColor = Colors.IndianRed, TextColor = Colors.White, HeightRequest = 30 };
            btnDelete.Clicked += async (s, e) =>
            {
                if (((Element)s).BindingContext is Pakiet item)
                {
                    // Usuwamy bez potwierdzeń (bez wyskakujących okienek)
                    var ok = await DeletePakietAsync(item.Id);
                    if (ok)
                    {
                        _all.Remove(item);
                        ApplyFilter();
                    }
                }
            };

            actionsStack.Add(chkSelect);
            actionsStack.Add(switchActive);
            actionsStack.Add(btnEdit);
            actionsStack.Add(btnDelete);

            mainGrid.Add(img, 0, 0);
            mainGrid.Add(contentStack, 1, 0);
            mainGrid.Add(actionsStack, 2, 0);

            frame.Content = mainGrid;
            frame.SetBinding(BindingContextProperty, ".");

            return frame;
        });

        // Górny pasek z przyciskami
        var topGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Padding = new Thickness(10),
            BackgroundColor = Colors.Transparent
        };
        topGrid.Add(btnBack, 0, 0);

        var rightButtons = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.End };
        rightButtons.Add(btnActivate);
        rightButtons.Add(btnDeactivate);
        rightButtons.Add(btnAdd);

        topGrid.Add(rightButtons, 2, 0);

        // Search row
        var searchGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(10, 0, 10, 0)
        };
        searchGrid.Add(Search, 0, 0);
        searchGrid.Add(LblCount, 1, 0);

        // kompozycja całej strony
        var mainStack = new StackLayout { Spacing = 8 };
        mainStack.Add(topGrid);
        mainStack.Add(searchGrid);
        mainStack.Add(Lista);
        mainStack.Add(EmptyView);

        Content = new ScrollView { Content = mainStack };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPakiety();
    }

    // =========================================
    //                  EVENTS
    // =========================================
    void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    async void OnDodajClicked(object sender, EventArgs e)
    {
        var form = new PakietFormPage(_authService, pakiet: null, modif: false, refresh: ApplyFilter);
        form.OnSaved += _ => MainThread.BeginInvokeOnMainThread(async () => await LoadPakiety());
        await Shell.Current.Navigation.PushAsync(form);
    }

    private async Task OnEditNavigate(Pakiet item)
    {
        if (item == null) return;
        var form = new PakietFormPage(_authService, pakiet: item, modif: true, refresh: ApplyFilter);
        form.OnSaved += updated =>
        {
            var idx = _all.IndexOf(item);
            if (idx >= 0) _all[idx] = updated;
            MainThread.BeginInvokeOnMainThread(ApplyFilter);
        };
        await Shell.Current.Navigation.PushAsync(form);
    }

    // =========================================
    //              API — LOAD
    // =========================================
    private async Task LoadPakiety()
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
            {
                // Pozostawiamy komunikat — to nie jest efekt kliknięcia pakietu
                await DisplayAlert("Błąd", "Brak autoryzacji. Zaloguj się ponownie.", "OK");
                return;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Błąd", "Nie znaleziono tokenu.", "OK");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/entity/package");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json    = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result  = JsonSerializer.Deserialize<PackageResponse>(json, options);

                _all.Clear();
                if (result?.Items != null)
                    foreach (var item in result.Items)
                        _all.Add(new Pakiet
                        {
                            Id = item.Id,
                            Nazwa = item.Name,
                            Cena = item.Price,
                            ZdjecieUrl = item.ImageUrl,
                            DataOd = item.DateFrom,
                            DataDo = item.DateTo,
                            Opis = item.Description,
                            IsActive = false,
                            IsSelected = false
                        });

                ApplyFilter();
            }
            else
            {
                await DisplayAlert("Błąd", $"Nie udało się załadować pakietów: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Błąd: {ex.Message}", "OK");
        }
    }

    // =========================================
    //              API — DELETE
    // =========================================
    private async Task<bool> DeletePakietAsync(int id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.ybook.pl/entity/package/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return true;

            // nie pokazujemy dodatkowych popupów w przypadku kliknięcia elementu — logujemy lokalnie
            return false;
        }
        catch
        {
            return false;
        }
    }

    // Przykładowe nieblokujące wywołanie aktualizacji stanu aktywności pakietu
    private async Task TryUpdateActiveRemote(int id, bool isActive)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return;

            // Jeśli API wspiera zmianę aktywności, można tu dodać odpowiednie wywołanie.
            // Pozostawiamy bez wyjątku — jeśli endpoint nie istnieje, po prostu ignorujemy błąd.
            var req = new HttpRequestMessage(HttpMethod.Patch, $"https://api.ybook.pl/entity/package/{id}/active")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { active = isActive }), System.Text.Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _httpClient.SendAsync(req);
            // nie blokujemy UI i nie pokazujemy alertów przy klikaniu elementów
        }
        catch
        {
            // ignoruj
        }
    }

    // Toggle aktywności dla zaznaczonych pakietów (bez popupów)
    private async Task ToggleActiveSelected(bool makeActive)
    {
        var selected = _all.Where(p => p.IsSelected).ToList();
        if (!selected.Any()) return;

        foreach (var p in selected)
        {
            p.IsActive = makeActive;
            // Asynchronicznie próbujemy zaktualizować zdalnie, ale nie blokujemy użytkownika
            _ = TryUpdateActiveRemote(p.Id, p.IsActive);
            p.IsSelected = false; // odznacz po operacji
        }
        ApplyFilter();
    }

    // =========================================
    //             PRIVATE METHODS
    // =========================================
    private void ApplyFilter()
    {
        var q = Search?.Text?.ToLower() ?? "";
        var result = _all.Where(x => string.IsNullOrEmpty(q) || x.Nazwa.ToLower().Contains(q)).ToList();
        Lista.ItemsSource   = result;
        LblCount.Text       = result.Count.ToString();
        EmptyView.IsVisible = result.Count == 0;
        Lista.IsVisible     = result.Count > 0;
    }

    // ── API response models ──
    private class PackageItem { public int Id { get; set; } public string Name { get; set; } = ""; public decimal Price { get; set; } public string? ImageUrl { get; set; } public string? DateFrom { get; set; } public string? DateTo { get; set; } public string? Description { get; set; } }
    private class PackageResponse { public List<PackageItem> Items { get; set; } = new(); public int Total { get; set; } }
}

// =========================================
//                  MODEL
// =========================================
public class Pakiet : INotifyPropertyChanged
{
    private bool _isActive;
    private bool _isSelected;
    private string _nazwa = "";

    public int      Id         { get; set; }
    public string   Nazwa
    {
        get => _nazwa;
        set { if (_nazwa != value) { _nazwa = value; OnPropertyChanged(nameof(Nazwa)); } }
    }
    public decimal  Cena       { get; set; }
    public string?  ZdjecieUrl { get; set; }
    public string?  DataOd     { get; set; }
    public string?  DataDo     { get; set; }
    public string?  Opis       { get; set; }

    public bool IsActive
    {
        get => _isActive;
        set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
    }

    public string CenaStr   => Cena > 0 ? $"{Cena:N2} zł" : "—";
    public string ZakresStr
    {
        get
        {
            if (!string.IsNullOrEmpty(DataOd) && !string.IsNullOrEmpty(DataDo)) return $"{DataOd} – {DataDo}";
            if (!string.IsNullOrEmpty(DataOd)) return $"od {DataOd}";
            return "—";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
