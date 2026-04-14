using System.Collections.ObjectModel;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Blokady;

public partial class BlokadyPage : ContentPage
{
    ObservableCollection<Blokada> blokady = new();

    private IBlockadeService _blockadeService;
    private IAuthService _authService;

    public BlokadyPage()
    {
        InitializeComponent();

        BlokadyList.ItemsSource = blokady;

        _blockadeService = IPlatformApplication.Current.Services.GetService<IBlockadeService>();
        _authService = IPlatformApplication.Current.Services.GetService<IAuthService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadBlockades();
    }

    private async Task ReloadBlockades()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return;

        var blockades = await _blockadeService.FetchBlockadesAsync(token);

        blokady.Clear();

        foreach (var dto in blockades)
        {
            blokady.Add(new Blokada
            {
                Id = dto.Id,
                Nazwa = dto.Name,
                Notatka = dto.Notes,
                DataOd = DateTime.Parse(dto.ApplyFromDate),
                DataDo = DateTime.Parse(dto.ApplyToDate),
                DlaWszystkich = dto.Rooms == null || dto.Rooms.Count == 0,
                Pokoje = dto.Rooms?.Select(r => r.ShortName).ToList() ?? new()
            });
        }
    }

    async void OnDodajClicked(object sender, EventArgs e)
    {
        var page = new BlokadyFormPage();
        await Navigation.PushModalAsync(page);

        page.Disappearing += async (s, ev) =>
        {
            if (page.Result == null) return;

            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return;

            var availableRooms = await _blockadeService.FetchBlockadeRoomsAsync(token);

            var dto = new BlockadeDto
            {
                Name = page.Result.Nazwa,
                Notes = page.Result.Notatka,
                ApplyFromDate = page.Result.DataOd.ToString("yyyy-MM-dd"),
                ApplyToDate = page.Result.DataDo.ToString("yyyy-MM-dd"),

                Rooms = page.Result.DlaWszystkich
                    ? new List<BlockadeRoomDto>()
                    : page.Result.Pokoje
                        .Select(roomName =>
                        {
                            var room = availableRooms.FirstOrDefault(r =>
                                r.ShortName == roomName || r.Name == roomName);

                            return new BlockadeRoomDto
                            {
                                RoomId = room?.RoomId ?? 0,
                                Name = room?.Name ?? roomName,
                                ShortName = room?.ShortName ?? roomName
                            };
                        })
                        .Where(r => r.RoomId > 0)
                        .ToList()
            };

            await _blockadeService.CreateBlockadeAsync(token, dto);

            await ReloadBlockades();
        };
    }

    async void OnBlokadaTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var blokada = frame?.BindingContext as Blokada;
        if (blokada == null) return;

        string action = await DisplayActionSheet(
            blokada.Nazwa,
            "Anuluj",
            null,
            "Edytuj",
            "Usuń");

        if (action == "Usuń")
        {
            var confirm = await DisplayAlert(
                "Usuń blokadę",
                $"Czy na pewno usunąć: {blokada.Nazwa}?",
                "Tak",
                "Nie");

            if (!confirm)
                return;

            var token = await _authService.GetTokenAsync();

            if (!string.IsNullOrEmpty(token) && blokada.Id.HasValue)
            {
                var ok = await _blockadeService.DeleteBlockadeAsync(token, blokada.Id.Value);

                if (!ok)
                {
                    await DisplayAlert("Błąd", "Nie udało się usunąć blokady w API", "OK");
                    return;
                }
            }

            blokady.Remove(blokada);
            return;
        }

        if (action == "Edytuj")
        {
            var page = new BlokadyFormPage(blokada);
            await Navigation.PushModalAsync(page);

            page.Disappearing += async (s, ev) =>
            {
                if (page.Result == null || !blokada.Id.HasValue) return;

                var token = await _authService.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return;

                var availableRooms = await _blockadeService.FetchBlockadeRoomsAsync(token);

                var dto = new BlockadeDto
                {
                    Id = blokada.Id.Value,
                    Name = page.Result.Nazwa,
                    Notes = page.Result.Notatka,
                    ApplyFromDate = page.Result.DataOd.ToString("yyyy-MM-dd"),
                    ApplyToDate = page.Result.DataDo.ToString("yyyy-MM-dd"),

                    Rooms = page.Result.DlaWszystkich
                        ? new List<BlockadeRoomDto>()
                        : page.Result.Pokoje
                            .Select(roomName =>
                            {
                                var room = availableRooms.FirstOrDefault(r =>
                                    r.ShortName == roomName || r.Name == roomName);

                                return new BlockadeRoomDto
                                {
                                    RoomId = room?.RoomId ?? 0,
                                    Name = room?.Name ?? roomName,
                                    ShortName = room?.ShortName ?? roomName
                                };
                            })
                            .Where(r => r.RoomId > 0)
                            .ToList()
                };

                await _blockadeService.UpdateBlockadeAsync(token, blokada.Id.Value, dto);

                await ReloadBlockades();
            };
        }
    }
}