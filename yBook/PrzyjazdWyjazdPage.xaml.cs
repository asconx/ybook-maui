using System.Collections.ObjectModel;
using yBook.Helpers;
using yBook.Models;

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

    public PrzyjazdWyjazdPage()
    {
        InitializeComponent();

        MonthsList.ItemsSource = miesiace;

        YearSlider.Value = selectedYear;
        YearLabel.Text = selectedYear.ToString();

        InitRoomsOnce();
        UpdateDaysOnly();

        // 🔥 ustawienie wybranego miesiąca NA KOŃCU (bez błędów)
        MonthsList.SelectedItem = miesiace[selectedMonth - 1];
    }

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
                    Data = d,
                    PrzyjazdMozliwy = true,
                    WyjazdMozliwy = true
                });
            }
        }
    }

    void OnYearSliderChanged(object sender, ValueChangedEventArgs e)
    {
        selectedYear = (int)e.NewValue;
        YearLabel.Text = selectedYear.ToString();

        UpdateDaysOnly();
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