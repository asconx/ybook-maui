using System.ComponentModel;

namespace yBook.Models
{
    public class PrzyjazdWyjazd : INotifyPropertyChanged
    {
        public int PokojId { get; set; }
        public string Pokoj { get; set; }
        public DateTime Data { get; set; }

        // Powiązanie z API
        public int? AvailabilityId { get; set; } // id rekordu z API (może być null dla lokalnych)

        bool przyjazdMozliwy = true;
        public bool PrzyjazdMozliwy
        {
            get => przyjazdMozliwy;
            set
            {
                if (przyjazdMozliwy != value)
                {
                    przyjazdMozliwy = value;
                    OnPropertyChanged(nameof(PrzyjazdMozliwy));
                }
            }
        }

        bool wyjazdMozliwy = true;
        public bool WyjazdMozliwy
        {
            get => wyjazdMozliwy;
            set
            {
                if (wyjazdMozliwy != value)
                {
                    wyjazdMozliwy = value;
                    OnPropertyChanged(nameof(WyjazdMozliwy));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}