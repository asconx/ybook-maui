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

                NameEntry.Text = item.Name;
                ExportLinkEntry.Text = item.ExportLink;
                Import1Entry.Text = item.Import1;
                Import2Entry.Text = item.Import2;
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
                    $"https://api.ybook.com/export/{safeName}_{Guid.NewGuid()}";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var item = editingItem ?? new CalendarItem();

            item.Name = NameEntry.Text;
            item.ExportLink = ExportLinkEntry.Text;
            item.Import1 = Import1Entry.Text;
            item.Import2 = Import2Entry.Text;

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