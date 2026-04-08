using yBook.Models;

namespace yBook.Views.Blokady;

public partial class RezerwacjaOnlineFormPage : ContentPage
{
    public RezerwacjaOnline? Wynik { get; private set; }

    readonly RezerwacjaOnline? _edytowana;
    readonly TaskCompletionSource _tcs = new();
    public Task WaitForResultAsync() => _tcs.Task;

    int _aktywnyszablon = 1;

    static readonly List<string> Pokoje = new()
    {
        "Czujnik temperatury — recepcja",
        "Villa Orłowska", "Villa Orłowska", "Villa Orłowska",
        "Villa Orłowska", "Villa Orłowska", "Villa Orłowska",
        "Villa Orłowska", "Villa Orłowska", "Villa Orłowska",
        "Villa Orłowska", "Villa Orłowska", "Villa Orłowska",
        "Villa Orłowska", "Villa Orłowska",
    };

    readonly List<string> _wybranePokoje = new();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tcs.TrySetResult();
    }

    public RezerwacjaOnlineFormPage(RezerwacjaOnline? rez = null)
    {
        InitializeComponent();
        _edytowana = rez;

        TerminOdPicker.Date = rez?.PoczatkowyTerminOd ?? new DateTime(2024, 9, 1);
        TerminDoPicker.Date = rez?.PoczatkowyTerminDo ?? new DateTime(2024, 9, 10);

        CzcionkaPicker.SelectedIndex    = 0;
        RozliczeniePicker.SelectedIndex = 0;

        GenerujPokoje();
        PodlaczKoloryLive();

        if (rez is null) return;

        TytulLabel.Text                 = "Edycja rezerwacji online";
        SlugEntry.Text                  = rez.Slug;
        NazwaPrzedplatyEntry.Text       = rez.NazwaPrzedplaty;
        GrupowanieSwitch.IsToggled      = rez.Grupowanie;
        OpcjaFakturySwitch.IsToggled    = rez.OpcjaFaktury;

        int ci = CzcionkaPicker.Items.IndexOf(rez.Czcionka);
        if (ci >= 0) CzcionkaPicker.SelectedIndex = ci;

        int ri = RozliczeniePicker.Items.IndexOf(rez.Rozliczenie);
        if (ri >= 0) RozliczeniePicker.SelectedIndex = ri;
    void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // Wymuś: wyjazd >= przyjazd + 1 dzień
        var przyjazd = DataPrzyjazduPicker.Date ?? DateTime.Today;
        if (!DataWyjazdPicker.Date.HasValue || DataWyjazdPicker.Date.Value <= przyjazd)
            DataWyjazdPicker.Date = przyjazd.AddDays(1);

        AktualizujSkrypt(rez.Slug);
    }

    // ── Checkboxy pokoi ───────────────────────────────────────────────────────

    void GenerujPokoje()
    {
        foreach (var pokoj in Pokoje)
        var przyjazd = DataPrzyjazduPicker.Date ?? DateTime.Today;
        var wyjazd = DataWyjazdPicker.Date ?? przyjazd.AddDays(1);
        int noce = Math.Max(0, (wyjazd - przyjazd).Days);
        NoceLabel.Text = noce switch
        {
            var row = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 2) };
            var cb  = new CheckBox();
            var lbl = new Label
            {
                Text              = pokoj,
                FontSize          = 13,
                VerticalOptions   = LayoutOptions.Center,
                TextColor         = Color.FromArgb("#455A64")
            };
            cb.CheckedChanged += (s, e) =>
            {
                if (e.Value) _wybranePokoje.Add(pokoj);
                else         _wybranePokoje.Remove(pokoj);
            };
            row.Children.Add(cb);
            row.Children.Add(lbl);
            PokojeContainer.Children.Add(row);
        }
    }

    // ── Podgląd kolorów na żywo ───────────────────────────────────────────────

    void PodlaczKoloryLive()
    {
        KolorPodstawowyEntry.TextChanged += (s, e) => UstawKolor(KolorPodstawowyPodglad, e.NewTextValue);
        KolorTlaEntry.TextChanged        += (s, e) => UstawKolor(KolorTlaPodglad,        e.NewTextValue);
        KolorPrzyciskuEntry.TextChanged  += (s, e) => UstawKolor(KolorPrzyciskuPodglad,  e.NewTextValue);
        KolorTekstuEntry.TextChanged     += (s, e) => UstawKolor(KolorTekstuPodglad,     e.NewTextValue);
    }

    static void UstawKolor(Border podglad, string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return;
        try { podglad.BackgroundColor = Color.FromArgb(hex.StartsWith('#') ? hex : "#" + hex); }
        catch { /* ignoruj niepełny hex */ }
    }

    // ── Szablony ──────────────────────────────────────────────────────────────

    void OnSzablonClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not string s) return;
        if (!int.TryParse(s, out int nr)) return;

        _aktywnyszablon = nr;
        LblSzablonPodglad.Text = $"Szablon {nr} — aktywny";

        // Styl przycisków zakładek
        foreach (var (btn, i) in new[] { (BtnSzablon1,1),(BtnSzablon2,2),(BtnSzablon3,3),(BtnSzablon4,4) })
        {
            btn.BackgroundColor = i == nr ? Color.FromArgb("#1565C0") : Colors.Transparent;
            btn.TextColor       = i == nr ? Colors.White : Color.FromArgb("#607D8B");
        }
    }

    // ── Skrypt ────────────────────────────────────────────────────────────────

    void AktualizujSkrypt(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            LblSkrypt.Text = "— (zapisz, aby wygenerować skrypt) —";
            return;
        }
        LblSkrypt.Text =
            $"<div id=\"js-online-reservation\"></div> " +
            $"<script src=\"https://api.ybook.pl/online-reservation-autoscroll.js\"></script> " +
            $"<script>getOnlineReservation('{slug}', 'js-online-reservation', 'pl');</script>";
    }

    // ── Zapis ─────────────────────────────────────────────────────────────────

    async void OnZapiszClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SlugEntry.Text))
        {
            await Shell.Current.DisplayAlert("Błąd", "Pole Slug jest wymagane.", "OK");
            return;
        }

        var slug = SlugEntry.Text.Trim().ToLower();
        if (DataWyjazdPicker.Date.GetValueOrDefault() <= DataPrzyjazduPicker.Date.GetValueOrDefault())
        { await WyswietlBlad("Data wyjazdu musi być późniejsza niż data przyjazdu."); return; }

        Wynik = new RezerwacjaOnline
        {
            Id             = _edytowana?.Id           ?? Guid.NewGuid().ToString("N")[..8].ToUpper(),
            DataZlozenia   = _edytowana?.DataZlozenia ?? DateTime.Now,
            Status         = _edytowana?.Status       ?? StatusRezerwacji.Oczekujaca,

            Slug               = slug,
            Czcionka           = CzcionkaPicker.SelectedItem?.ToString()      ?? "DM Sans",
            NazwaPrzedplaty    = string.IsNullOrWhiteSpace(NazwaPrzedplatyEntry.Text)
                                     ? "Zadatek" : NazwaPrzedplatyEntry.Text.Trim(),
            Rozliczenie        = RozliczeniePicker.SelectedItem?.ToString()   ?? "Na miejscu",
            Grupowanie         = GrupowanieSwitch.IsToggled,
            OpcjaFaktury       = OpcjaFakturySwitch.IsToggled,
            PoczatkowyTerminOd = TerminOdPicker.Date,
            PoczatkowyTerminDo = TerminDoPicker.Date,
            Id             = _edytowana?.Id             ?? Guid.NewGuid().ToString("N")[..8].ToUpper(),
            DataZlozenia   = _edytowana?.DataZlozenia   ?? DateTime.Now,
            Status         = _edytowana?.Status         ?? StatusRezerwacji.Oczekujaca,

            Imie           = ImieEntry.Text.Trim(),
            Nazwisko       = NazwiskoEntry.Text.Trim(),
            Email          = EmailEntry.Text.Trim(),
            Telefon        = TelefonEntry.Text.Trim(),
            DataPrzyjazdu  = DataPrzyjazduPicker.Date ?? DateTime.Today.AddDays(1),
            DataWyjazdu    = DataWyjazdPicker.Date ?? DateTime.Today.AddDays(2),
            TypPokoju      = TypPokojuPicker.SelectedItem?.ToString() ?? string.Empty,
            LiczbaGosci    = _liczbaGosci,
            Uwagi          = UwagiEditor.Text?.Trim() ?? string.Empty
        };

        AktualizujSkrypt(slug);
        await Navigation.PopModalAsync();
    }

    async void OnAnulujClicked(object sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
