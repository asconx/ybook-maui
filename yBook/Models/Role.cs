namespace yBook.Models
{
    public class Role
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public DateTime DateModified { get; set; }
        public string Name { get; set; } = "";
        public RolePermissions Permissions { get; set; } = new();

        public string PermissionsDisplay => RolePermissions.ConvertPermissionsToString(Permissions);

        public override string ToString() => Name;
    }

    public class RolePermissions
    {
        // Rezerwacje (Recepcja)
        public bool RezerRetComAddReservation { get; set; }
        public bool RezerRetComEditReservation { get; set; }
        public bool RezerRetComArchiveReservation { get; set; }
        public bool RezerRetComReturnReservation { get; set; }
        public bool RezerRetComNotifications { get; set; }
        public bool RezerRetComSmsSendReservation { get; set; }

        // Terminarz
        public bool CalendarAddSchedule { get; set; }
        public bool CalendarEditSchedule { get; set; }
        public bool CalendarDeleteSchedule { get; set; }
        public bool CalendarAddActivity { get; set; }
        public bool CalendarEditActivity { get; set; }
        public bool CalendarDeleteActivity { get; set; }

        // Miejsca i kwatery
        public bool PropertiesEditPlace { get; set; }
        public bool PropertiesAddRoom { get; set; }
        public bool PropertiesEditRoom { get; set; }
        public bool PropertiesDeleteRoom { get; set; }

        // iCalendar
        public bool IcalAddSync { get; set; }
        public bool IcalEditSync { get; set; }
        public bool IcalDeleteSync { get; set; }

        // Blokady
        public bool LockAddBlock { get; set; }
        public bool LockEditBlock { get; set; }
        public bool LockDeleteBlock { get; set; }

        // Użytkownicy i role
        public bool UserInviteUser { get; set; }
        public bool UserEditUser { get; set; }
        public bool UserDeleteUser { get; set; }
        public bool UserAddRole { get; set; }
        public bool UserEditRole { get; set; }
        public bool UserDeleteRole { get; set; }

        // Klienci
        public bool GuestAddGuest { get; set; }
        public bool GuestEditGuest { get; set; }
        public bool GuestDeleteGuest { get; set; }

        // Statusy i powiadomienia
        public bool StatusAddStatus { get; set; }
        public bool StatusEditStatus { get; set; }
        public bool StatusDeleteStatus { get; set; }
        public bool StatusAddNotification { get; set; }
        public bool StatusEditNotification { get; set; }
        public bool StatusDeleteNotification { get; set; }

        // Cenniki
        public bool PricingAddPricing { get; set; }
        public bool PricingEditPricing { get; set; }
        public bool PricingDeletePricing { get; set; }

        // Usługi i opłaty
        public bool ServiceAddServiceToReservation { get; set; }
        public bool ServiceCreateService { get; set; }
        public bool ServiceEditService { get; set; }
        public bool ServiceDeleteService { get; set; }

        // Pakiety
        public bool PackageAddPackage { get; set; }
        public bool PackageEditPackage { get; set; }
        public bool PackageDeletePackage { get; set; }

        // Zniżki i rabat
        public bool DiscountAddDiscountToGuest { get; set; }
        public bool DiscountAddDiscountToReservation { get; set; }
        public bool DiscountAddDeductionToReservation { get; set; }
        public bool DiscountCreateDiscount { get; set; }
        public bool DiscountEditDiscount { get; set; }
        public bool DiscountDeleteDiscount { get; set; }
        public bool DiscountCreateDeduction { get; set; }
        public bool DiscountEditDeduction { get; set; }
        public bool DiscountDeleteDeduction { get; set; }

        // Kasa
        public bool CashStartEndShift { get; set; }
        public bool CashAcceptPayment { get; set; }
        public bool CashEditDocumentItem { get; set; }
        public bool CashEditPayment { get; set; }
        public bool CashDeletePayment { get; set; }
        public bool CashSettleAccount { get; set; }

        // Księgowość
        public bool AccountingAddAccount { get; set; }
        public bool AccountingEditAccount { get; set; }
        public bool AccountingDeleteAccount { get; set; }
        public bool AccountingCreateDocument { get; set; }
        public bool AccountingEditDocument { get; set; }
        public bool AccountingDeleteDocument { get; set; }

        // Analityka
        public bool AnalyticsViewAnalytics { get; set; }
        public bool AnalyticsAddSurvey { get; set; }
        public bool AnalyticsEditSurvey { get; set; }
        public bool AnalyticsDeleteSurvey { get; set; }

        // Smart Locks
        public bool SmartLockAddCode { get; set; }
        public bool SmartLockUpdateCode { get; set; }
        public bool SmartLockDeleteCode { get; set; }

        // Właściciele zarządzani
        public bool ManagedOwnerManageOwners { get; set; }
        public bool ManagedOwnerAddCost { get; set; }
        public bool ManagedOwnerEditCost { get; set; }
        public bool ManagedOwnerDeleteCost { get; set; }

        /// <summary>
        /// Mapowanie property names na polskie nazwy wyświetlane dla uprawnień
        /// </summary>
        private static readonly Dictionary<string, string> PropertyNameToDisplayName = new()
        {
            { "RezerRetComAddReservation", "Dodawanie rezerwacji" },
            { "RezerRetComEditReservation", "Edycja rezerwacji" },
            { "RezerRetComArchiveReservation", "Archiwizacja rezerwacji" },
            { "RezerRetComReturnReservation", "Przywracanie zarchiwizowanej rezerwacji" },
            { "RezerRetComNotifications", "Wysyłanie powiadomień" },
            { "RezerRetComSmsSendReservation", "Wysyłanie SMS do aktywnych rezerwacji" },
            { "CalendarAddSchedule", "Dodawanie terminarza" },
            { "CalendarEditSchedule", "Edycja terminarza" },
            { "CalendarDeleteSchedule", "Usuwanie terminarza" },
            { "CalendarAddActivity", "Dodawanie aktywności" },
            { "CalendarEditActivity", "Edycja aktywności" },
            { "CalendarDeleteActivity", "Usuwanie aktywności" },
            { "PropertiesEditPlace", "Edycja miejsca" },
            { "PropertiesAddRoom", "Dodawanie kwater" },
            { "PropertiesEditRoom", "Edycja kwater" },
            { "PropertiesDeleteRoom", "Usuwanie kwater" },
            { "IcalAddSync", "Dodawanie synchronizacji iCalendar" },
            { "IcalEditSync", "Edycja synchronizacji iCalendar" },
            { "IcalDeleteSync", "Usuwanie synchronizacji iCalendar" },
            { "LockAddBlock", "Dodawanie blokady" },
            { "LockEditBlock", "Edycja blokady" },
            { "LockDeleteBlock", "Usuwanie blokady" },
            { "UserInviteUser", "Zapraszanie użytkowników" },
            { "UserEditUser", "Edycja użytkownika" },
            { "UserDeleteUser", "Usuwanie użytkownika" },
            { "UserAddRole", "Dodawanie roli" },
            { "UserEditRole", "Edycja roli" },
            { "UserDeleteRole", "Usuwanie roli" },
            { "GuestAddGuest", "Dodawanie klienta" },
            { "GuestEditGuest", "Edycja klienta" },
            { "GuestDeleteGuest", "Usuwanie klienta" },
            { "StatusAddStatus", "Dodawanie statusu" },
            { "StatusEditStatus", "Edycja statusu" },
            { "StatusDeleteStatus", "Usuwanie statusu" },
            { "StatusAddNotification", "Dodawanie powiadomienia" },
            { "StatusEditNotification", "Edycja powiadomienia" },
            { "StatusDeleteNotification", "Usuwanie powiadomienia" },
            { "PricingAddPricing", "Dodawanie cennika" },
            { "PricingEditPricing", "Edycja cennika" },
            { "PricingDeletePricing", "Usuwanie cennika" },
            { "ServiceAddServiceToReservation", "Dodawanie usług i opłat do rezerwacji" },
            { "ServiceCreateService", "Tworzenie usług i opłat" },
            { "ServiceEditService", "Edycja usług i opłat" },
            { "ServiceDeleteService", "Usuwanie usług i opłat" },
            { "PackageAddPackage", "Dodawanie pakietu" },
            { "PackageEditPackage", "Edycja pakietu" },
            { "PackageDeletePackage", "Usuwanie pakietu" },
            { "DiscountAddDiscountToGuest", "Dodawanie rabatu do klienta" },
            { "DiscountAddDiscountToReservation", "Dodawanie rabatu do rezerwacji" },
            { "DiscountAddDeductionToReservation", "Dodawanie zniżek do rezerwacji" },
            { "DiscountCreateDiscount", "Tworzenie rabatów" },
            { "DiscountEditDiscount", "Edycja rabatów" },
            { "DiscountDeleteDiscount", "Usuwanie rabatów" },
            { "DiscountCreateDeduction", "Tworzenie zniżek" },
            { "DiscountEditDeduction", "Edycja zniżek" },
            { "DiscountDeleteDeduction", "Usuwanie zniżek" },
            { "CashStartEndShift", "Rozpoczynanie i kończenie zmiany" },
            { "CashAcceptPayment", "Przyjmowanie wpłat" },
            { "CashEditDocumentItem", "Edycja pozycji na dokumentach" },
            { "CashEditPayment", "Edycja wpłat" },
            { "CashDeletePayment", "Usuwanie wpłat" },
            { "CashSettleAccount", "Rozliczanie konta kasjera" },
            { "AccountingAddAccount", "Dodawanie konta" },
            { "AccountingEditAccount", "Edycja konta" },
            { "AccountingDeleteAccount", "Usuwanie konta" },
            { "AccountingCreateDocument", "Tworzenie dokumentu" },
            { "AccountingEditDocument", "Edycja dokumentu" },
            { "AccountingDeleteDocument", "Usuwanie dokumentu" },
            { "AnalyticsViewAnalytics", "Wgląd do analiz" },
            { "AnalyticsAddSurvey", "Dodawanie ankiet" },
            { "AnalyticsEditSurvey", "Edycja ankiet" },
            { "AnalyticsDeleteSurvey", "Usuwanie ankiet" },
            { "SmartLockAddCode", "Dodawanie kodu" },
            { "SmartLockUpdateCode", "Aktualizacja kodu" },
            { "SmartLockDeleteCode", "Usunięcie kodu" },
            { "ManagedOwnerManageOwners", "Zarządzanie właścicielami" },
            { "ManagedOwnerAddCost", "Dodawanie kosztów" },
            { "ManagedOwnerEditCost", "Edycja kosztów" },
            { "ManagedOwnerDeleteCost", "Usuwanie kosztów" },
        };

        /// <summary>
        /// Konwertuje RolePermissions na string oddzielony przecinkami z polskimi nazwami
        /// </summary>
        public static string ConvertPermissionsToString(RolePermissions permissions)
        {
            if (permissions == null) return "";

            var permissionNames = new List<string>();
            foreach (var prop in typeof(RolePermissions).GetProperties())
            {
                if (prop.PropertyType == typeof(bool))
                {
                    bool value = (bool?)prop.GetValue(permissions) ?? false;
                    if (value && PropertyNameToDisplayName.TryGetValue(prop.Name, out var displayName))
                    {
                        permissionNames.Add(displayName);
                    }
                }
            }

            return string.Join(", ", permissionNames);
        }

        /// <summary>
        /// Konwertuje string uprawnień (oddzielony przecinkami) na RolePermissions object
        /// </summary>
        public static RolePermissions ConvertStringToPermissions(string permissionsString)
        {
            var permissions = new RolePermissions();

            if (string.IsNullOrEmpty(permissionsString)) 
                return permissions;

            // Odwróć mapowanie
            var displayNameToPropertyName = PropertyNameToDisplayName
                .ToDictionary(x => x.Value, x => x.Key);

            var permissionNames = permissionsString.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var displayName in permissionNames)
            {
                if (displayNameToPropertyName.TryGetValue(displayName.Trim(), out var propName))
                {
                    var prop = typeof(RolePermissions).GetProperty(propName);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(permissions, true);
                    }
                }
            }

            return permissions;
        }
    }
}