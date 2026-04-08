using System.Collections.ObjectModel;
using yBook.Helpers;
using yBook.Models;

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

    public PrzyjazdWyjazdPage()
    {
        InitializeComponent();

        MonthsList.ItemsSource = miesiace;

        YearSlider.Value = selectedYear;
        YearLabel.Text = selectedYear.ToString();

        InitRoomsOnce();
        UpdateDaysOnly();
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

            UpdateDaysOnly();
        } catch
        {

        }
    }

    void OnPrevYear(object sender, EventArgs e)
    {
        selectedYear--;
        ApplyYear();
    }

    void OnNextYear(object sender, EventArgs e)
    {
        selectedYear++;
        ApplyYear();
    }

    void ApplyYear()
    {
        YearLabel.Text = selectedYear.ToString();
        YearSlider.Value = selectedYear;

        UpdateDaysOnly();
    }

    void OnMonthSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
            return;

        var selected = e.CurrentSelection[0] as string;

        if (string.IsNullOrEmpty(selected))
            return;

        selectedMonth = miesiace.IndexOf(selected) + 1;

        UpdateDaysOnly();
    }

    void OnArrivalTapped(object sender, EventArgs e)
    {
        if (sender is Label lbl && lbl.BindingContext is PrzyjazdWyjazd item)
        {
            item.PrzyjazdMozliwy = !item.PrzyjazdMozliwy;
        }
    }

    void OnDepartureTapped(object sender, EventArgs e)
    {
        if (sender is Label lbl && lbl.BindingContext is PrzyjazdWyjazd item)
        {
            item.WyjazdMozliwy = !item.WyjazdMozliwy;
        }
    }
}