using System.Text.Json;
using yBook.Services;

namespace yBook.Views.Pakiety;

public partial class PakietFormPage : ContentPage
{
    // =========================================
    //                VARIABLES
    // =========================================
    private Pakiet? _pakiet;
    private bool _modif;
    private Action? _refresh;
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    private List<string> _elementy = new();

    // Callback — wywoływany po udanym zapisie
    public Action<Pakiet>? OnSaved { get; set; }

    // =========================================
    //                   START
    // =========================================
    public PakietFormPage(IAuthService authService, Pakiet? pakiet = null, bool modif = false, Action? refresh = null)
    {
        InitializeComponent();

        _authService = authService;
        _httpClient  = new HttpClient();
        _pakiet      = pakiet ?? new Pakiet();
        _modif       = modif;
        _refresh     = refresh;

        // Ustaw domyślne wartości pickerów
        WalutaPicker.SelectedIndex = 0; // PLN
        TypCenyPicker.SelectedIndex = 0;
        VatPicker.SelectedIndex = 2;    // 8%

        DataOdPicker.Date = DateTime.Today;
        DataDoPicker.Date = DateTime.Today.AddMonths(3);

        if (modif && pakiet != null)
            FillForm(pakiet);
        else
            LblTytul.Text = "Nowy pakiet";

        RefreshElementyUI();
    }

    // =========================================
    //            FILL FORM (edycja)
    // =========================================
    private void FillForm(Pakiet p)
    {
        LblTytul.Text        = "Edytuj pakiet";
        NazwaEntry.Text      = p.Nazwa;
        OpisEditor.Text      = p.Opis;
        ZdjecieUrlEntry.Text = p.ZdjecieUrl;
        CenaEntry.Text       = p.Cena > 0 ? p.Cena.ToString("F2") : "";

        if (!string.IsNullOrEmpty(p.ZdjecieUrl))
            ShowImagePreview(p.ZdjecieUrl);

        if (!string.IsNullOrEmpty(p.DataOd) &&
            DateTime.TryParse(p.DataOd, out var od))
            DataOdPicker.Date = od;

        if (!string.IsNullOrEmpty(p.DataDo) &&
            DateTime.TryParse(p.DataDo, out var doo))
            DataDoPicker.Date = doo;
    }

    // =========================================
    //                  EVENTS
    // =========================================

    // ── Bezterminowy toggle ──
    void OnBezterminowyToggled(object sender, ToggledEventArgs e)
    {
        DatyContainer.IsVisible = !e.Value;
    }

    // ── Limit toggle ──
    void OnLimitToggled(object sender, ToggledEventArgs e)
    {
        LimitContainer.IsVisible = e.Value;
    }

    // ── URL zdjęcia wpisany ──
    void OnZdjecieUrlChanged(object sender, TextChangedEventArgs e)
    {
        var url = e.NewTextValue?.Trim();
        if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            ShowImagePreview(url);
    }

