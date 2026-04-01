namespace yBook.Helpers
{
    public class Pokoj
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
    }

    public static class PokojeRepo
    {
        public static List<Pokoj> Lista => new()
        {
            new() { Id = 1, Nazwa = "Ma³y pokój 1" },
            new() { Id = 2, Nazwa = "Pokój dwuosobowy typu Standard 2" },
            new() { Id = 3, Nazwa = "Pokój czteroosobowy typu Classic 3" },
            new() { Id = 4, Nazwa = "Pokój dwuosobowy typu Economy 4" },
            new() { Id = 5, Nazwa = "Pokój czteroosobowy typu Comfort 5" },
            new() { Id = 6, Nazwa = "Pokój Dwuosobowy typu Deluxe 6" },
            new() { Id = 7, Nazwa = "Pokój Dwuosobowy typu Deluxe 7" },
            new() { Id = 8, Nazwa = "Pokój Dwuosobowy typu Deluxe 8" },
            new() { Id = 9, Nazwa = "Pokój Dwuosobowy typu Deluxe 9" },
            new() { Id = 10, Nazwa = "Pokój Dwuosobowy typu Deluxe 10" },
            new() { Id = 11, Nazwa = "Pokój Dwuosobowy typu Deluxe 11" }
        };
    }
}