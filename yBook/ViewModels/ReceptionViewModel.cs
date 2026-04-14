using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace yBook.ViewModels
{
    public class ReceptionViewModel : INotifyPropertyChanged
    {
        public string Today { get; set; } = string.Empty;
        public ObservableCollection<ReceptionItemViewModel> Reservations { get; } = new ObservableCollection<ReceptionItemViewModel>();

        public event PropertyChangedEventHandler? PropertyChanged;
        void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ReceptionItemViewModel
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomShortName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsCheckedIn { get; set; }
        public bool IsPaid { get; set; }
        public bool IsUntilToday { get; set; }
        public int StatusId { get; set; }

        // helpers
        public bool IsActive { get; set; }
        public bool IsEndingToday { get; set; }
        public bool IsStartingToday { get; set; }
    }
}
