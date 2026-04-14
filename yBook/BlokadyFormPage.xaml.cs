using yBook.Models;
using yBook.Helpers;
using yBook.Services;

namespace yBook.Views.Blokady;

public partial class BlokadyFormPage : ContentPage
{
    public Blokada Result { get; private set; }

    List<BlockadeRoomDto> wszystkiePokoje = new();
    List<int> wybrane = new();

    private readonly IBlockadeService _blockadeService;
    private string _token;

    private bool _initialized = false; // 🔥 KLUCZOWE

    public BlokadyFormPage(Blokada blokada = null)
    {
        InitializeComponent();

        DataOdPicker.Date = DateTime.Now;
        DataDoPicker.Date = DateTime.Now.AddDays(1);

        _blockadeService = IPlatformApplication.Current.Services.GetService<IBlockadeService>();

        _ = InitializeAsync(blokada);
    }

    private async Task InitializeAsync(Blokada blokada)
    {
        if (_initialized) return; // 🔥 blokuje ponowne ładowanie
        _initialized = true;

        var authService = IPlatformApplication.Current.Services.GetService<IAuthService>();
        _token = await authService.GetTokenAsync();

        await LoadRoomsFromApiAsync();

        if (blokada != null)
        {
            NazwaEntry.Text = blokada.Nazwa;
            NotatkaEditor.Text = blokada.Notatka;
            DataOdPicker.Date = blokada.DataOd;
            DataDoPicker.Date = blokada.DataDo;
            WszystkieCheck.IsChecked = blokada.DlaWszystkich;

            if (blokada.Pokoje != null && blokada.Pokoje.Count > 0)
                wybrane = ConvertPokojeToRoomIds(blokada.Pokoje);
            else
                wybrane = new List<int>();
        }

        GenerujPokoje();
    }

    private async Task LoadRoomsFromApiAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            LoadHardcodedRooms();
            return;
        }

        var rooms = await _blockadeService.FetchBlockadeRoomsAsync(_token);

        if (rooms.Any())
            wszystkiePokoje = rooms.OrderBy(r => r.RoomId).ToList();
        else
            LoadHardcodedRooms();
    }

    private void LoadHardcodedRooms()
    {
        wszystkiePokoje = PokojeRepo.Lista
            .Select(p => new BlockadeRoomDto
            {
                RoomId = p.Id,
                Name = p.Id.ToString(),
                ShortName = p.Id.ToString(),
                FullName = p.Nazwa
            })
            .OrderBy(r => r.RoomId)
            .ToList();
    }

    private List<int> ConvertPokojeToRoomIds(List<string> pokoje)
    {
        var roomIds = new List<int>();

        foreach (var pokoj in pokoje)
        {
            var found = wszystkiePokoje
                .FirstOrDefault(r => r.Name == pokoj || r.ShortName == pokoj);

            if (found != null)
            {
                roomIds.Add(found.RoomId);
            }
            else if (int.TryParse(pokoj, out var id))
            {
                var foundById = wszystkiePokoje
                    .FirstOrDefault(r => r.RoomId == id);

                if (foundById != null)
                    roomIds.Add(foundById.RoomId);
            }
        }

        return roomIds;
    }

    void GenerujPokoje()
    {
        System.Diagnostics.Debug.WriteLine("GENERUJE POKOJE");

        PokojeContainer.Children.Clear();

        foreach (var pok in wszystkiePokoje)
        {
            var isSelected = wybrane.Contains(pok.RoomId);

            var btn = new Button
            {
                Text = pok.ShortName,
                CornerRadius = 20,
                BackgroundColor = isSelected ? Colors.LightGreen : Colors.LightGray,
                Margin = 4
            };

            var roomId = pok.RoomId;

            btn.Clicked += (s, e) =>
            {
                if (wybrane.Contains(roomId))
                {
                    wybrane.Remove(roomId);
                    btn.BackgroundColor = Colors.LightGray;
                }
                else
                {
                    wybrane.Add(roomId);
                    btn.BackgroundColor = Colors.LightGreen;
                }

                // 🔥 debug – zobacz czy stan się zgadza
                System.Diagnostics.Debug.WriteLine($"Wybrane: {string.Join(",", wybrane)}");
            };

            PokojeContainer.Children.Add(btn);
        }
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        var pokojeNames = wybrane
            .Select(id => wszystkiePokoje
                .FirstOrDefault(r => r.RoomId == id)?.ShortName ?? id.ToString())
            .ToList();

        if (pokojeNames.Count == 1 && string.IsNullOrEmpty(pokojeNames[0]))
            pokojeNames = new List<string>();

        Result = new Blokada
        {
            Nazwa = NazwaEntry.Text,
            Notatka = NotatkaEditor.Text,
            DataOd = DataOdPicker.Date ?? DateTime.Now,
            DataDo = DataDoPicker.Date ?? DateTime.Now.AddDays(1),
            DlaWszystkich = WszystkieCheck.IsChecked,
            Pokoje = pokojeNames
        };

        await Navigation.PopModalAsync();
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}