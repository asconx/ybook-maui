using System;

namespace yBook.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal PricePerNight { get; set; }

        public Room() { }
        public Room(string id, string name, decimal price)
        {
            Id = id;
            Name = name;
            PricePerNight = price;
        }

        public override string ToString() => Name;
    }
}
