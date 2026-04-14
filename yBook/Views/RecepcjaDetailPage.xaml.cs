using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using yBook.Models;
using Microsoft.Maui.Storage;
using yBook.ViewModels;

namespace yBook.Views
{
    public partial class RecepcjaDetailPage : ContentPage
    {
        ReservationViewModel Vm => BindingContext as ReservationViewModel;

        public RecepcjaDetailPage()
        {
            InitializeComponent();

            // Use viewmodel from resources if present, otherwise create new
            if (Resources.ContainsKey("ReservationVm"))
                BindingContext = Resources["ReservationVm"] as ReservationViewModel;
            else
                BindingContext = new ReservationViewModel();

            // Wire controls
            StepAdults.ValueChanged += (s, e) => { Vm.Adults = (int)e.NewValue; LblAdults.Text = Vm.Adults.ToString(); };
            StepChildren.ValueChanged += (s, e) => { Vm.Children = (int)e.NewValue; LblChildren.Text = Vm.Children.ToString(); };
            StepInfants.ValueChanged += (s, e) => { Vm.Infants = (int)e.NewValue; LblInfants.Text = Vm.Infants.ToString(); };

            // Bindings
            PickerRoom.SetBinding(Picker.SelectedItemProperty, "SelectedRoom");
            DateFrom.SetBinding(DatePicker.DateProperty, "From", BindingMode.TwoWay);
            DateTo.SetBinding(DatePicker.DateProperty, "To", BindingMode.TwoWay);

            SwExtraBed.Toggled += (s, e) => { Vm.ExtraBed = e.Value; UpdateUI(); };

            DateFrom.DateSelected += (s, e) => UpdateUI();
            DateTo.DateSelected += (s, e) => UpdateUI();

            EntDiscount.TextChanged += (s, e) => ParseDiscount(EntDiscount.Text);

            Vm.PropertyChanged += Vm_PropertyChanged;

            // initial update
            UpdateUI();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // uruchom ładowanie pokoi w tle
            _ = LoadRoomsFromApiAsync();
        }

        // Ładuje pokoje z API i mapuje je do lokalnego modelu ReservationViewModel.Rooms
        async Task LoadRoomsFromApiAsync()
        {
            try
            {
                // pobierz token zapisany przez AuthService w SecureStorage
                var token = await SecureStorage.Default.GetAsync("auth_token");

                // utwórz klient serwisu API i pobierz dane
                var apiService = new Services.Api.RoomApiService(new System.Net.Http.HttpClient());
                var apiRooms = await apiService.GetRoomsAsync(token);

                if (apiRooms == null || apiRooms.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadRoomsFromApiAsync: brak pokoi z API.");
                    return;
                }

                // mapowanie API model -> lokalny model yBook.Models.Room
                Vm.Rooms.Clear();
                foreach (var r in apiRooms)
                {
                    var local = new Room(r.Id.ToString(), r.Name ?? string.Empty, r.DefaultPrice);
                    Vm.Rooms.Add(local);
                }

                // jeśli nie ma wybranego pokoju, ustaw pierwszy
                if (Vm.SelectedRoom == null && Vm.Rooms.Count > 0)
                    Vm.SelectedRoom = Vm.Rooms[0];

                UpdateUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRoomsFromApiAsync: błąd pobierania pokoi: {ex}");
            }
        }

        // New constructor overload to accept a sample booking block
        public RecepcjaDetailPage(List<string> sample) : this()
        {
            if (sample != null && sample.Count > 0)
            {
                // Show the provided sample lines in the info editor (safe because InitializeComponent ran via : this())
                try
                {
                    EditorInfo.Text = string.Join(Environment.NewLine, sample);
                }
                catch
                {
                    // ignore if EditorInfo is not available for some reason
                }
            }
        }

        void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // update relevant UI when viewmodel changes
            UpdateUI();
        }

