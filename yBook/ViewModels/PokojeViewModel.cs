using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using yBook.Application.UseCases;
using yBook.Models;

namespace yBook.ViewModels;

public partial class PokojeViewModel(GetRoomsWithImagesUseCase getRoomsWithImagesUseCase) : ObservableObject
{
    public ObservableCollection<Pokoj> Pokoje { get; } = [];

    [ObservableProperty]
    private bool isLoading;

    public async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var rooms = await getRoomsWithImagesUseCase.ExecuteAsync();
            Pokoje.Clear();
            foreach (var room in rooms)
            {
                Pokoje.Add(MapToPokoj(room));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static Pokoj MapToPokoj(yBook.Domain.Entities.Room room) => new()
    {
        Id = room.Id,
        OrganizationId = room.OrganizationId,
        PropertyId = room.PropertyId,
        DateModified = room.DateModified,
        Nazwa = room.Name,
        Type = room.Type,
        CzyDostepny = room.IsAvailable,
        MaxOsobLiczbą = room.MaxPeople,
        Powierzchnia = room.Area,
        Opis = room.Description,
        ShortName = room.ShortName,
        DefaultPrice = room.DefaultPrice,
        Kolor = room.Color,
        Standard = room.Standard,
        MinOsobLiczbą = room.MinPeople,
        LockId = room.LockId,
        CalendarPosition = room.CalendarPosition,
        ImageUrl = room.ImageUrl,
        PhotoFileId = room.PhotoFileId,
        BedSummary = room.BedSummary,
        AmenitySummary = room.AmenitySummary,
        BedItems = room.BedItems,
        AmenityItems = room.AmenityItems,
        PropertyName = room.PropertyName,
        PropertyAddress = room.PropertyAddress,
        PriceModifierId = room.PriceModifierId
    };
}