    // ── Wybierz zdjęcie z galerii ──
    async void OnWybierzZdjecieClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Wybierz zdjęcie pakietu"
            });

            if (result != null)
            {
                ZdjeciePlaceholder.IsVisible = false;
                ZdjeciePreview.IsVisible = true;
                ZdjeciePreview.Source = ImageSource.FromFile(result.FullPath);
                ZdjecieUrlEntry.Text = "";
            }
        }
        catch
        {
            await DisplayAlert("Zdjęcie", "Nie udało się otworzyć galerii.\nMożesz wkleić URL zdjęcia poniżej.", "OK");
        }
    }

    // ── Dodaj element pakietu ──
    async void OnDodajElementClicked(object sender, EventArgs e)
    {
        var element = await DisplayPromptAsync(
            "Element pakietu",
            "Co wchodzi w skład pakietu?",
            placeholder: "np. Kolacja dla 2 osób, Butelka wina...",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (!string.IsNullOrWhiteSpace(element))
        {
            _elementy.Add(element.Trim());
            RefreshElementyUI();
        }
    }

    // ── Strzałka wstecz / Anuluj ──
    async void OnAnulujClicked(object sender, EventArgs e)
    {
        bool ok = await DisplayAlert(
            "Porzucić zmiany?",
            "Niezapisane dane zostaną utracone.",
            "Tak, wyjdź",
            "Zostań");

        if (ok)
            await Shell.Current.GoToAsync("..");
    }

    // ── Zapisz ──
    async void OnZapiszClicked(object sender, EventArgs e)
    {
        // ── Walidacja ──
        if (string.IsNullOrWhiteSpace(NazwaEntry.Text))
        {
            await DisplayAlert("Wymagane pole", "Podaj nazwę pakietu.", "OK");
            NazwaEntry.Focus();
            return;
        }

        if (!decimal.TryParse(CenaEntry.Text?.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal cena) || cena < 0)
        {
            await DisplayAlert("Nieprawidłowa cena", "Podaj poprawną cenę (liczba ≥ 0).", "OK");
            CenaEntry.Focus();
            return;
        }

        if (!BezterminowySwitch.IsToggled &&
            DataDoPicker.Date < DataOdPicker.Date)
        {
            await DisplayAlert("Błędne daty", "Data do musi być późniejsza niż data od.", "OK");
            return;
        }

        // ── Zbuduj model ──
        _pakiet!.Nazwa      = NazwaEntry.Text.Trim();
        _pakiet.Opis        = OpisEditor.Text?.Trim();
        _pakiet.ZdjecieUrl  = ZdjecieUrlEntry.Text?.Trim();
        _pakiet.Cena        = cena;
        _pakiet.DataOd      = BezterminowySwitch.IsToggled
                                  ? null
                                  : DataOdPicker.Date?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        _pakiet.DataDo      = BezterminowySwitch.IsToggled
                                  ? null
                                  : DataDoPicker.Date?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        // ── Wyślij do API ──
        bool success = _modif
            ? await UpdatePakietAsync(_pakiet)
            : await AddPakietAsync(_pakiet);

        if (!success) return;

        _refresh?.Invoke();
        OnSaved?.Invoke(_pakiet);
        await Shell.Current.GoToAsync("..");
    }

    // =========================================
    //              PRIVATE HELPERS
    // =========================================

    private void ShowImagePreview(string url)
    {
        ZdjeciePlaceholder.IsVisible = false;
        ZdjeciePreview.IsVisible     = true;
        ZdjeciePreview.Source        = ImageSource.FromUri(new Uri(url));
    }

    private void RefreshElementyUI()
    {
        ElementyContainer.Children.Clear();

        foreach (var el in _elementy)
        {
            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(36) }
                },
                Margin = new Thickness(0, 0, 0, 6)
            };

            var chip = new Border
            {
                BackgroundColor = Color.FromArgb("#EEF2FF"),
                StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Stroke          = Color.FromArgb("#C5CAE9"),
                Padding         = new Thickness(12, 8)
            };

            var text = new HorizontalStackLayout { Spacing = 8 };
            text.Children.Add(new Label { Text = "✓", TextColor = Color.FromArgb("#43A047"), FontSize = 13, VerticalOptions = LayoutOptions.Center });
            text.Children.Add(new Label { Text = el, FontSize = 13, TextColor = Color.FromArgb("#1C1C1E"), VerticalOptions = LayoutOptions.Center });
            chip.Content = text;
            row.Children.Add(chip);
            Grid.SetColumn(chip, 0);

            var btnDel = new Border
            {
                BackgroundColor = Color.FromArgb("#FFEBEE"),
                StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Stroke          = Color.FromArgb("#FFCDD2"),
                Padding         = new Thickness(8),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };
            btnDel.Content = new Label { Text = "✕", FontSize = 13, TextColor = Color.FromArgb("#E53935"), HorizontalOptions = LayoutOptions.Center };

            var captured = el;
            var tapDel = new TapGestureRecognizer();
            tapDel.Tapped += (_, _) =>
            {
                _elementy.Remove(captured);
                RefreshElementyUI();
            };
            btnDel.GestureRecognizers.Add(tapDel);
            row.Children.Add(btnDel);
            Grid.SetColumn(btnDel, 1);

            ElementyContainer.Children.Add(row);
        }

        // Pokaż placeholder gdy brak elementów
        if (_elementy.Count == 0)
        {
            ElementyContainer.Children.Add(new Label
            {
                Text      = "Brak elementów — dodaj co wchodzi w skład pakietu",
                FontSize  = 12,
                TextColor = Color.FromArgb("#B0BEC5"),
                Margin    = new Thickness(0, 0, 0, 8)
            });
        }
    }

    // =========================================
    //                 API CALLS
    // =========================================
    private async Task<bool> AddPakietAsync(Pakiet p)
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji.", "OK");
                return false;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var payload = JsonSerializer.Serialize(new
            {
                name        = p.Nazwa,
                description = p.Opis,
                price       = p.Cena,
                image_url   = p.ZdjecieUrl,
                date_from   = p.DataOd,
                date_to     = p.DataDo,
                is_active   = AktywnySwitch.IsToggled ? 1 : 0,
                is_online   = OnlineSwitch.IsToggled  ? 1 : 0
            });

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.ybook.pl/entity/package");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("✅ Sukces", "Pakiet został dodany.", "OK");
                return true;
            }

            var err = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Błąd", $"Nie udało się dodać pakietu ({response.StatusCode}):\n{err}", "OK");
            return false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Błąd połączenia: {ex.Message}", "OK");
            return false;
        }
    }

    private async Task<bool> UpdatePakietAsync(Pakiet p)
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("Błąd", "Brak autoryzacji.", "OK");
                return false;
            }

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var payload = JsonSerializer.Serialize(new
            {
                name        = p.Nazwa,
                description = p.Opis,
                price       = p.Cena,
                image_url   = p.ZdjecieUrl,
                date_from   = p.DataOd,
                date_to     = p.DataDo,
                is_active   = AktywnySwitch.IsToggled ? 1 : 0,
                is_online   = OnlineSwitch.IsToggled  ? 1 : 0
            });

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put,
                $"https://api.ybook.pl/entity/package/{p.Id}");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("✅ Sukces", "Pakiet został zaktualizowany.", "OK");
                return true;
            }

            var err = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Błąd", $"Nie udało się zaktualizować ({response.StatusCode}):\n{err}", "OK");
            return false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Błąd połączenia: {ex.Message}", "OK");
            return false;
        }
    }
}