        void ParseDiscount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Vm.DiscountPercent = 0;
                return;
            }
            text = text.Trim();
            if (text.EndsWith("%") && decimal.TryParse(text.TrimEnd('%'), out var p))
            {
                Vm.DiscountPercent = p;
            }
            else if (decimal.TryParse(text, out var amount))
            {
                // treat as absolute discount amount -> convert to percent relative to current base
                var baseAmount = Vm.TotalNightsPrice + Vm.ExtraBedCost;
                if (baseAmount > 0) Vm.DiscountPercent = (amount / baseAmount) * 100m;
            }
            UpdateUI();
        }

        void UpdateUI()
        {
            if (Vm == null) return;
            LblNights.Text = Vm.NightsText;
            LblRoomPrice.Text = "Cena za noc: " + (Vm.SelectedRoom != null ? Vm.SelectedRoom.PricePerNight.ToString("0.00") + " PLN" : "0 PLN");
            LblExtraBedCost.Text = Vm.ExtraBedCost.ToString("0.00") + " PLN";
            LblSumNights.Text = "Suma za noclegi: " + Vm.TotalNightsPrice.ToString("0.00") + " PLN";
            LblPrepayment.Text = "Przedpłata: " + Vm.Prepayment.ToString("0.00") + " PLN";
            LblTotalPrice.Text = "Cena za całość: " + Vm.TotalPrice.ToString("0.00") + " PLN";
            LblRemaining.Text = "Pozostało do zapłaty: " + Vm.TotalPrice.ToString("0.00") + " PLN";
        }

        // Uniwersalny handler przycisków: działanie zależy od tekstu na przycisku
        async void OnActionButtonClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            var txt = (btn.Text ?? string.Empty).Trim().ToLowerInvariant();
            System.Diagnostics.Debug.WriteLine($"OnActionButtonClicked: przycisk '{btn.Text}' został naciśnięty.");

            try
            {
                switch (txt)
                {
                    case "dodaj termin":
                        // proste dodanie przykładowego terminu (ustawienie dat)
                        DateFrom.Date = DateTime.Today;
                        DateTo.Date = DateTime.Today.AddDays(1);
                        UpdateUI();
                        await DisplayAlert("Termin", "Dodano przykładowy termin.", "OK");
                        break;

                    case "archiwizuj":
                        // tutaj można dodać logikę archiwizacji
                        System.Diagnostics.Debug.WriteLine("Archwizacja rezerwacji...");
                        await DisplayAlert("Archiwizuj", "Rezerwacja została zarchiwizowana.", "OK");
                        break;

                    case "powiadomienie":
                        // wyślij powiadomienie (tu tylko symulacja)
                        System.Diagnostics.Debug.WriteLine($"Powiadomienie: {EditorInfo?.Text}");
                        await DisplayAlert("Powiadomienie", "Powiadomienie zostało wysłane (symulacja).", "OK");
                        break;

                    case "zapisz":
                        // zapisz zmiany (tu tylko symulacja)
                        System.Diagnostics.Debug.WriteLine("Zapisz rezerwację (symulacja)");
                        await DisplayAlert("Zapisz", "Zapisano zmiany.", "OK");
                        break;

                    case "dodaj wpłatę":
                        // poproś o kwotę i zaktualizuj przedpłatę
                        var amountStr = await DisplayPromptAsync("Dodaj wpłatę", "Podaj kwotę:", "OK", "Anuluj", "0");
                        if (!string.IsNullOrWhiteSpace(amountStr) && decimal.TryParse(amountStr, out var a))
                        {
                            Vm.Prepayment += a;
                            UpdateUI();
                            await DisplayAlert("Wpłata", $"Dodano wpłatę: {a:0.00} PLN", "OK");
                        }
                        break;

                    case "obciążenia":
                        // otwórz listę obciążeń lub pokaż symulację
                        await DisplayAlert("Obciążenia", "Otwieranie obciążeń (symulacja).", "OK");
                        break;

                    default:
                        // domyślna obsługa: pokaż tekst przycisku
                        await DisplayAlert("Akcja", $"Wciśnięto przycisk: {btn.Text}", "OK");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd w OnActionButtonClicked: {ex}");
                await DisplayAlert("Błąd", "Wystąpił błąd podczas wykonywania akcji.", "OK");
            }
        }
    }
}
