using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using yBook.Services.Api;
using yBook.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace yBook.Views
{
    public partial class RecepcjaPage : ContentPage
    {
        VerticalStackLayout BookingList;
        Label LblStatus;
        ActivityIndicator Loader;
        readonly ActiveReservationService? _activeService;

        public RecepcjaPage(ActiveReservationService activeService)
        {
            // service injected by DI
            _activeService = activeService ?? throw new ArgumentNullException(nameof(activeService));

            // build simple booking list UI
            BookingList = new VerticalStackLayout { Spacing = 10, Padding = 12 };
            LblStatus = new Label { Text = "", FontSize = 14 };
            Loader = new ActivityIndicator { IsRunning = false, IsVisible = false };

            var col = new VerticalStackLayout { Children = { LblStatus, Loader, BookingList } };
            var scroll = new ScrollView { Content = col };
            Content = scroll;

            // Load data
            _ = LoadAsync();
        }

        async Task LoadAsync()
        {
            try
            {
                Loader.IsVisible = Loader.IsRunning = true;
                LblStatus.Text = "Ładowanie...";

                var (data, error) = await _activeService.GetActiveReservationsAsync();
                if (!string.IsNullOrEmpty(error) || data == null)
                {
                    LblStatus.Text = "Błąd: " + (error ?? "brak danych");
                    return;
                }

                LblStatus.Text = "Dzisiaj: " + data.Today;

                BookingList.Children.Clear();
                foreach (var r in data.Reservations)
                {
                    var lines = new List<string>
                    {
                        $"Rezerwacja nr. {r.Id}",
                        r.ClientName,
                        r.RoomName,
                        $"Termin:",
                        $"{r.DateFrom:yyyy-MM-dd} - {r.DateTo:yyyy-MM-dd}"
                    };

                    var frame = MakeBookingBlock(lines);
                    frame.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () => await Navigation.PushAsync(new RecepcjaDetailPage(lines)))
                    });

                    BookingList.Add(frame);
                }
            }
            catch (Exception ex)
            {
                LblStatus.Text = "Błąd ładowania: " + ex.Message;
            }
            finally
            {
                Loader.IsVisible = Loader.IsRunning = false;
            }
        }

        Frame MakeBookingBlock(List<string> lines)
        {
            var frame = new Frame
            {
                CornerRadius = 8,
                Padding = 12,
                HasShadow = true,
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#E0E0E0")
            };

            var col = new VerticalStackLayout { Spacing = 4 };
            foreach (var l in lines)
            {
                col.Children.Add(new Label { Text = l, FontSize = 14, TextColor = Color.FromArgb("#263238") });
            }

            frame.Content = col;
            return frame;
        }

        // Selection now opens RecepcjaDetailPage; no in-place selection logic here.
    }
}
