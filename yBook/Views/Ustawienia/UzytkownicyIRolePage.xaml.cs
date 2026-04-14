using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Maui.Controls;
using yBook.Models;
using yBook.Services;
using yBook.Views.Uzytkownicy;

namespace yBook.Views.Ustawienia
{
    public partial class UzytkownicyIRolePage : ContentPage
    {
        private ObservableCollection<Role> _roles = new();
        private bool _isUsersTabActive = true;

        public UzytkownicyIRolePage()
        {
            InitializeComponent();
            InitializeRoles();
            UsersList.ItemsSource = UserStore.Users;
            UpdateEmptyLabel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UsersList.ItemsSource = UserStore.Users;
            UpdateEmptyLabel();
            UserStore.Users.CollectionChanged += Users_CollectionChanged;
            _ = FetchUsersFromApiAsync();
            _ = FetchRolesFromApiAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UserStore.Users.CollectionChanged -= Users_CollectionChanged;
        }

        private void InitializeRoles()
        {
            _roles = new ObservableCollection<Role>();
            RolesCollection.ItemsSource = _roles;
            UpdateEmptyRolesLabel();
        }

        private void Users_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateEmptyLabel();
        }

        private void UpdateEmptyLabel()
        {
            EmptyUsersLabel.IsVisible = UserStore.Users.Count == 0;
        }

        private void UpdateEmptyRolesLabel()
        {
            if (EmptyRolesLabel != null)
            {
                EmptyRolesLabel.IsVisible = _roles.Count == 0;
            }
        }

        // ── TABS ───────────────────────────────────────────────────────────

        void OnUsersTabClicked(object sender, EventArgs e)
        {
            if (_isUsersTabActive) return;
            _isUsersTabActive = true;
            UsersView.IsVisible = true;
            RolesView.IsVisible = false;
            BtnUsersTab.BackgroundColor = Color.FromArgb("#1565C0");
            BtnUsersTab.TextColor = Colors.White;
            BtnRolesTab.BackgroundColor = Color.FromArgb("#E0E0E0");
            BtnRolesTab.TextColor = Color.FromArgb("#333");
        }

        void OnRolesTabClicked(object sender, EventArgs e)
        {
            if (!_isUsersTabActive) return;
            _isUsersTabActive = false;
            UsersView.IsVisible = false;
            RolesView.IsVisible = true;
            BtnRolesTab.BackgroundColor = Color.FromArgb("#1565C0");
            BtnRolesTab.TextColor = Colors.White;
            BtnUsersTab.BackgroundColor = Color.FromArgb("#E0E0E0");
            BtnUsersTab.TextColor = Color.FromArgb("#333");
        }

        // ── USERS ──────────────────────────────────────────────────────────

