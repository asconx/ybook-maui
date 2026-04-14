using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using yBook.Models;

namespace yBook.ViewModels
{
    public class ReservationViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Room> Rooms { get; } = new ObservableCollection<Room>();

        DateTime _from = DateTime.Today;
        DateTime _to = DateTime.Today.AddDays(1);
        int _adults = 1;
        int _children = 0;
        int _infants = 0;
        bool _extraBed = false;
        Room _selectedRoom;
        decimal _discountPercent = 0m;
        decimal _prepayment = 0m;

        public ReservationViewModel()
        {
            // mock rooms
            Rooms.Add(new Room("r1", "Pokój Standard", 200));
            Rooms.Add(new Room("r2", "Pokój Deluxe", 350));
            Rooms.Add(new Room("r3", "Apartament", 500));
            SelectedRoom = Rooms[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public DateTime From { get => _from; set { _from = value; Raise(); Raise(nameof(Nights)); Raise(nameof(TotalNightsPrice)); Raise(nameof(TotalPrice)); } }
        public DateTime To { get => _to; set { _to = value; Raise(); Raise(nameof(Nights)); Raise(nameof(TotalNightsPrice)); Raise(nameof(TotalPrice)); } }
        public int Adults { get => _adults; set { _adults = Math.Max(1, value); Raise(); } }
        public int Children { get => _children; set { _children = Math.Max(0, value); Raise(); } }
        public int Infants { get => _infants; set { _infants = Math.Max(0, value); Raise(); } }
        public bool ExtraBed { get => _extraBed; set { _extraBed = value; Raise(); Raise(nameof(TotalPrice)); } }
        public Room SelectedRoom { get => _selectedRoom; set { _selectedRoom = value; Raise(); Raise(nameof(TotalNightsPrice)); Raise(nameof(TotalPrice)); } }
        public decimal DiscountPercent { get => _discountPercent; set { _discountPercent = value; Raise(); Raise(nameof(TotalPrice)); } }
        public decimal Prepayment { get => _prepayment; set { _prepayment = value; Raise(); Raise(nameof(TotalPrice)); } }

        public int Nights => Math.Max(0, (To - From).Days);
        public decimal TotalNightsPrice => SelectedRoom != null ? SelectedRoom.PricePerNight * Nights : 0m;
        public decimal ExtraBedCost => ExtraBed ? 50m * Nights : 0m;
        public decimal DiscountAmount => (TotalNightsPrice + ExtraBedCost) * (DiscountPercent / 100m);
        public decimal TotalPrice => Math.Max(0, TotalNightsPrice + ExtraBedCost - DiscountAmount - Prepayment);

        // helpers for UI
        public string NightsText => $"{Nights} nocy";
    }
}
