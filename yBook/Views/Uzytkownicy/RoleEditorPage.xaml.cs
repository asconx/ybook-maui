using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;
using yBook.Models;

namespace yBook.Views.Uzytkownicy
{
    public partial class RoleEditorPage : ContentPage
    {
        private readonly Role? _editingRole;
        private RolePermissions _currentPermissions = new();
        private Dictionary<string, List<(string PropertyName, string DisplayName)>> _categoryPermissions = new();
        private string _selectedCategory = "";
        private Dictionary<string, RadioButton> _categoryRadios = new();
        private Dictionary<string, CheckBox> _permissionCheckboxes = new();

        /// <summary>
        /// Saved role result - set to non-null when user clicks Save
        /// </summary>
        public Role? SavedRole { get; private set; }

        public RoleEditorPage(Role? editingRole = null)
        {
            InitializeComponent();
            _editingRole = editingRole;

            if (_editingRole != null)
            {
                HeaderLabel.Text = "Edytuj rolę";
                RoleNameEntry.Text = _editingRole.Name;
                _currentPermissions = CopyPermissions(_editingRole.Permissions);
            }

            InitializeCategoryPermissionsMap();
            BuildCategoryRadioButtons();
        }

        private void InitializeCategoryPermissionsMap()
        {
            _categoryPermissions = new Dictionary<string, List<(string, string)>>
            {
                { "Rezerwacje", new()
                {
                    ("RezerRetComAddReservation", "Dodawanie rezerwacji"),
                    ("RezerRetComEditReservation", "Edycja rezerwacji"),
                    ("RezerRetComArchiveReservation", "Archiwizacja rezerwacji"),
                    ("RezerRetComReturnReservation", "Przywracanie zarchiwizowanej rezerwacji"),
                    ("RezerRetComNotifications", "Wysyłanie powiadomień"),
                    ("RezerRetComSmsSendReservation", "Wysyłanie SMS do aktywnych rezerwacji"),
                }},
                { "Terminarz", new()
                {
                    ("CalendarAddSchedule", "Dodawanie terminarza"),
                    ("CalendarEditSchedule", "Edycja terminarza"),
                    ("CalendarDeleteSchedule", "Usuwanie terminarza"),
                    ("CalendarAddActivity", "Dodawanie aktywności"),
                    ("CalendarEditActivity", "Edycja aktywności"),
                    ("CalendarDeleteActivity", "Usuwanie aktywności"),
                }},
                { "Miejsca i kwatery", new()
                {
                    ("PropertiesEditPlace", "Edycja miejsca"),
                    ("PropertiesAddRoom", "Dodawanie kwater"),
                    ("PropertiesEditRoom", "Edycja kwater"),
                    ("PropertiesDeleteRoom", "Usuwanie kwater"),
                }},
                { "iCalendar", new()
                {
                    ("IcalAddSync", "Dodawanie synchronizacji iCalendar"),
                    ("IcalEditSync", "Edycja synchronizacji iCalendar"),
                    ("IcalDeleteSync", "Usuwanie synchronizacji iCalendar"),
                }},
                { "Blokady", new()
                {
                    ("LockAddBlock", "Dodawanie blokady"),
                    ("LockEditBlock", "Edycja blokady"),
                    ("LockDeleteBlock", "Usuwanie blokady"),
                }},
                { "Użytkownicy i role", new()
                {
                    ("UserInviteUser", "Zapraszanie użytkowników"),
                    ("UserEditUser", "Edycja użytkownika"),
                    ("UserDeleteUser", "Usuwanie użytkownika"),
                    ("UserAddRole", "Dodawanie roli"),
                    ("UserEditRole", "Edycja roli"),
                    ("UserDeleteRole", "Usuwanie roli"),
                }},
                { "Klienci", new()
                {
                    ("GuestAddGuest", "Dodawanie klienta"),
                    ("GuestEditGuest", "Edycja klienta"),
                    ("GuestDeleteGuest", "Usuwanie klienta"),
                }},
                { "Statusy i powiadomienia", new()
                {
                    ("StatusAddStatus", "Dodawanie statusu"),
                    ("StatusEditStatus", "Edycja statusu"),
                    ("StatusDeleteStatus", "Usuwanie statusu"),
                    ("StatusAddNotification", "Dodawanie powiadomienia"),
                    ("StatusEditNotification", "Edycja powiadomienia"),
                    ("StatusDeleteNotification", "Usuwanie powiadomienia"),
                }},
                { "Cenniki", new()
                {
                    ("PricingAddPricing", "Dodawanie cennika"),
                    ("PricingEditPricing", "Edycja cennika"),
                    ("PricingDeletePricing", "Usuwanie cennika"),
                }},
                { "Usługi i opłaty", new()
                {
                    ("ServiceAddServiceToReservation", "Dodawanie usług i opłat do rezerwacji"),
                    ("ServiceCreateService", "Tworzenie usług i opłat"),
                    ("ServiceEditService", "Edycja usług i opłat"),
                    ("ServiceDeleteService", "Usuwanie usług i opłat"),
                }},
                { "Pakiety", new()
                {
                    ("PackageAddPackage", "Dodawanie pakietu"),
                    ("PackageEditPackage", "Edycja pakietu"),
                    ("PackageDeletePackage", "Usuwanie pakietu"),
                }},
                { "Zniżki i rabat", new()
                {
                    ("DiscountAddDiscountToGuest", "Dodawanie rabatu do klienta"),
                    ("DiscountAddDiscountToReservation", "Dodawanie rabatu do rezerwacji"),
                    ("DiscountAddDeductionToReservation", "Dodawanie zniżek do rezerwacji"),
                    ("DiscountCreateDiscount", "Tworzenie rabatów"),
                    ("DiscountEditDiscount", "Edycja rabatów"),
                    ("DiscountDeleteDiscount", "Usuwanie rabatów"),
                    ("DiscountCreateDeduction", "Tworzenie zniżek"),
                    ("DiscountEditDeduction", "Edycja zniżek"),
                    ("DiscountDeleteDeduction", "Usuwanie zniżek"),
                }},
                { "Kasa", new()
                {
                    ("CashStartEndShift", "Rozpoczynanie i kończenie zmiany"),
                    ("CashAcceptPayment", "Przyjmowanie wpłat"),
                    ("CashEditDocumentItem", "Edycja pozycji na dokumentach"),
                    ("CashEditPayment", "Edycja wpłat"),
                    ("CashDeletePayment", "Usuwanie wpłat"),
                    ("CashSettleAccount", "Rozliczanie konta kasjera"),
                }},
                { "Księgowość", new()
                {
                    ("AccountingAddAccount", "Dodawanie konta"),
                    ("AccountingEditAccount", "Edycja konta"),
                    ("AccountingDeleteAccount", "Usuwanie konta"),
                    ("AccountingCreateDocument", "Tworzenie dokumentu"),
                    ("AccountingEditDocument", "Edycja dokumentu"),
                    ("AccountingDeleteDocument", "Usuwanie dokumentu"),
                }},
                { "Analityka", new()
                {
                    ("AnalyticsViewAnalytics", "Wgląd do analiz"),
                    ("AnalyticsAddSurvey", "Dodawanie ankiet"),
                    ("AnalyticsEditSurvey", "Edycja ankiet"),
                    ("AnalyticsDeleteSurvey", "Usuwanie ankiet"),
                }},
                { "Smart Locks", new()
                {
                    ("SmartLockAddCode", "Dodawanie kodu"),
                    ("SmartLockUpdateCode", "Aktualizacja kodu"),
                    ("SmartLockDeleteCode", "Usunięcie kodu"),
                }},
                { "Właściciele zarządzani", new()
                {
                    ("ManagedOwnerManageOwners", "Zarządzanie właścicielami"),
                    ("ManagedOwnerAddCost", "Dodawanie kosztów"),
                    ("ManagedOwnerEditCost", "Edycja kosztów"),
                    ("ManagedOwnerDeleteCost", "Usuwanie kosztów"),
                }},
            };
        }

