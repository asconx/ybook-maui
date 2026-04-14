using System.Collections.ObjectModel;
using yBook.Helpers;
using yBook.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;
using yBook.Services;

namespace yBook.Views.Przyjazdy;

public partial class PrzyjazdWyjazdPage : ContentPage
{
    public class PokojGrid
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public ObservableCollection<PrzyjazdWyjazd> Dni { get; set; } = new();
    }

    ObservableCollection<DateTime> dni = new();
    ObservableCollection<PokojGrid> pokoje = new();

    Dictionary<string, List<PokojGrid>> cache = new();

    ObservableCollection<string> miesiace = new()
    {
        "Sty","Lut","Mar","Kwi","Maj","Cze",
        "Lip","Sie","Wrz","Paź","Lis","Gru"
    };

    int selectedYear = DateTime.Now.Year;
    int selectedMonth = DateTime.Now.Month;

    CancellationTokenSource sliderCts;

    private string _token; // Token użytkownika
    private readonly IAuthService _authService;

    public PrzyjazdWyjazdPage()
    {
        InitializeComponent();

        MonthsList.ItemsSource = miesiace;

        YearSlider.Value = selectedYear;
        YearLabel.Text = selectedYear.ToString();

        InitRoomsOnce();
        _authService = IPlatformApplication.Current.Services.GetService<IAuthService>();
        LoadAndSyncData();
    }

    async void LoadAndSyncData()
    {
        _token = await _authService.GetTokenAsync();
        await SyncFromApi();
    }

    async Task SyncFromApi()
    {
        if (string.IsNullOrEmpty(_token)) return;
        var apiData = await PokojeRepo.FetchArrivalDepartureAvailabilityAsync(_token, selectedYear, selectedMonth);
        // Loguj wszystkie room_id z API
        foreach (var rec in apiData)
            System.Diagnostics.Debug.WriteLine($"API: room_id={rec.RoomId}, day={rec.Day}, can_arrive={rec.CanArrive}, can_depart={rec.CanDepart}");
        // Loguj wszystkie pokoje z repozytorium
        foreach (var pokoj in PokojeRepo.Lista)
            System.Diagnostics.Debug.WriteLine($"REPO: Id={pokoj.Id}, Nazwa={pokoj.Nazwa}");
        // Mapowanie danych z API do pokoje/dni
        dni.Clear();
        var start = new DateTime(selectedYear, selectedMonth, 1);
        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
        for (int i = 0; i < daysInMonth; i++)
            dni.Add(start.AddDays(i));
        DaysList.ItemsSource = dni;
        pokoje.Clear();
        foreach (var pokoj in PokojeRepo.Lista)
        {
            var grid = new PokojGrid
            {
                Id = pokoj.Id,
                Nazwa = pokoj.Nazwa
            };
            foreach (var d in dni)
            {
                var found = apiData.FirstOrDefault(x => x.RoomId == pokoj.Id && x.Day == d.ToString("yyyy-MM-dd"));
                bool przyjazd = true;
                bool wyjazd = true;
                if (found != null)
                {
                    przyjazd = found.CanArrive == 1;
                    wyjazd = found.CanDepart == 1;
                }
                System.Diagnostics.Debug.WriteLine($"room_id={pokoj.Id}, day={d:yyyy-MM-dd}, can_arrive={przyjazd}, can_depart={wyjazd}");
                grid.Dni.Add(new PrzyjazdWyjazd
                {
                    PokojId = pokoj.Id,
                    Pokoj = pokoj.Nazwa,
                    Data = d,
                    PrzyjazdMozliwy = przyjazd,
                    WyjazdMozliwy = wyjazd,
                    AvailabilityId = found?.Id
                });
            }
            pokoje.Add(grid);
        }
        RoomsList.ItemsSource = pokoje;
    }

    async Task SyncToApi()
    {
        if (string.IsNullOrEmpty(_token)) return;
        var items = pokoje.SelectMany(p => p.Dni.Select(d => new ArrivalDepartureAvailability
        {
            Id = d.AvailabilityId ?? 0,
            RoomId = d.PokojId,
            Day = d.Data.ToString("yyyy-MM-dd"),
            CanArrive = d.PrzyjazdMozliwy ? 1 : 0,
            CanDepart = d.WyjazdMozliwy ? 1 : 0
        })).ToList();
        await PokojeRepo.SaveArrivalDepartureAvailabilityAsync(_token, items);
    }

    string GetKey() => $"{selectedYear}-{selectedMonth}";

    void InitRoomsOnce()
    {
        pokoje.Clear();

        foreach (var p in PokojeRepo.Lista)
        {
            pokoje.Add(new PokojGrid
            {
                Id = p.Id,
                Nazwa = p.Nazwa
            });
        }

        RoomsList.ItemsSource = pokoje;
    }

    void UpdateDaysOnly()
    {
        dni.Clear();

        var start = new DateTime(selectedYear, selectedMonth, 1);
        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

        for (int i = 0; i < daysInMonth; i++)
            dni.Add(start.AddDays(i));

        DaysList.ItemsSource = dni;

        string key = GetKey();

        // 🔥 jeśli mamy zapis → użyj
        if (cache.ContainsKey(key))
        {
            pokoje.Clear();
            foreach (var p in cache[key])
                pokoje.Add(p);

            return;
        }

        // 🔥 jeśli nie → generuj nowe
        var newData = new List<PokojGrid>();

        foreach (var pokoj in PokojeRepo.Lista)
        {
            var grid = new PokojGrid
            {
                Id = pokoj.Id,
                Nazwa = pokoj.Nazwa
            };

            foreach (var d in dni)
            {
                grid.Dni.Add(new PrzyjazdWyjazd
                {
                    PokojId = pokoj.Id,
                    Pokoj = pokoj.Nazwa,
                    Data = d,
                    PrzyjazdMozliwy = true,
                    WyjazdMozliwy = true
                });
            }

            newData.Add(grid);
        }

        cache[key] = newData;

        pokoje.Clear();
        foreach (var p in newData)
            pokoje.Add(p);
    }


    async void OnYearSliderChanged(object sender, ValueChangedEventArgs e)
    {
        sliderCts?.Cancel();
        sliderCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(200, sliderCts.Token);
            selectedYear = (int)e.NewValue;
            YearLabel.Text = selectedYear.ToString();
            await SyncFromApi();
        }
        catch { }
    }


    void OnPrevYear(object sender, EventArgs e)
    {
        selectedYear--;
        _ = SyncFromApi();
    }


    void OnNextYear(object sender, EventArgs e)
    {
        selectedYear++;
        _ = SyncFromApi();
    }


    void ApplyYear()
    {
        YearLabel.Text = selectedYear.ToString();
        YearSlider.Value = selectedYear;
        _ = SyncFromApi();
    }


    void OnMonthSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
            return;

        var selected = e.CurrentSelection[0] as string;

        if (string.IsNullOrEmpty(selected))
            return;

        selectedMonth = miesiace.IndexOf(selected) + 1;
        _ = SyncFromApi();
    }

    void OnArrivalTapped(object sender, EventArgs e)
    {
        if (sender is Label lbl && lbl.BindingContext is PrzyjazdWyjazd item)
        {
            item.PrzyjazdMozliwy = !item.PrzyjazdMozliwy;
            _ = PostSingleCell(item);
        }
    }

    void OnDepartureTapped(object sender, EventArgs e)
    {
        if (sender is Label lbl && lbl.BindingContext is PrzyjazdWyjazd item)
        {
            item.WyjazdMozliwy = !item.WyjazdMozliwy;
            _ = PostSingleCell(item);
        }
    }

    async Task PostSingleCell(PrzyjazdWyjazd item)
    {
        var post = new ArrivalDepartureAvailabilityPost
        {
            RoomId = item.PokojId,
            Day = item.Data.ToString("yyyy-MM-dd"),
            CanArrive = item.PrzyjazdMozliwy,
            CanDepart = item.WyjazdMozliwy
        };
        await PokojeRepo.PostSingleAvailabilityAsync(_token, post);
    }
}