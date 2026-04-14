using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace yBook.Models
{
    public class Role
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public RolePermissions Permissions { get; set; } = new();
        public DateTime DateModified { get; set; }

        // To jest właściwość, którą wyświetlamy w tabeli
        public string PermissionsDisplay => RolePermissions.ConvertPermissionsToReadableString(Permissions);
    }

    public class RolePermissions
    {
        // Tutaj powinny być wszystkie Twoje właściwości bool (skróciłem dla przykładu, upewnij się że masz wszystkie)
        public bool RezerRetComAddReservation { get; set; }
        public bool RezerRetComEditReservation { get; set; }
        public bool CalendarAddSchedule { get; set; }
        public bool UserEditRole { get; set; }
        // ... dopisz resztę swoich pól bool tutaj ...

        public static RolePermissions ConvertStringToPermissions(string permissionsString)
        {
            var permissions = new RolePermissions();
            if (string.IsNullOrEmpty(permissionsString)) return permissions;

            var parts = permissionsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var type = typeof(RolePermissions);

            foreach (var part in parts)
            {
                var prop = type.GetProperty(part.Trim());
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(permissions, true);
                }
            }
            return permissions;
        }

        public static string ConvertPermissionsToReadableString(RolePermissions permissions)
        {
            var active = new List<string>();
            var props = typeof(RolePermissions).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(bool) && (bool)(prop.GetValue(permissions) ?? false))
                {
                    // Tutaj zamieniamy techniczne nazwy na ładne (opcjonalnie)
                    // Na razie zwracamy nazwy właściwości, aby cokolwiek się pojawiło
                    active.Add(FormatPropName(prop.Name));
                }
            }

            return string.Join(", ", active);
        }

        private static string FormatPropName(string name)
        {
            // Prosta logika mapująca lub po prostu zwracanie nazwy
            return name;
        }
    }
}