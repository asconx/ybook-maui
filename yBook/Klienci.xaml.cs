using System;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Klienci
{
    public partial class KlienciPage : ContentPage
    {
        public KlienciPage()
        {
            InitializeComponent();
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {   
            var user = new User
            {
                Name = NazwaEntry.Text?.Trim(),
                Role = RolaPicker.SelectedItem?.ToString(),
                Email = EmailEntry.Text?.Trim(),
                Phone = TelefonEntry.Text?.Trim(),
                NowaPlatnosc = CbNowaPlatnosc.IsChecked,
                WyslijPowiadomienieKlient = CbWyslijPowiadomienieKlient.IsChecked,
                AnulowanieRezerwacji = CbAnulowanieRezerwacji.IsChecked,
                NowaRezerwacjaOnline = CbNowaRezerwacjaOnline.IsChecked,
                SynchronizacjaRezerwacji = CbSynchronizacjaRezerwacji.IsChecked,
                UtworzenieNowejRezerwacji = CbUtworzenieNowejRezerwacji.IsChecked
            };

            if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
            {
                await DisplayAlert("B³¹d", "Podaj imiê i e-mail.", "OK");
                return;
            }

            UserStore.Add(user);

            await Shell.Current.GoToAsync("..");
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}