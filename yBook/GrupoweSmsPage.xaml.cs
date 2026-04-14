namespace yBook;

public partial class GrupoweSmsPage : ContentPage
{
    // Statusy aktywne (można dodawać/usuwać przez tagi)
    readonly HashSet<string> _aktywneStatusy = new()
    {
        "RezerwacjaWstepna", "Oferta", "Potwierdzona",
        "Rozliczona", "Anulowanie", "KodDostepu"
    };

    // Słownik tag → kontrolka Border
    Dictionary<string, Border> _tagi = new();

    public GrupoweSmsPage()
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

        LblOpis.Text = $"System automatycznie wyszuka rezerwacje aktywne " +
                       $"w dniu dzisiejszym ({DateTime.Today:yyyy-MM-dd}) " +
                       $"i wyśle wiadomość do pasujących gości.";

        AktualizujOdbiorcow();
    }

    // ── Usuwanie statusu przez ✕ ─────────────────────────────────────────────

    void OnUsunStatus(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string status) return;

        _aktywneStatusy.Remove(status);

        if (_tagi.TryGetValue(status, out var tag))
            tag.IsVisible = false;

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
        // TODO: zastąp wywołaniem serwisu, np.:
        // int count = await _rezerwacjaService.LiczbaAktywnychAsync(DateTime.Today, _aktywneStatusy, ChkZameldowany.IsChecked, ChkNiezameldowany.IsChecked);

        // Na razie mock:
        int count = _aktywneStatusy.Count > 0 ? _aktywneStatusy.Count * 2 : 0;

        FrameOdbiorcy.IsVisible   = true;
        LblBrakOdbiorcow.IsVisible = count == 0;
        LblOdbiorcy.IsVisible      = count > 0;

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
            // TODO: podpnij swój serwis SMS, np.:
            // await _smsService.WyslijGrupowoAsync(
            //     SmsEditor.Text,
            //     _aktywneStatusy,
            //     zameldowany,
            //     niezameldowany,
            //     DateTime.Today);

            await Task.Delay(1000); // mock opóźnienia

            await DisplayAlert("Sukces", "Wiadomości SMS zostały wysłane.", "OK");
            SmsEditor.Text = string.Empty;
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
