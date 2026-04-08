using System.Reflection.PortableExecutable;
using yBook.Models;

public partial class AddCalendarPopup : ContentPage
{
    public string ResultName { get; private set; }
    public string ExportLink { get; private set; }

    public AddCalendarPopup()
    {
        InitializeComponent();
    }

    private void OnNameChanged(object sender, TextChangedEventArgs e)
    {
        var name = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(name))
        {
            ExportLinkEntry.Text = string.Empty;
            return;
        }

        // proste generowanie linku
        var safeName = name.Replace(" ", "_").ToLower();

        ExportLinkEntry.Text = $"https://api.ybook.com/export/{safeName}_{Guid.NewGuid()}";
    }

        public TaskCompletionSource<string> tcs = new();

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            tcs.SetResult(NameEntry.Text);
            await Navigation.PopModalAsync();
        }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

}