using System.Reflection.PortableExecutable;
using yBook.Models;

namespace yBook.Views.ICalendar
{
    public partial class ICalendarPage : ContentPage
    {
       

        public ICalendarPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
        }

       public void OnDodajClicked(object sender, EventArgs e) 
        {

        }
    }

}
