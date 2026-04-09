using Microcharts;
using SkiaSharp;
using yBook.Models;
using System.Linq;

namespace yBook.Views.Przyjazdy
{
    public partial class PrzyjazdWyjazdPage : ContentPage
    {
        public List<PrzyjazdWyjazd> PrzyjazdyWyjazdy { get; set; }

        public PrzyjazdWyjazdPage()
        {
            InitializeComponent();
            PrzyjazdyWyjazdy = GetSampleData(); // Twoje dane – np. z bazy
            LoadChart();
        }

        void LoadChart()
        {
            int liczbaPrzyjazdow = PrzyjazdyWyjazdy.Count(p => p.PrzyjazdMozliwy);
            int liczbaWyjazdow = PrzyjazdyWyjazdy.Count(p => p.WyjazdMozliwy);

            var entries = new[]
            {
                new ChartEntry(liczbaPrzyjazdow)
                {
                    Label = "Przyjazdy",
                    ValueLabel = liczbaPrzyjazdow.ToString(),
                    Color = SKColor.Parse("#4DB6AC")
                },
                new ChartEntry(liczbaWyjazdow)
                {
                    Label = "Wyjazdy",
                    ValueLabel = liczbaWyjazdow.ToString(),
                    Color = SKColor.Parse("#FF8A65")
                }
            };

            ArrivalsDeparturesChart.Chart = new DonutChart
            {
                Entries = entries,
                HoleRadius = 0.5f,
                LabelTextSize = 32
            };
        }

        List<PrzyjazdWyjazd> GetSampleData()
        {
            return new List<PrzyjazdWyjazd>
            {
                new PrzyjazdWyjazd { PokojId = 1, Pokoj = "101", Data = DateTime.Today, PrzyjazdMozliwy = true, WyjazdMozliwy = false },
                new PrzyjazdWyjazd { PokojId = 2, Pokoj = "102", Data = DateTime.Today, PrzyjazdMozliwy = false, WyjazdMozliwy = true },
                new PrzyjazdWyjazd { PokojId = 3, Pokoj = "103", Data = DateTime.Today, PrzyjazdMozliwy = true, WyjazdMozliwy = true },
            };
        }
    }
}