        async void OnInviteClicked(object sender, EventArgs e)
        {
            var page = new Views.Uzytkownicy.UzytkownicyPage();
            await Navigation.PushModalAsync(page);
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is User user)
            {
                var page = new Views.Uzytkownicy.UzytkownicyPage(user);
                await Navigation.PushModalAsync(page);
            }
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not User user)
                return;

            bool ok = await DisplayAlert("Usuń użytkownika", $"Czy na pewno chcesz usunąć {user.Name}?", "Tak", "Anuluj");
            if (!ok) return;

            if (user.Id > 0)
            {
                try
                {
                    var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                    var token = await auth.GetTokenAsync();
                    if (string.IsNullOrEmpty(token))
                    {
                        await DisplayAlert("Błąd", "Brak tokena autoryzacji.", "OK");
                        return;
                    }

                    using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var resp = await http.DeleteAsync($"/entity/user/{user.Id}");
                    if (!resp.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Błąd", $"Nie udało się usunąć użytkownika (kod {(int)resp.StatusCode}).", "OK");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Users] Delete error: {ex.Message}");
                    await DisplayAlert("Błąd", $"Błąd połączenia: {ex.Message}", "OK");
                    return;
                }
            }

            UserStore.Users.Remove(user);
            UpdateEmptyLabel();
        }

        // ── ROLES ──────────────────────────────────────────────────────────

        async void OnAddRoleClicked(object sender, EventArgs e)
        {
            var page = new Views.Uzytkownicy.RoleEditorPage();
            await Navigation.PushModalAsync(page);

            // Modal has closed - check if user saved data
            if (page.SavedRole != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AddRole] SavedRole: Name={page.SavedRole.Name}, PermissionsDisplay={page.SavedRole.PermissionsDisplay}");

                bool success = await SaveRoleToApiAsync(page.SavedRole);
                if (success)
                {
                    if (page.SavedRole.Id > 0)
                    {
                        // New role was created with ID from API - refresh from API to ensure consistency
                        await FetchRolesFromApiAsync();
                        System.Diagnostics.Debug.WriteLine($"[AddRole] New role saved with ID {page.SavedRole.Id}, refreshed from API");
                    }
                    else
                    {
                        // Fallback: add locally if API didn't return ID
                        _roles.Add(page.SavedRole);
                        System.Diagnostics.Debug.WriteLine($"[AddRole] Added new role to collection (no ID returned)");
                    }
                    UpdateEmptyRolesLabel();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AddRole] Failed to save role to API");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AddRole] SavedRole is null - user cancelled");
            }
        }

        async void OnEditRoleClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not Role role)
                return;

            var page = new Views.Uzytkownicy.RoleEditorPage(role);
            await Navigation.PushModalAsync(page);

            // Modal has closed - check if user saved changes
            if (page.SavedRole != null)
            {
                bool success = await SaveRoleToApiAsync(page.SavedRole);
                if (success)
                {
                    // Refresh from API to get updated data
                    await FetchRolesFromApiAsync();
                    System.Diagnostics.Debug.WriteLine($"[EditRole] Role updated and refreshed from API");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[EditRole] Failed to save role to API");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[EditRole] SavedRole is null - user cancelled");
            }
        }

        async void OnDeleteRoleClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not Role role)
                return;

            bool ok = await DisplayAlert("Usuń rolę", $"Czy na pewno chcesz usunąć rolę '{role.Name}'?", "Tak", "Anuluj");
            if (!ok) return;

            if (role.Id > 0)
            {
                try
                {
                    var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                    var token = await auth.GetTokenAsync();
                    if (string.IsNullOrEmpty(token))
                    {
                        await DisplayAlert("Błąd", "Brak tokena autoryzacji.", "OK");
                        return;
                    }

                    using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var resp = await http.DeleteAsync($"/entity/role/{role.Id}");
                    if (!resp.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Błąd", $"Nie udało się usunąć rolę (kod {(int)resp.StatusCode}).", "OK");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Roles] Delete error: {ex.Message}");
                    await DisplayAlert("Błąd", $"Błąd połączenia: {ex.Message}", "OK");
                    return;
                }
            }

            _roles.Remove(role);
            UpdateEmptyRolesLabel();
        }

        // ── API ────────────────────────────────────────────────────────────

        private async Task FetchRolesFromApiAsync()
        {
            try
            {
                var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                var token = await auth.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return;

                using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await http.GetAsync("/entity/role");
                if (!resp.IsSuccessStatusCode) return;

                var json = await resp.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _roles.Clear();

                    JsonElement itemsArray;
                    if (doc.RootElement.TryGetProperty("items", out var itemsProp))
                    {
                        itemsArray = itemsProp;
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        itemsArray = doc.RootElement;
                    }
                    else
                    {
                        return;
                    }

                    foreach (var elem in itemsArray.EnumerateArray())
                    {
                        try
                        {
                            var role = new Role
                            {
                                Id = elem.GetProperty("id").GetInt32(),
                                OrganizationId = elem.TryGetProperty("organization_id", out var orgIdProp) 
                                    ? orgIdProp.GetInt32() 
                                    : 0,
                                Name = elem.GetProperty("name").GetString() ?? ""
                            };

                            // Note: API endpoint doesn't return "permissions" field in list response
                            // Permissions remain empty (default RolePermissions object)
                            if (elem.TryGetProperty("permissions", out var permProp))
                            {
                                var permissionsString = permProp.GetString() ?? "";
                                role.Permissions = RolePermissions.ConvertStringToPermissions(permissionsString);
                            }

                            if (elem.TryGetProperty("date_modified", out var dateProp))
                            {
                                if (DateTime.TryParse(dateProp.GetString(), out var modDate))
                                {
                                    role.DateModified = modDate;
                                }
                            }

                            _roles.Add(role);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FetchRoles] Error parsing role: {ex.Message}");
                        }
                    }

                    UpdateEmptyRolesLabel();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FetchRoles] Error: {ex.Message}");
            }
        }

        private async Task<bool> SaveRoleToApiAsync(Role role)
        {
            try
            {
                var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                var token = await auth.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Błąd", "Brak tokena autoryzacji.", "OK");
                    return false;
                }

                using var http = new HttpClient { BaseAddress = new Uri("https://api.ybook.pl") };
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var permissionsString = ConvertPermissionsToApiFormat(role.Permissions);
                System.Diagnostics.Debug.WriteLine($"[SaveRole] Permissions string: {permissionsString}");

                var payload = new
                {
                    name = role.Name,
                    organization_id = role.OrganizationId,
                    permissions = permissionsString
                };

                var json = JsonSerializer.Serialize(payload);
                System.Diagnostics.Debug.WriteLine($"[SaveRole] Payload: {json}");

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage resp;
                if (role.Id > 0)
                {
                    resp = await http.PutAsync($"/entity/role/{role.Id}", content);
                    System.Diagnostics.Debug.WriteLine($"[SaveRole] PUT request to /entity/role/{role.Id}");
                }
                else
                {
                    resp = await http.PostAsync("/entity/role", content);
                    System.Diagnostics.Debug.WriteLine($"[SaveRole] POST request to /entity/role");

                    if (resp.IsSuccessStatusCode)
                    {
                        var respJson = await resp.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"[SaveRole] Response: {respJson}");

                        var doc = JsonDocument.Parse(respJson);

                        if (doc.RootElement.TryGetProperty("id", out var idProp))
                        {
                            role.Id = idProp.GetInt32();
                            System.Diagnostics.Debug.WriteLine($"[SaveRole] Role ID set to: {role.Id}");
                        }

                        if (doc.RootElement.TryGetProperty("date_modified", out var dateProp))
                        {
                            if (DateTime.TryParse(dateProp.GetString(), out var modDate))
                            {
                                role.DateModified = modDate;
                            }
                        }
                    }
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[SaveRole] API Error ({(int)resp.StatusCode}): {errorContent}");
                    await DisplayAlert("Błąd", $"Nie udało się zapisać rolę (kod {(int)resp.StatusCode}).", "OK");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[SaveRole] Success - Role saved");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveRole] Exception: {ex.Message}\n{ex.StackTrace}");
                await DisplayAlert("Błąd", $"Błąd połączenia: {ex.Message}", "OK");
                return false;
            }
        }

        /// <summary>
        /// Konwertuje RolePermissions do formatu oczekiwanego przez API (string oddzielony przecinkami)
        /// </summary>
        private string ConvertPermissionsToApiFormat(RolePermissions permissions)
        {
            return RolePermissions.ConvertPermissionsToString(permissions);
        }

        private async Task FetchUsersFromApiAsync()
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
                var doc = JsonDocument.Parse(json);

                UserStore.Users.Clear();
                foreach (var elem in doc.RootElement.EnumerateArray())
                {
                    var user = new User
                    {
                        Id = elem.GetProperty("id").GetInt32(),
                        Name = elem.GetProperty("name").GetString() ?? "",
                        Email = elem.GetProperty("email").GetString() ?? "",
                        Phone = elem.TryGetProperty("phone", out var phoneProp) ? phoneProp.GetString() : "",
                        Role = elem.TryGetProperty("role", out var roleProp) ? roleProp.GetString() : ""
                    };
                    UserStore.Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FetchUsers] Error: {ex.Message}");
            }
        }
    }
}