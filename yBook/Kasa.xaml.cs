using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;

namespace yBook
{
    public partial class Kasa : ContentPage
    {
        const string HistoryUrl = "https://api.ybook.pl/cashierShift/history";

        public ObservableCollection<CashierShift> Shifts { get; } = new();

        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }

        public DateTime? CurrentStartTime => Shifts.Count > 0 ? Shifts[0].StartDate : null;

        int pageSize = 5;
        public string PagingText => $"{Math.Min(1, Shifts.Count)}- {Shifts.Count} z {Shifts.Count}";

        public Command<CashierShift> ShowDetailedReportCommand { get; }

        public Kasa()
        {
            InitializeComponent();
            BindingContext = this;
            ShowDetailedReportCommand = new Command<CashierShift>(OnShowDetailedReport);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadHistoryAsync();
        }

        async Task LoadHistoryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // Pobierz serwisy z DI przez MauiContext (bez u¿ycia Application.Current.Services)
                var services = Application.Current?.Handler?.MauiContext?.Services;
                var auth = services?.GetService<IAuthService>();
                var http = services?.GetService<HttpClient>() ?? new HttpClient();

                // Przywróæ/zweryfikuj sesjê jeœli mamy AuthService
                if (auth is not null)
                {
                    // IsAuthenticatedAsync ustawi header w AuthService._http jeœli token w SecureStorage jest wa¿ny
                    var isAuth = await auth.IsAuthenticatedAsync();
                    var token = await auth.GetTokenAsync();
                    System.Diagnostics.Debug.WriteLine($"[Kasa] IsAuthenticated: {isAuth}, token present: {!string.IsNullOrEmpty(token)}");

                    // Na wszelki wypadek — pobierz token i ustaw header w http (jeœli header dalej nie istnieje)
                    if (!string.IsNullOrEmpty(token) && http.DefaultRequestHeaders.Authorization == null)
                    {
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }

                // Wykonaj ¿¹danie korzystaj¹c z HttpClient z DI (powinien mieæ nag³ówek jeœli u¿ytkownik jest zalogowany)
                var json = await http.GetStringAsync(HistoryUrl);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var items = JsonSerializer.Deserialize<CashierShift[]>(json, options) ?? Array.Empty<CashierShift>();

                Shifts.Clear();
                foreach (var it in items)
                    Shifts.Add(it);

                OnPropertyChanged(nameof(CurrentStartTime));
                OnPropertyChanged(nameof(PagingText));
            }
            catch (HttpRequestException httpEx)
            {
                await DisplayAlert("B³¹d sieci", $"B³¹d po³¹czenia: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", $"Nie uda³o siê pobraæ historii: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        void OnShowDetailedReport(CashierShift shift)
        {
            _ = DisplayAlert("Raport", $"Szczegó³y zmiany:\nStart: {shift?.StartDateStr}\nKoniec: {shift?.FinishDateStr}\nGotówka: {shift?.BalanceCashStr}", "OK");
        }

        void OnFinishShiftClicked(object sender, EventArgs e)
        {
            _ = DisplayAlert("Zakoñcz", "Zakoñcz zmianê — funkcja do zaimplementowania.", "OK");
        }

        void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (sender is Picker p && p.SelectedIndex >= 0)
            {
                if (int.TryParse(p.Items[p.SelectedIndex], out var size))
                {
                    pageSize = size;
                    OnPropertyChanged(nameof(PagingText));
                }
            }
        }
    }
}