        private void BuildCategoryRadioButtons()
        {
            CategoriesContainer.Clear();
            _categoryRadios.Clear();

            int index = 0;
            foreach (var category in _categoryPermissions.Keys)
            {
                var radio = new RadioButton
                {
                    Content = category,
                    FontSize = 13,
                    Padding = new Thickness(8, 4)
                };

                if (index == 0)
                {
                    radio.IsChecked = true;
                    _selectedCategory = category;
                }

                _categoryRadios[category] = radio;
                radio.CheckedChanged += (s, e) => OnCategoryRadioCheckedChanged(category, e);
                CategoriesContainer.Add(radio);
                index++;
            }

            // Display permissions for first category
            if (_categoryPermissions.Count > 0)
            {
                DisplayPermissionsForCategory(_categoryPermissions.Keys.First());
            }
        }

        private void OnCategoryRadioCheckedChanged(string categoryName, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                _selectedCategory = categoryName;
                DisplayPermissionsForCategory(categoryName);
            }
        }

        private void DisplayPermissionsForCategory(string categoryName)
        {
            PermissionsContainer.Clear();
            _permissionCheckboxes.Clear();

            if (!_categoryPermissions.TryGetValue(categoryName, out var permissions))
                return;

            foreach (var (propName, displayName) in permissions)
            {
                var row = new HorizontalStackLayout
                {
                    Spacing = 8,
                    Padding = new Thickness(0, 4)
                };

                var checkbox = new CheckBox
                {
                    VerticalOptions = LayoutOptions.Center
                };

                // Load state from persistent _currentPermissions
                var prop = typeof(RolePermissions).GetProperty(propName);
                if (prop != null)
                {
                    bool value = (bool?)prop.GetValue(_currentPermissions) ?? false;
                    checkbox.IsChecked = value;
                }

                var label = new Label
                {
                    Text = displayName,
                    FontSize = 13,
                    VerticalOptions = LayoutOptions.Center
                };

                // Update persistent state when checkbox changes
                checkbox.CheckedChanged += (s, e) =>
                {
                    prop?.SetValue(_currentPermissions, checkbox.IsChecked);
                };

                row.Add(checkbox);
                row.Add(label);

                _permissionCheckboxes[propName] = checkbox;
                PermissionsContainer.Add(row);
            }
        }

