using System;
using yBook.Models;

namespace yBook.Services
{
    public static class CalendarEvents
    {
        // reservation, roomId
        public static event Action<KalendarzRezerwacja, int>? ReservationAdded;

        public static void RaiseReservationAdded(KalendarzRezerwacja rez, int roomId)
        {
            ReservationAdded?.Invoke(rez, roomId);
        }
    }
}
