using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Services
{
    public static class UserStore
    {
        public static ObservableCollection<User> Users { get; } = new();

        public static void Add(User u)
        {
            if (u == null) return;
            Users.Add(u);
        }
    }
}