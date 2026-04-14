using System.Collections.ObjectModel;
using System.Text.Json;
using yBook.Models;

namespace yBook.Views.Uzytkownicy
{
    public partial class UzytkownicyIRolePage : ContentPage
    {
        private const string ApiUrl = "https://api.ybook.pl/entity/role";
        public ObservableCollection<Role> Roles { get; set; } = new();

        public UzytkownicyIRolePage()
        {
            InitializeComponent();
            // To jest kluczowe, żeby CollectionView widziało listę
            RolesCollection.ItemsSource = Roles;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await FetchRolesFromApiAsync();
        }

        private async Task FetchRolesFromApiAsync()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(ApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var rolesData = JsonDocument.Parse(json);

                    if (rolesData.RootElement.TryGetProperty("items", out var itemsArray))
                    {
                        Roles.Clear();
                        foreach (var elem in itemsArray.EnumerateArray())
                        {
                            var role = new Role
                            {
                                Id = elem.GetProperty("id").GetInt32(),
                                Name = elem.GetProperty("name").GetString() ?? "",
                                OrganizationId = elem.GetProperty("organization_id").GetInt32()
                            };

                            // Próba pobrania uprawnień
                            if (elem.TryGetProperty("permissions", out var permProp))
                            {
                                role.Permissions = RolePermissions.ConvertStringToPermissions(permProp.GetString() ?? "");
                            }
                            else
                            {
                                role.Permissions = new RolePermissions();
                            }

                            Roles.Add(role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd API: {ex.Message}");
            }
        }

        private async void OnDeleteRoleClicked(object sender, EventArgs e)
        {
            var role = (sender as Button)?.BindingContext as Role;
            if (role == null) return;

            bool confirm = await DisplayAlert("Usuń", $"Czy chcesz usunąć rolę {role.Name}?", "Tak", "Nie");
            if (confirm)
            {
                try
                {
                    using var client = new HttpClient();
                    var res = await client.DeleteAsync($"{ApiUrl}/{role.Id}");
                    if (res.IsSuccessStatusCode)
                    {
                        Roles.Remove(role);
                    }
                }
                catch (Exception ex) { await DisplayAlert("Błąd", ex.Message, "OK"); }
            }
        }

        private async void OnEditRoleClicked(object sender, EventArgs e)
        {
            var role = (sender as Button)?.BindingContext as Role;
            if (role != null)
            {
                await Navigation.PushModalAsync(new RoleEditorPage(role));
            }
        }

        private async void OnAddRoleClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new RoleEditorPage());
        }

        private void OnUsersTabClicked(object sender, EventArgs e) { /* Widok użytkowników */ }
        private void OnRolesTabClicked(object sender, EventArgs e) { /* Widok ról */ }
    }
}