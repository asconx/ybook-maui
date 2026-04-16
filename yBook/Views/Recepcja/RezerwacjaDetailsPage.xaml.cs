namespace yBook.Views.Recepcja
{
    public partial class RezerwacjaDetailsPage : ContentPage
    {
        private string _rezerwacjaId;

        public RezerwacjaDetailsPage()
        {
            InitializeComponent();
        }

        public void LoadRezerwacja(string rezerwacjaId)
        {
            _rezerwacjaId = rezerwacjaId;
            // TODO: Załaduj szczegóły rezerwacji na podstawie ID
        }
    }
}