using System;

namespace ticketmasterwpf.Models
{
    public class Screening
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal Price { get; set; }
        public Movie? Movie { get; set; }
    }
}