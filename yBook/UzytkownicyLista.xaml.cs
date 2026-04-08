using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using yBook.Services;
using yBook.Models;
using yBook.Views.Uzytkownicy;


namespace yBook.Views.Uzytkownicy
{
    public partial class Uzytkownicy1Page : ContentPage
    {
        public Uzytkownicy1Page()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UsersList.ItemsSource = UserStore.Users;
            UpdateEmpty();
            UserStore.Users.CollectionChanged += Users_CollectionChanged;

            // Asynchroniczne pobranie u¿ytkowników z serwera (nie blokuje UI)
            _ = FetchUsersFromApiAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UserStore.Users.CollectionChanged -= Users_CollectionChanged;
        }

        void Users_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateEmpty();
        }

        void UpdateEmpty()
        {
            EmptyLabel.IsVisible = UserStore.Users.Count == 0;
        }

        // Bezpoœrednie otwarcie formularza jako modal — niezawodne
        async void OnInviteClicked(object sender, EventArgs e)
        {
            var page = new UzytkownicyPage();
            await Navigation.PushModalAsync(page);
        }

        // Edit existing user
        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is User user)
            {
                var page = new UzytkownicyPage(user);
                await Navigation.PushModalAsync(page);
            }
        }

        // Delete user from store with confirmation and on-server DELETE
        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not User user)
                return;

            bool ok = await DisplayAlert("Usuñ u¿ytkownika", $"Czy na pewno chcesz usun¹æ {user.Name}?", "Tak", "Anuluj");
            if (!ok) return;

            // Jeœli u¿ytkownik ma przypisane Id (istnieje na serwerze) — wyœlij DELETE
            if (user.Id > 0)
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

                    var resp = await http.DeleteAsync($"/entity/user/{user.Id}");
                    if (!resp.IsSuccessStatusCode)
                    {
                        await DisplayAlert("B³¹d", $"Nie uda³o siê usun¹æ u¿ytkownika (kod {(int)resp.StatusCode}).", "OK");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Users] Delete error: {ex.Message}");
                    await DisplayAlert("B³¹d", "Wyst¹pi³ problem podczas usuwania u¿ytkownika (po³¹czenie).", "OK");
                    return;
                }
            }

            // Usuñ lokalnie (tak¿e dla u¿ytkowników bez Id)
            UserStore.Users.Remove(user);
        }

        // Pobiera listê u¿ytkowników z API i dopisuje nowe rekordy do UserStore (bez kasowania istniej¹cych)
        async System.Threading.Tasks.Task FetchUsersFromApiAsync()
        {
            try
            {
                var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                var token = await auth.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return;

                using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resp = await http.GetAsync("/entity/user");
                if (!resp.IsSuccessStatusCode) return;

                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("items", out var items)) return;

                // Zbiór istniej¹cych e-maili (lowercase) — pozwala pomin¹æ duplikaty
                var existingEmails = new HashSet<string>(
                    UserStore.Users
                             .Select(u => (u.Email ?? string.Empty).Trim().ToLowerInvariant())
                             .Where(s => !string.IsNullOrEmpty(s))
                );

                foreach (var it in items.EnumerateArray())
                {
                    var id = it.TryGetProperty("id", out var pId) ? pId.GetInt32() : 0;
                    var name = it.TryGetProperty("name", out var pName) ? pName.GetString() : string.Empty;
                    var email = it.TryGetProperty("email", out var pEmail) ? pEmail.GetString() : string.Empty;
                    var role = it.TryGetProperty("role", out var pRole) ? pRole.GetString() : string.Empty;
                    var phone = it.TryGetProperty("phone", out var pPhone) ? pPhone.GetString() : string.Empty;

                    var notif = it.TryGetProperty("notification_settings", out var pNotif) ? pNotif.GetString() ?? string.Empty : string.Empty;
                    var tokens = notif.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => s.Trim().ToLowerInvariant())
                                      .ToArray();

                    var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(normalizedEmail) || existingEmails.Contains(normalizedEmail))
                    {
                        // Pomin¹æ: brak e-maila lub ju¿ istnieje
                        continue;
                    }

                    var u = new User
                    {
                        Id = id,
                        Name = name ?? string.Empty,
                        Email = email ?? string.Empty,
                        Role = role ?? string.Empty,
                        Phone = phone ?? string.Empty,
                        NowaPlatnosc = tokens.Contains("new_online_payment"),
                        WyslijPowiadomienieKlient = tokens.Contains("reservation_notification"),
                        AnulowanieRezerwacji = tokens.Contains("cancel_reservation"),
                        NowaRezerwacjaOnline = tokens.Contains("new_online_booking"),
                        SynchronizacjaRezerwacji = tokens.Contains("reservation_sync") || tokens.Contains("new_ical_sync") || tokens.Contains("sync"),
                        UtworzenieNowejRezerwacji = tokens.Contains("create_reservation")
                    };

                    UserStore.Add(u);
                    existingEmails.Add(normalizedEmail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Users] FetchUsersFromApiAsync error: {ex.Message}");
            }
        }
    }
}