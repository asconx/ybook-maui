using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;

namespace yBook.Views.Uzytkownicy
{
    public partial class UzytkownicyPage : ContentPage
    {
        private readonly User? _editingUser;
        private bool _notificationsExpanded = false;

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

                (FindByName("SwNowaPlatnosc") as Switch)!.IsToggled = _editingUser.NowaPlatnosc;
                (FindByName("SwWyslijPowiadomienieKlient") as Switch)!.IsToggled = _editingUser.WyslijPowiadomienieKlient;
                (FindByName("SwAnulowanieRezerwacji") as Switch)!.IsToggled = _editingUser.AnulowanieRezerwacji;
                (FindByName("SwNowaRezerwacjaOnline") as Switch)!.IsToggled = _editingUser.NowaRezerwacjaOnline;
                (FindByName("SwSynchronizacjaRezerwacji") as Switch)!.IsToggled = _editingUser.SynchronizacjaRezerwacji;
                (FindByName("SwUtworzenieNowejRezerwacji") as Switch)!.IsToggled = _editingUser.UtworzenieNowejRezerwacji;

                _notificationsExpanded = true;
                (FindByName("NotificationsContent") as VerticalStackLayout)!.IsVisible = true;
                (FindByName("NotificationsToggleBtn") as Button)!.Text = "Wybierz powiadomienia ?";
            }
        }

        void OnNotificationsToggleClicked(object sender, EventArgs e)
        {
            _notificationsExpanded = !_notificationsExpanded;
            (FindByName("NotificationsContent") as VerticalStackLayout)!.IsVisible = _notificationsExpanded;
            (FindByName("NotificationsToggleBtn") as Button)!.Text = _notificationsExpanded ? "Wybierz powiadomienia ?" : "Wybierz powiadomienia ?";
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var user = new User
            {
                Name = NazwaEntry.Text?.Trim(),
                Role = RolaPicker.SelectedItem?.ToString(),
                Email = EmailEntry.Text?.Trim(),
                Phone = TelefonEntry.Text?.Trim(),
                NowaPlatnosc = (FindByName("SwNowaPlatnosc") as Switch)!.IsToggled,
                WyslijPowiadomienieKlient = (FindByName("SwWyslijPowiadomienieKlient") as Switch)!.IsToggled,
                AnulowanieRezerwacji = (FindByName("SwAnulowanieRezerwacji") as Switch)!.IsToggled,
                NowaRezerwacjaOnline = (FindByName("SwNowaRezerwacjaOnline") as Switch)!.IsToggled,
                SynchronizacjaRezerwacji = (FindByName("SwSynchronizacjaRezerwacji") as Switch)!.IsToggled,
                UtworzenieNowejRezerwacji = (FindByName("SwUtworzenieNowejRezerwacji") as Switch)!.IsToggled
            };

            if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
            {
                await DisplayAlert("B³¹d", "Podaj imiê i e-mail.", "OK");
                return;
            }

            // Przygotuj notification_settings CSV
            string BuildNotifCsv(User u)
            {
                var list = new List<string>();
                if (u.NowaPlatnosc) list.Add("new_online_payment");
                if (u.WyslijPowiadomienieKlient) list.Add("reservation_notification");
                if (u.AnulowanieRezerwacji) list.Add("cancel_reservation");
                if (u.NowaRezerwacjaOnline) list.Add("new_online_booking");
                if (u.SynchronizacjaRezerwacji) list.Add("new_ical_sync");
                if (u.UtworzenieNowejRezerwacji) list.Add("create_reservation");
                return string.Join(",", list);
            }

            // Edycja istniej¹cego -> PUT do API, potem zast¹p w kolekcji
            if (_editingUser != null && _editingUser.Id > 0)
            {
                try
                {
                    var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                    var token = await auth.GetTokenAsync();
                    if (string.IsNullOrEmpty(token))
                    {
                        await DisplayAlert("B³¹d", "Brak tokena autoryzacji.", "OK");
                        return;
                    }

                    using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var payload = new
                    {
                        name = user.Name,
                        email = user.Email,
                        phone = user.Phone,
                        role = user.Role,
                        notification_settings = BuildNotifCsv(user)
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var resp = await http.PutAsync($"/entity/user/{_editingUser.Id}", content);

                    if (!resp.IsSuccessStatusCode && resp.StatusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        await DisplayAlert("B³¹d", $"Nie uda³o siê zapisaæ zmian ({(int)resp.StatusCode}).", "OK");
                        return;
                    }

                    // Zastêpujemy wpis w UserStore (aby CollectionView odœwie¿y³ siê)
                    var idx = UserStore.Users.IndexOf(_editingUser);
                    user.Id = _editingUser.Id;
                    if (idx >= 0)
                        UserStore.Users[idx] = user;
                    else
                        UserStore.Add(user);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Users] Save error: {ex.Message}");
                    await DisplayAlert("B³¹d", "Wyst¹pi³ b³¹d podczas zapisu.", "OK");
                    return;
                }
            }
            else
            {
                // Tworzenie nowego lokalnie (mo¿esz dodaæ POST do API analogicznie)
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