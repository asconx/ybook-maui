using System;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Uzytkownicy
{
    public partial class UzytkownicyPage : ContentPage
    {
        private readonly User? _editingUser;

        public UzytkownicyPage(User? editingUser = null)
        {
            InitializeComponent();

            _editingUser = editingUser;

            if (_editingUser != null)
            {
                // Populate fields for editing
                NazwaEntry.Text = _editingUser.Name;
                EmailEntry.Text = _editingUser.Email;
                TelefonEntry.Text = _editingUser.Phone;
                if (!string.IsNullOrEmpty(_editingUser.Role))
                    RolaPicker.SelectedItem = _editingUser.Role;

                CbNowaPlatnosc.IsChecked = _editingUser.NowaPlatnosc;
                CbWyslijPowiadomienieKlient.IsChecked = _editingUser.WyslijPowiadomienieKlient;
                CbAnulowanieRezerwacji.IsChecked = _editingUser.AnulowanieRezerwacji;
                CbNowaRezerwacjaOnline.IsChecked = _editingUser.NowaRezerwacjaOnline;
                CbSynchronizacjaRezerwacji.IsChecked = _editingUser.SynchronizacjaRezerwacji;
                CbUtworzenieNowejRezerwacji.IsChecked = _editingUser.UtworzenieNowejRezerwacji;
            }
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

            if (_editingUser != null)
            {
                // Update existing user in place
                _editingUser.Name = user.Name;
                _editingUser.Role = user.Role;
                _editingUser.Email = user.Email;
                _editingUser.Phone = user.Phone;
                _editingUser.NowaPlatnosc = user.NowaPlatnosc;
                _editingUser.WyslijPowiadomienieKlient = user.WyslijPowiadomienieKlient;
                _editingUser.AnulowanieRezerwacji = user.AnulowanieRezerwacji;
                _editingUser.NowaRezerwacjaOnline = user.NowaRezerwacjaOnline;
                _editingUser.SynchronizacjaRezerwacji = user.SynchronizacjaRezerwacji;
                _editingUser.UtworzenieNowejRezerwacji = user.UtworzenieNowejRezerwacji;
            }
            else
            {
                // Dodaj do wspólnego store -> lista na Uzytkownicy1 zaktualizuje siê automatycznie
                UserStore.Add(user);
            }

            // Wróæ do poprzedniej strony (Uzytkownicy1)
            if (Navigation.ModalStack.Count > 0 && Navigation.ModalStack[^1] == this)
                await Navigation.PopModalAsync();
            else
                await Shell.Current.GoToAsync("..");
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            // Anuluj i wróæ do poprzedniej strony
            if (Navigation.ModalStack.Count > 0 && Navigation.ModalStack[^1] == this)
                await Navigation.PopModalAsync();
            else
                await Shell.Current.GoToAsync("..");
        }
    }
}