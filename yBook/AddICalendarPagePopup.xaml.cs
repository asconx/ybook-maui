using System;
using System.Threading.Tasks;

namespace yBook.Views.ICalendar
{
    public partial class AddCalendarPopup : ContentPage
    {
        public TaskCompletionSource<CalendarItem> tcs = new();

        private CalendarItem editingItem;

        public AddCalendarPopup(CalendarItem item = null)
        {
            InitializeComponent();

            if (item != null)
            {
                editingItem = item;

                PortalEntry.Text = item.Portal;
                KwateraEntry.Text = item.Kwatera;
                ExportLinkEntry.Text = item.ExportLink;
                ImportLinkEntry.Text = item.ImportLink;
            }
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            var name = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(name))
            {
                ExportLinkEntry.Text = string.Empty;
                return;
            }

            var safeName = name.Replace(" ", "_").ToLower();

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
            item.Kwatera = KwateraEntry.Text;
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