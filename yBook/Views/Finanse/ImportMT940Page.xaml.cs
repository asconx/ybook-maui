using yBook.Models;

namespace yBook.Views.Finanse
{
    public partial class ImportMT940Page : ContentPage
    {
        public ImportMT940Page()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();

            // Rejestracja konwertera dla widoczności bloku błędu
            Resources.Add("NotNullConverter", new NotNullToBoolConverter());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ImportyList.ItemsSource = MockFinanse.ImportyMT940();
        }

        async void OnWybierzPlikClicked(object? sender, EventArgs e)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Wybierz plik MT940",
                    FileTypes   = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS,     new[] { "public.plain-text", "public.data" } },
                        { DevicePlatform.Android, new[] { "text/plain", "*/*" } },
                        { DevicePlatform.WinUI,   new[] { ".mt940", ".sta", ".txt" } },
                        { DevicePlatform.MacCatalyst, new[] { "public.plain-text" } },
                    })
                };

                var result = await FilePicker.PickAsync(options);
                if (result is null) return;

                // Symulacja przetwarzania
                await DisplayAlert(
                    "Import MT940",
                    $"Wybrano: {result.FileName}\n\nPrzetwarzanie pliku... (mock)\nWkrótce: realne parsowanie MT940.",
                    "OK");

                // W realnej implementacji: parsuj plik, wyślij do API yBook
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się otworzyć pliku: {ex.Message}", "OK");
            }
        }

        async void OnImportSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not ImportMT940Record rec) return;
            ImportyList.SelectedItem = null;

            var msg = $"Plik:       {rec.NazwaPliku}\n" +
                      $"Data:       {rec.DataStr}\n" +
                      $"Operacje:   {rec.DopasowanieStr}\n" +
                      $"Suma:       {rec.SumaStr}\n" +
                      $"Status:     {rec.StatusLabel}";

            if (rec.Blad is not null)
                msg += $"\n\nBłąd: {rec.Blad}";

            await DisplayAlert("Szczegóły importu", msg, "Zamknij");
        }
    }

    // ── Konwerter: string? != null → bool (widoczność bloku błędu) ────────────
    public class NotNullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => value is string s && !string.IsNullOrEmpty(s);

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
