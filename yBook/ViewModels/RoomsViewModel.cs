using System.Collections.ObjectModel;
using System.Threading.Tasks;
using yBook.Models.Api;
using yBook.Services.Api;

namespace yBook.ViewModels
{
    /// <summary>
    /// Prosty ViewModel, który pobiera listę pokoi z API i udostępnia ją do powiązań w UI
    /// </summary>
    public class RoomsViewModel
    {
        private readonly RoomApiService _apiService;

        public ObservableCollection<Room> Rooms { get; } = new ObservableCollection<Room>();

        public RoomsViewModel(RoomApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadAsync(string bearerToken)
        {
            var items = await _apiService.GetRoomsAsync(bearerToken);
            Rooms.Clear();
            foreach (var r in items)
                Rooms.Add(r);
        }
    }
}
