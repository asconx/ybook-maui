using System;
using System.Threading.Tasks;

namespace yBook.Views.ICalendar
{
    public partial class AddCalendarPopup : ContentPage
    {
        public TaskCompletionSource<CalendarItem> tcs = new();

        private CalendarItem editingItem;

        public List<string> Kwatera = new List<string>{"Mały Dom","Duży Dom","Apartament" };

        public AddCalendarPopup(CalendarItem item = null)
        {
            InitializeComponent();
            KwateraEntry.ItemsSource = Kwatera;
            if (item != null)
            {
                editingItem = item;

                PortalEntry.Text = item.Portal;
                KwateraEntry.SelectedItem = item.Kwatera;
                ExportLinkEntry.Text = item.ExportLink;
                ImportLinkEntry.Text = item.ImportLink;
            }
        }

        private void OnKwateraChanged(object sender, EventArgs e)
        {
            var selected = KwateraEntry.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(selected))
            {
                ExportLinkEntry.Text = string.Empty;
                return;
            }

            var safeName = selected.Replace(" ", "_").ToLower();

            if (string.IsNullOrWhiteSpace(ExportLinkEntry.Text))
            {
                ExportLinkEntry.Text =
                    $"https://api.ybook.pl/ical{safeName}_{Guid.NewGuid()}";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var item = editingItem ?? new CalendarItem();

            item.Portal = PortalEntry.Text;
            item.Kwatera = KwateraEntry.SelectedItem?.ToString();
            item.ExportLink = ExportLinkEntry.Text;
            item.ImportLink = ImportLinkEntry.Text;

            tcs.SetResult(item);

            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            await Navigation.PopModalAsync();
        }
    }
}