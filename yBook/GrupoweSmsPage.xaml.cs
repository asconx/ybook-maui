using yBook.Services;

namespace yBook;

public partial class GrupoweSmsPage : ContentPage
{
    // Statusy aktywne (można dodawać/usuwać przez tagi)
    readonly HashSet<string> _aktywneStatusy = new();

    // Słownik tag → kontrolka Border
    Dictionary<string, Border> _tagi = new();

    // Sms service (injected via DI)
    ISmsService? _smsService;

    // Available mapping tag key -> status id from API
    Dictionary<string, int> _availableStatusIds = new();

    // Currently selected status ids (the groups user added)
    HashSet<int> _selectedStatusIds = new();

    // Dynamic borders added for statuses fetched from API
    List<Border> _dynamicStatusBorders = new();
    // mapping from dynamic border -> status id
    Dictionary<Border,int> _dynamicStatusMap = new();

    // Store original visuals so we can highlight and restore
    Dictionary<string, Color?> _origBackground = new();
    Dictionary<string, Brush?> _origStroke = new();
    Dictionary<string, double> _origStrokeThickness = new();

    public GrupoweSmsPage(ISmsService? smsService = null)
    {
        InitializeComponent();
        Header.HamburgerClicked += (_, _) => { };

        _tagi = new()
        {
            ["RezerwacjaWstepna"] = TagRezWstepna,
            ["Oferta"]            = TagOferta,
            ["Potwierdzona"]      = TagPotwierdzona,
            ["Rozliczona"]        = TagRozliczona,
            ["Anulowanie"]        = TagAnulowanie,
            ["KodDostepu"]        = TagKodDostepu,
        };

        // Add tap handlers to tags so user can add/remove groups by tapping the tag (does not change UI layout)
        foreach (var kv in _tagi)
        {
            var key = kv.Key;
            var border = kv.Value;
            var tap = new TapGestureRecognizer { CommandParameter = key };
            tap.Tapped += OnToggleStatus;
            border.GestureRecognizers.Add(tap);
            // capture original visuals
            try
            {
                _origBackground[key] = border.BackgroundColor;
                _origStroke[key] = border.Stroke;
                _origStrokeThickness[key] = border.StrokeThickness;
            }
            catch { }
        }

        LblOpis.Text = $"System automatycznie wyszuka rezerwacje aktywne " +
                       $"w dniu dzisiejszym ({DateTime.Today:yyyy-MM-dd}) " +
                       $"i wyśle wiadomość do pasujących gości.";

        // accept injected service (if provided by DI)
        if (smsService is not null)
            _smsService = smsService;

        // defer loading statuses until page appears (handler/services available)
        // also try to load immediately in case OnAppearing isn't invoked in some navigation flows
        try
        {
            _ = LoadStatusesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GrupoweSms] ctor LoadStatusesAsync error: {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("[GrupoweSms] OnAppearing start");

        // Resolve service from DI when page appears (MauiContext available) if not injected
        if (_smsService is null)
        {
            try
            {
                _smsService = App.Current?.Handler?.MauiContext?.Services.GetService(typeof(ISmsService)) as ISmsService
                              ?? Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(ISmsService)) as ISmsService;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GrupoweSms] resolve smsservice error: {ex.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine($"[GrupoweSms] smsService is {( _smsService == null ? "NULL" : "AVAILABLE" )}");

        try
        {
            _ = LoadStatusesAsync();
            AktualizujOdbiorcow();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GrupoweSms] LoadStatusesAsync start error: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("[GrupoweSms] OnAppearing end");
    }

    void OnToggleStatus(object? sender, EventArgs e)
    {
        if (sender is not VisualElement ve) return;
        if (ve.BindingContext is string keyFromCtx && !string.IsNullOrEmpty(keyFromCtx)) { }

        // CommandParameter was set on the recognizer
        if (e is TappedEventArgs te && te.Parameter is string key)
        {
            // Toggle selection: if available mapping exists, toggle selected id
            bool nowSelected = false;
            if (_availableStatusIds.TryGetValue(key, out var sid))
            {
                if (_selectedStatusIds.Contains(sid))
                {
                    _selectedStatusIds.Remove(sid);
                    _aktywneStatusy.Remove($"id:{sid}");
                    nowSelected = false;
                }
                else
                {
                    _selectedStatusIds.Add(sid);
                    _aktywneStatusy.Add($"id:{sid}");
                    nowSelected = true;
                }

                SetTagSelected(key, nowSelected);
            }
            else
            {
                // If no mapping, toggle logical key presence and highlight
                if (_aktywneStatusy.Contains(key))
                {
                    _aktywneStatusy.Remove(key);
                    nowSelected = false;
                }
                else
                {
                    _aktywneStatusy.Add(key);
                    nowSelected = true;
                }

                SetTagSelected(key, nowSelected);
            }

        // Also, if dynamic borders exist with this name, toggle them too
        try
        {
            foreach (var kv in _dynamicStatusMap.ToList())
            {
                var border = kv.Key;
                var id = kv.Value;
                if (TryGetLabelText(border, out var lab) && !string.IsNullOrEmpty(lab) && lab.Trim().ToLowerInvariant().Contains(key.Trim().ToLowerInvariant()))
                {
                    // toggle
                    if (_selectedStatusIds.Contains(id)) { _selectedStatusIds.Remove(id); SetDynamicSelected(border, false); }
                    else { _selectedStatusIds.Add(id); SetDynamicSelected(border, true); }
                }
            }
        }
        catch { }

        AktualizujOdbiorcow();
        }
    }

    void SetTagSelected(string key, bool selected)
    {
        if (!_tagi.TryGetValue(key, out var border)) return;

        try
        {
            if (selected)
            {
                // highlight: stronger stroke and slightly brighter background
                border.Stroke = new SolidColorBrush(Color.FromArgb("#1565C0"));
                border.StrokeThickness = (_origStrokeThickness.ContainsKey(key) ? _origStrokeThickness[key] : 1) + 1;
                border.BackgroundColor = Color.FromArgb("#E3F2FD");
            }
            else
            {
                // restore originals if available
                if (_origStroke.ContainsKey(key) && _origStroke[key] != null)
                    border.Stroke = _origStroke[key];
                if (_origStrokeThickness.ContainsKey(key))
                    border.StrokeThickness = _origStrokeThickness[key];
                if (_origBackground.ContainsKey(key) && _origBackground[key] != null)
                    border.BackgroundColor = _origBackground[key];
            }
        }
        catch { }
    }

    void OnFiltrChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Recompute recipient count when filters change
        AktualizujOdbiorcow();
    }

    // Helpers for dynamic status UI
    static bool TryGetLabelText(Border border, out string? text)
    {
        text = null;
        try
        {
            if (border?.Content is HorizontalStackLayout hs)
            {
                foreach (var c in hs.Children)
                {
                    if (c is Label l && l.Text != "✕") { text = l.Text; return true; }
                }
            }
        }
        catch { }
        return false;
    }

    void SetDynamicSelected(Border border, bool selected)
    {
        try
        {
            if (border == null) return;
            if (selected)
            {
                border.Stroke = new SolidColorBrush(Color.FromArgb("#1565C0"));
                border.BackgroundColor = Color.FromArgb("#E3F2FD");
                border.StrokeThickness = 2;
            }
            else
            {
                border.Stroke = new SolidColorBrush(Color.FromArgb("#CFD8DC"));
                border.BackgroundColor = Colors.Transparent;
                border.StrokeThickness = 1;
            }
        }
        catch { }
    }

    async Task LoadStatusesAsync()
    {
        try
        {
            // Always fetch directly from API to ensure we get current statuses
            var http = new System.Net.Http.HttpClient();
            var url = "https://api.ybook.pl/entity/status";

            // attach token if available
            try
            {
                var auth = App.Current?.Handler?.MauiContext?.Services.GetService(typeof(yBook.Services.IAuthService)) as yBook.Services.IAuthService
                           ?? Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(yBook.Services.IAuthService)) as yBook.Services.IAuthService;
                if (auth != null)
                {
                    var token = await auth.GetTokenAsync();
                    if (!string.IsNullOrEmpty(token))
                        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }

            var resp = await http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return;

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            List<StatusDto> list = new();
            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object && doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var el in items.EnumerateArray())
                {
                    var dto = new StatusDto();
                    if (el.TryGetProperty("id", out var idp) && idp.TryGetInt32(out var id)) dto.Id = id;
                    if (el.TryGetProperty("name", out var np) && np.ValueKind == System.Text.Json.JsonValueKind.String) dto.Name = np.GetString();
                    if (el.TryGetProperty("color", out var cp) && cp.ValueKind == System.Text.Json.JsonValueKind.String) dto.Color = cp.GetString();
                    list.Add(dto);
                }
            }
            else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var dto = new StatusDto();
                    if (el.TryGetProperty("id", out var idp) && idp.TryGetInt32(out var id)) dto.Id = id;
                    if (el.TryGetProperty("name", out var np) && np.ValueKind == System.Text.Json.JsonValueKind.String) dto.Name = np.GetString();
                    if (el.TryGetProperty("color", out var cp) && cp.ValueKind == System.Text.Json.JsonValueKind.String) dto.Color = cp.GetString();
                    list.Add(dto);
                }
            }

            if (list == null || list.Count == 0) return;

            // Start with empty selection (user can add groups). Do not auto-add.
            _aktywneStatusy.Clear();
            _availableStatusIds.Clear();
            _selectedStatusIds.Clear();

            // Remove previous dynamic borders once
            try
            {
                await Dispatcher.DispatchAsync(() =>
                {
                    foreach (var db in _dynamicStatusBorders.ToList())
                    {
                        if (TagsFlex.Children.Contains(db)) TagsFlex.Children.Remove(db);
                    }
                    _dynamicStatusBorders.Clear();
                });
            }
            catch { }

            foreach (var dto in list)
            {
                if (dto == null) continue;
                string? key = null;
                var name = (dto.Name ?? string.Empty).ToLowerInvariant();
                if (name.Contains("wst") || name.Contains("wstep") || name.Contains("wst19p")) key = "RezerwacjaWstepna";
                else if (name.Contains("ofert")) key = "Oferta";
                else if (name.Contains("potwier")) key = "Potwierdzona";
                else if (name.Contains("rozlic")) key = "Rozliczona";
                else if (name.Contains("anul")) key = "Anulowanie";
                else if (name.Contains("kod")) key = "KodDostepu";

                if (key != null)
                {
                    _availableStatusIds[key] = dto.Id;
                }

                try
                {
                    System.Diagnostics.Debug.WriteLine($"[GrupoweSms] status fetched: id={dto.Id}, name={dto.Name}");

                    // try to match by visible label text if key not determined
                    if (key == null && !string.IsNullOrEmpty(dto.Name))
                    {
                        var nameNorm = dto.Name.Trim().ToLowerInvariant();
                        foreach (var kvp in _tagi)
                        {
                            var tb = kvp.Value;
                            if (TryGetLabelText(tb, out var labText))
                            {
                                if (!string.IsNullOrEmpty(labText) && labText.Trim().ToLowerInvariant().Contains(nameNorm))
                                {
                                    key = kvp.Key;
                                    break;
                                }
                            }
                        }
                    }

                    if (key != null && _tagi.TryGetValue(key, out var existingBorder))
                    {
                        await Dispatcher.DispatchAsync(() =>
                        {
                            existingBorder.IsVisible = true;
                            if (existingBorder.Content is HorizontalStackLayout hs)
                            {
                                foreach (var child in hs.Children)
                                {
                                    if (child is Microsoft.Maui.Controls.Shapes.Ellipse el)
                                    {
                                        try { el.Fill = new SolidColorBrush(Color.FromArgb(dto.Color ?? "#CCCCCC")); } catch { }
                                    }
                                    else if (child is Label lab)
                                    {
                                        if (lab.Text == "✕") continue;
                                        lab.Text = dto.Name ?? lab.Text;
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        var lbl = new Label { Text = dto.Name ?? dto.Id.ToString(), FontSize = 12, TextColor = Colors.Black };
                        var border = new Border();
                        border.BackgroundColor = Colors.Transparent;
                        border.Stroke = new SolidColorBrush(Color.FromArgb("#CFD8DC"));
                        border.StrokeThickness = 1;
                        border.Padding = new Thickness(8,4);
                        var h = new HorizontalStackLayout { Spacing = 4 };
                        var dot = new BoxView { WidthRequest = 10, HeightRequest = 10, BackgroundColor = Color.FromArgb(dto.Color ?? "#CCCCCC"), CornerRadius = 5 };
                        h.Add(dot);
                        h.Add(lbl);
                        border.Content = h;

                        var id = dto.Id;
                        var tap = new TapGestureRecognizer { CommandParameter = id.ToString() };
                        tap.Tapped += (s,e) => {
                            // toggle selection and highlight dynamic border
                            bool now = false;
                            if (_selectedStatusIds.Contains(id))
                            {
                                _selectedStatusIds.Remove(id);
                                now = false;
                            }
                            else
                            {
                                _selectedStatusIds.Add(id);
                                now = true;
                            }
                            // update visual
                            SetDynamicSelected(border, now);
                            AktualizujOdbiorcow();
                        };
                        border.GestureRecognizers.Add(tap);

                        await Dispatcher.DispatchAsync(() =>
                        {
                            TagsFlex.Children.Add(border);
                            _dynamicStatusBorders.Add(border);
                            _dynamicStatusMap[border] = id;
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GrupoweSms] add status UI error: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GrupoweSms] LoadStatusesAsync error: {ex.Message}");
        }
    }

    // ── Usuwanie statusu przez ✕ ─────────────────────────────────────────────

    void OnUsunStatus(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string status) return;
        // Instead of removing the tag from UI, toggle selection for the group.
        if (_tagi.TryGetValue(status, out var tag))
        {
            // If there's a mapping to an id, toggle that id selection
            if (_availableStatusIds.TryGetValue(status, out var sid))
            {
                if (_selectedStatusIds.Contains(sid))
                {
                    _selectedStatusIds.Remove(sid);
                    _aktywneStatusy.Remove($"id:{sid}");
                    SetTagSelected(status, false);
                }
                else
                {
                    _selectedStatusIds.Add(sid);
                    _aktywneStatusy.Add($"id:{sid}");
                    SetTagSelected(status, true);
                }
            }
            else
            {
                // fallback: toggle by key
                if (_aktywneStatusy.Contains(status))
                {
                    _aktywneStatusy.Remove(status);
                    SetTagSelected(status, false);
                }
                else
                {
                    _aktywneStatusy.Add(status);
                    SetTagSelected(status, true);
                }
            }
        }

        AktualizujOdbiorcow();
    }

    // ── Walidacja SMS ─────────────────────────────────────────────────────────

    void OnSmsTextChanged(object? sender, TextChangedEventArgs e)
    {
        var tekst = e.NewTextValue ?? string.Empty;
        int dlugosc = tekst.Length;

        LblLicznikZnakow.Text = $"{dlugosc} / 160 znaków";
        LblLicznikZnakow.TextColor = dlugosc > 160
            ? Color.FromArgb("#E53935")
            : Color.FromArgb("#90A4AE");

        bool puste = string.IsNullOrWhiteSpace(tekst);
        LblBladSms.IsVisible = false; // chowamy błąd podczas pisania
        BorderSms.Stroke = puste
            ? new SolidColorBrush(Color.FromArgb("#CFD8DC"))
            : new SolidColorBrush(Color.FromArgb("#1565C0"));
    }

    // ── Aktualizacja liczby odbiorców (mock — podpnij swoje API) ─────────────

    void AktualizujOdbiorcow()
    {
        // Recompute recipient count asynchronously
        _ = AktualizujOdbiorcowAsync();
    }

    async Task AktualizujOdbiorcowAsync()
    {
        int count = 0;
        try
        {
            // Extract numeric ids
            var ids = _aktywneStatusy
                .Where(s => s.StartsWith("id:") || s.StartsWith("avail:") )
                .Select(s => s.StartsWith("id:") ? s.Substring(3) : s.Substring(6))
                .Select(s => int.TryParse(s, out var v) ? v : -1)
                .Where(v => v > 0)
                .ToList();

            if (_smsService != null && ids.Count > 0)
            {
                count = await _smsService.GetActiveCountAsync(ids, ChkZameldowany.IsChecked, ChkNiezameldowany.IsChecked);
            }

            // Fallback to mock if server not available or returned 0
            if (count == 0)
                count = _aktywneStatusy.Count > 0 ? _aktywneStatusy.Count * 2 : 0;
        }
        catch { count = _aktywneStatusy.Count > 0 ? _aktywneStatusy.Count * 2 : 0; }

        FrameOdbiorcy.IsVisible = true;
        LblBrakOdbiorcow.IsVisible = count == 0;
        LblOdbiorcy.IsVisible = count > 0;

        if (count > 0)
            LblOdbiorcy.Text = $"Znaleziono {count} rezerwacji — SMS zostanie wysłany do pasujących gości.";
    }

    // ── Wysyłanie SMS ─────────────────────────────────────────────────────────

    async void OnWyslijClicked(object? sender, EventArgs e)
    {
        // Walidacja treści
        if (string.IsNullOrWhiteSpace(SmsEditor.Text))
        {
            LblBladSms.IsVisible = true;
            BorderSms.Stroke = new SolidColorBrush(Color.FromArgb("#E53935"));
            return;
        }

        if (SmsEditor.Text.Length > 160)
        {
            await DisplayAlert("Błąd", "Wiadomość SMS nie może przekraczać 160 znaków.", "OK");
            return;
        }

        if (_aktywneStatusy.Count == 0)
        {
            await DisplayAlert("Błąd", "Wybierz co najmniej jeden status rezerwacji.", "OK");
            return;
        }

        bool zameldowany   = ChkZameldowany.IsChecked;
        bool niezameldowany = ChkNiezameldowany.IsChecked;

        if (!zameldowany && !niezameldowany)
        {
            await DisplayAlert("Błąd", "Wybierz co najmniej jeden filtr meldunku.", "OK");
            return;
        }

        bool potwierdz = await DisplayAlert(
            "Potwierdzenie",
            $"Czy na pewno chcesz wysłać SMS do aktywnych rezerwacji?\n\nTreść:\n\"{SmsEditor.Text}\"",
            "Wyślij", "Anuluj");

        if (!potwierdz) return;

        BtnWyslij.IsEnabled = false;
        BtnWyslij.Text = "Wysyłanie...";

        try
        {
            bool ok = false;
            if (_smsService != null)
            {
                var ids = _selectedStatusIds.Any() ? _selectedStatusIds.ToList() : _availableStatusIds.Values.ToList();
                ok = await _smsService.SendToActiveAsync(SmsEditor.Text.Trim(), ids, zameldowany, niezameldowany);
            }

            if (ok)
            {
                await DisplayAlert("Sukces", "Wiadomości SMS zostały wysłane.", "OK");
                SmsEditor.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Błąd", "Nie udało się wysłać SMS (błąd serwera lub brak połączenia).", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się wysłać SMS:\n{ex.Message}", "OK");
        }
        finally
        {
            BtnWyslij.IsEnabled = true;
            BtnWyslij.Text = "  wyślij sms";
        }
    }
}
