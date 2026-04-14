using System.Reflection.PortableExecutable;
using yBook.Models;
using yBook.Services;
using System;

namespace yBook.Views.ICalendar
{
    public partial class ICalendarPage : ContentPage
    {
        // simple backing fields for financial calculations
        decimal _sumNoclegi = 0m;
        decimal _extraFees = 0m;
        decimal _totalPrice = 0m;

        public ICalendarPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();

            // fill some pickers with sample data
            PickerVehicleType.ItemsSource = new List<string> { "Osobowy", "Motocykl", "Dostawczy" };
            PickerStatus.ItemsSource = new List<string> { "Nowa", "Potwierdzona", "Zameldowana", "Anulowana" };

            // sample totals (would normally come from reservation data)
            _sumNoclegi = 500m;
            _extraFees = 50m;
            _totalPrice = _sumNoclegi + _extraFees;

            LblNoclegiSum.Text = _sumNoclegi.ToString("C0");
            LblExtraFees.Text = _extraFees.ToString("C0");
            LblTotalPrice.Text = _totalPrice.ToString("C0");
            UpdateRemaining();

            // populate sample notifications and action log
            CvNotifications.ItemsSource = new[] { "Powiadomienie SMS - 2026-04-10", "E-mail - 2026-04-11" };
            CvActionLog.ItemsSource = new[] {
                new { Date = "2026-04-14 15:31:38", Title = "Aktualizacja rezerwacji", Details = "status: dddd, użytkownik: Test" }
            };
        }

        public void OnDodajClicked(object sender, EventArgs e)
        {
            // existing add button - could open a new reservation form
            DisplayAlert("Dodaj", "Funkcja dodawania jeszcze niezaimplementowana.", "OK");
        }

        void OnToggleNotifications(object? sender, EventArgs e)
        {
            NotificationsContainer.IsVisible = !NotificationsContainer.IsVisible;
            LblNotificationsChevron.Text = NotificationsContainer.IsVisible ? "˅" : "˄";
        }

        void OnToggleActionLog(object? sender, EventArgs e)
        {
            ActionLogContainer.IsVisible = !ActionLogContainer.IsVisible;
            LblActionLogChevron.Text = ActionLogContainer.IsVisible ? "˅" : "˄";
        }

        void OnToggleParking(object? sender, EventArgs e)
        {
            ParkingContainer.IsVisible = !ParkingContainer.IsVisible;
            LblParkingChevron.Text = ParkingContainer.IsVisible ? "˅" : "˄";
        }