        private RolePermissions CopyPermissions(RolePermissions? source)
        {
            var copy = new RolePermissions();
            if (source == null) return copy;

            // Deep copy all boolean properties
            foreach (var prop in typeof(RolePermissions).GetProperties())
            {
                if (prop.PropertyType == typeof(bool) && prop.CanWrite && prop.CanRead)
                {
                    bool value = (bool?)prop.GetValue(source) ?? false;
                    prop.SetValue(copy, value);
                }
            }

            return copy;
        }

        private RolePermissions GetPermissionsFromUI()
        {
            // Return persistent state object that has been updated in real-time
            return _currentPermissions;
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            string roleName = RoleNameEntry.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(roleName))
            {
                await DisplayAlert("Błąd", "Podaj nazwę roli.", "OK");
                return;
            }

            var role = new Role
            {
                Id = _editingRole?.Id ?? 0,
                OrganizationId = _editingRole?.OrganizationId ?? 1,
                Name = roleName,
                Permissions = GetPermissionsFromUI(),
                DateModified = DateTime.UtcNow
            };

            SavedRole = role;

            if (Navigation.ModalStack.Count > 0 && Navigation.ModalStack[^1] == this)
                await Navigation.PopModalAsync();
            else
                await Shell.Current.GoToAsync("..");
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Count > 0 && Navigation.ModalStack[^1] == this)
                await Navigation.PopModalAsync();
            else
                await Shell.Current.GoToAsync("..");
        }
    }
}
