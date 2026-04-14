using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace yBook.Views.ICalendar
{
    public partial class ICalendarPage : ContentPage
    {
        
        public ObservableCollection<CalendarItem> Items { get; set; } = new();

       
        private List<CalendarItem> AllItems = new();

        private CancellationTokenSource _cts;

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
                AllItems.Add(result);
                FilterItems("");
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
                var index = AllItems.IndexOf(item);
                AllItems[index] = result;

                FilterItems("");
            }
        }


        private void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button?.BindingContext as CalendarItem;

            if (item == null) return;

            AllItems.Remove(item);
            FilterItems("");
        }

        private async void OnSzukajChange(object sender, TextChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _cts.Token);
                var query = e.NewTextValue?.ToLower() ?? "";
                FilterItems(query);
            }
            catch
            {
                // anulowane
            }
        }


        private void FilterItems(string query)
        {
            Items.Clear();

            var filtered = AllItems.Where(x =>
                (x.Portal?.ToLower().Contains(query) ?? false) ||
                (x.Kwatera?.ToLower().Contains(query) ?? false) ||
                (x.ExportLink?.ToLower().Contains(query) ?? false) ||
                (x.ImportLink?.ToLower().Contains(query) ?? false)
            );

            foreach (var item in filtered)
                Items.Add(item);

            LblCount.Text = Items.Count.ToString();
        }
    }

    public class CalendarItem
    {
        public string Portal { get; set; }
        public string Kwatera { get; set; }
        public string ExportLink { get; set; }
        public string ImportLink { get; set; }
    }
}