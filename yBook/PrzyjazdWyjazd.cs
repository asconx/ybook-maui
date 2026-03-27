using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yBook.Models
{
    public class PrzyjazdWyjazd : INotifyPropertyChanged
    {
        public string Pokoj { get; set; }
        public DateTime Data { get; set; }

        bool przyjazdMozliwy = true;
        public bool PrzyjazdMozliwy
        {
            get => przyjazdMozliwy;
            set
            {
                if (przyjazdMozliwy != value)
                {
                    przyjazdMozliwy = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        public string DataStr => Data.ToString("dd.MM.yyyy");

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}