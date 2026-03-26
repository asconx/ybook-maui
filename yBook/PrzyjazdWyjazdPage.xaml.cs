using System.Collections.ObjectModel;
using yBook.Models;
using yBook.Helpers;

namespace yBook.Views.Przyjazdy;

public partial class PrzyjazdWyjazdPage : ContentPage
{
    public class PokojGrid
    {
        public string Nazwa { get; set; }
        public ObservableCollection<PrzyjazdWyjazd> Dni { get; set; } = new();
    }

    ObservableCollection<DateTime> dni = new();
    ObservableCollection<PokojGrid> pokoje = new();

    ObservableCollection<string> miesiace = new()
    {
        "Sty","Lut","Mar","Kwi","Maj","Cze",
        "Lip","Sie","Wrz","Paź","Lis","Gru"
    };

    int selectedYear = DateTime.Now.Year;
    int selectedMonth = DateTime.Now.Month;

    bool isInitializing = false;

    public PrzyjazdWyjazdPage()
    {
        InitializeComponent();

        MonthsList.ItemsSource = miesiace;

        YearSlider.Value = selectedYear;
        YearLabel.Text = selectedYear.ToString();

        InitRoomsOnce();   // 🔥 tylko raz
        UpdateDaysOnly();  // 🔥 potem tylko dni
    }

    // 🔥 TWORZY POKOJE TYLKO RAZ
    void InitRoomsOnce()
    {
        pokoje.Clear();

        foreach (var p in PokojeRepo.Lista)
        {
            pokoje.Add(new PokojGrid
            {
                Nazwa = p
            });
        }

        RoomsList.ItemsSource = pokoje;
    }

    // 🔥 TYLKO AKTUALIZUJE DNI (bez przebudowy UI)
    void UpdateDaysOnly()
    {
        dni.Clear();

        var start = new DateTime(selectedYear, selectedMonth, 1);
        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

        for (int i = 0; i < daysInMonth; i++)
            dni.Add(start.AddDays(i));

        DaysList.ItemsSource = dni;

        foreach (var pokoj in pokoje)
        {
            pokoj.Dni.Clear();

            foreach (var d in dni)
            {
                pokoj.Dni.Add(new PrzyjazdWyjazd
                {
                    Pokoj = pokoj.Nazwa,
                    Data = d
                });
            }
        }
    }

    // 🔥 DEBOUNCE SLIDERA (OGROMNY BOOST)
    CancellationTokenSource sliderCts;

    async void OnYearSliderChanged(object sender, ValueChangedEventArgs e)
    {
        sliderCts?.Cancel();
        sliderCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(200, sliderCts.Token); // debounce

            selectedYear = (int)e.NewValue;
            YearLabel.Text = selectedYear.ToString();

            UpdateDaysOnly();
        }
        catch { }
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
        if (e.CurrentSelection.FirstOrDefault() is not string selected)
            return;

        selectedMonth = miesiace.IndexOf(selected) + 1;

        UpdateDaysOnly();
    }

    async void OnCellTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var item = frame?.BindingContext as PrzyjazdWyjazd;
        if (item == null) return;

        string action = await DisplayActionSheet(
            $"{item.Pokoj}\n{item.Data:dd.MM.yyyy}",
            "Anuluj",
            null,
            "Przełącz przyjazd",
            "Przełącz wyjazd");

        if (action == "Przełącz przyjazd")
            item.PrzyjazdMozliwy = !item.PrzyjazdMozliwy;

        if (action == "Przełącz wyjazd")
            item.WyjazdMozliwy = !item.WyjazdMozliwy;
    }
}