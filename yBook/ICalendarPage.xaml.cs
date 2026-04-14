using System;
using System.Collections.ObjectModel;

namespace yBook.Views.ICalendar
{
    public partial class ICalendarPage : ContentPage
    {
        public ObservableCollection<CalendarItem> Items { get; set; } = new();

        public ICalendarPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public async void OnDodajClicked(object sender, EventArgs e)
        {
            var popup = new AddCalendarPopup();

            await Navigation.PushModalAsync(popup);

            var result = await popup.tcs.Task;

            if (result != null)
            {
                Items.Add(result);
                LblCount.Text = Items.Count.ToString();
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button?.BindingContext as CalendarItem;

            if (item == null) return;

            var popup = new AddCalendarPopup(item);

            await Navigation.PushModalAsync(popup);

            var result = await popup.tcs.Task;

            if (result != null)
            {
                var index = Items.IndexOf(item);

                Items.RemoveAt(index);
                Items.Insert(index, result);
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button?.BindingContext as CalendarItem;

            if (item == null) return;

            Items.Remove(item);
            LblCount.Text = Items.Count.ToString();
        }
    }

    public class CalendarItem
    {
        public string Name { get; set; }
        public string ExportLink { get; set; }
        public string Import1 { get; set; }
        public string Import2 { get; set; }
    }
}