        void OnAppointmentClicked(object? sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is Reservation r)
            {
                ShowReservation(r);
            }
            else if (sender is Button b2 && b2.BindingContext is Reservation r2)
            {
                ShowReservation(r2);
            }
        }

        void OnAppointmentTapped(object? sender, EventArgs e)
        {
            if (sender is Frame f && f.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer t && t.CommandParameter is Reservation r)
            {
                ShowReservation(r);
                return;
            }

            if (sender is Frame f2 && f2.BindingContext is Reservation r2)
            {
                ShowReservation(r2);
                return;
            }
        }

        void ShowReservation(Reservation r)
        {
            // header
            LblReservationHeader.Text = $"Rezerwacja ({r.Id})";

            // date created
            if (DateTime.TryParse(r.DateCreated, out var dt))
            {
                LblDateCreated.Text = dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                LblDateCreated.Text = r.DateCreated ?? "";
            }

            // source
            LblSource.Text = $"Źródło: ICAL:booking{r.Id}";

            // fill notifications sample
            CvNotifications.ItemsSource = new string[] { "Powiadomienie 1", "Powiadomienie 2" };

            // set selected status if possible
            if (r.StatusId >= 0 && r.StatusId < PickerStatus.Items.Count)
                PickerStatus.SelectedIndex = r.StatusId;

            // set info fields
            EntryNotes.Text = r.Notes;
            EntryInfoForNotify.Text = r.InfoForClient;

            // show panel with slide animation
            ReservationPanelRoot.IsVisible = true;
            ReservationPanel.TranslationX = 360;
            ReservationPanel.TranslateTo(0, 0, 240, Easing.CubicOut);
        }

        async void OnCloseReservationPanel(object sender, EventArgs e)
        {
            await ReservationPanel.TranslateTo(360, 0, 200, Easing.CubicIn);
            ReservationPanelRoot.IsVisible = false;
        }

        void OnAddTermClicked(object? sender, EventArgs e)
        {
            // gather form data and simulate adding a term
            var status = PickerStatus.SelectedItem?.ToString() ?? "";
            var uwagi = EntryNotes.Text ?? "";
            var info = EntryInfoForNotify.Text ?? "";

            // TODO: save to backend
            DisplayAlert("Dodano termin", $"Status: {status}\nUwagi: {uwagi}\nInfo: {info}", "OK");

            // Create a calendar reservation and notify calendar view
            var rnd = new Random();
            var rez = new KalendarzRezerwacja
            {
                Id = 7000 + rnd.Next(1, 999),
                DataOd = DateTime.Today,
                DataDo = DateTime.Today.AddDays(1),
                Opis = string.IsNullOrWhiteSpace(uwagi) ? status : $"{status}: {uwagi}",
            };

            // For now attach to room id 1 (adjust as needed)
            CalendarEvents.RaiseReservationAdded(rez, 1);
            // close panel
            ReservationPanelRoot.IsVisible = false;
        }

        void OnSaveParkingClicked(object? sender, EventArgs e)
        {
            var vehicle = PickerVehicleType.SelectedItem?.ToString() ?? "";
            var plate = EntryPlate.Text ?? "";
            var term = LblParkingTerm?.Text ?? "";

            // TODO: save parking info
            DisplayAlert("Parking", $"Pojazd: {vehicle}\nRej: {plate}\nTermin: {term}", "OK");
        }

        void OnDownloadCardClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Karta", "Pobieranie karty...", "OK");
        }

        void OnCheckInClicked(object? sender, EventArgs e)
        {
            // simulate check-in
            DisplayAlert("Zameldowanie", "Gość zostanie zameldowany.", "OK");
        }

        void OnDiscountChanged(object? sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(EntryDiscountPercent.Text, out var pct))
            {
                var discountPln = Math.Round(_totalPrice * pct / 100m, 2);
                LblDiscountPln.Text = discountPln.ToString("C0");
                UpdateRemaining();
            }
            else
            {
                LblDiscountPln.Text = "0";
                UpdateRemaining();
            }
        }

        void UpdateRemaining()
        {
            decimal pct = 0m;
            if (decimal.TryParse(EntryDiscountPercent?.Text, out var p)) pct = p;
            var discount = Math.Round(_totalPrice * pct / 100m, 2);
            decimal prepayment = 0m;
            if (decimal.TryParse(EntryPrepayment?.Text, out var pr)) prepayment = pr;

            var remaining = _totalPrice - discount - prepayment;
            if (remaining < 0) remaining = 0;
            LblRemaining.Text = remaining.ToString("C0");
        }

        void OnAddPaymentClicked(object? sender, EventArgs e)
        {
            // simple add payment simulation
            if (decimal.TryParse(EntryPrepayment.Text, out var amount))
            {
                DisplayAlert("Wpłata", $"Dodano wpłatę: {amount:C0}", "OK");
                UpdateRemaining();
            }
            else
            {
                DisplayAlert("Błąd", "Niepoprawna kwota", "OK");
            }
        }

        void OnChargesClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Obciążenia", "Historia wpłat i obciążeń...", "OK");
        }

        void OnArchiveClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Archiwizuj", "Rezerwacja została zarchiwizowana.", "OK");
        }

        void OnSaveClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Zapisz", "Zmiany zapisane.", "OK");
        }

        void OnNotifyClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Powiadomienie", "Powiadomienie wysłane.", "OK");
        }
    }

}
