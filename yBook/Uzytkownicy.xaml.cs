using System;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Uzytkownicy
{
    public partial class UzytkownicyPage : ContentPage
    {
        public UzytkownicyPage()
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

            // Dodaj do wspólnego store -> lista na Uzytkownicy1 zaktualizuje siê automatycznie
            UserStore.Add(user);

            // Wróæ do poprzedniej strony (Uzytkownicy1)
            await Shell.Current.GoToAsync("..");
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            // Anuluj i wróæ do poprzedniej strony
            await Shell.Current.GoToAsync("..");
        }
    }
}