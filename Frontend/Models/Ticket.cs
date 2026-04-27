using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Models
{
    public class Ticket
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("screeningId")]
        public int ScreeningId { get; set; }

        [JsonPropertyName("userId")]
        public int? UserId { get; set; }

        [JsonPropertyName("seatId")]
        public int SeatId { get; set; }

        [JsonPropertyName("guestEmail")]
        public string? GuestEmail { get; set; }

        [JsonPropertyName("guestPhone")]
        public string? GuestPhone { get; set; }

        [JsonPropertyName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [JsonPropertyName("isValidated")]
        public bool IsValidated { get; set; }

        [JsonPropertyName("isCancelled")]
        public bool IsCancelled { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonIgnore]
        public Screening? LinkedScreening => DataService.AllScreenings.FirstOrDefault(s => s.Id == ScreeningId);

        [JsonIgnore]
        public User? LinkedUser => UserId.HasValue ? DataService.AllUsers.FirstOrDefault(u => u.Id == UserId) : null;

        [JsonIgnore]
        public string CustomerEmail => LinkedUser != null ? LinkedUser.Email : (GuestEmail ?? "Guest");

        [JsonIgnore]
        public string CustomerName => LinkedUser != null ? LinkedUser.Username : "Guest";

        [JsonIgnore]
        public string MovieTitle => LinkedScreening?.Movie?.Title ?? "Unknown Movie";

        [JsonIgnore]
        public string SessionTime => LinkedScreening?.StartTime.ToString("yyyy.MM.dd. HH:mm") ?? "-";

        [JsonIgnore]
        public string SeatInfo => $"Seat ID: {SeatId}";

        [JsonIgnore]
        public string StatusText => IsCancelled ? "Cancelled" : (IsValidated ? "Validated" : "Valid");

        [JsonIgnore]
        public string RoomName => LinkedScreening?.CinemaHall?.Name ?? "Unknown Room";

        [JsonIgnore]
        public string RoomAndDate => $"{RoomName} | {SessionTime}";
    }

    public class GroupedTicket
    {
        public Ticket MainTicket { get; set; }
        public string CombinedSeats { get; set; }
        public int TotalPrice { get; set; }
        public List<Ticket> AllTicketsInGroup { get; set; }
    }

    public class ChartBar
    {
        public string Day { get; set; }
        public double Value { get; set; }
        public string Label { get; set; }
    }

    public class TopMovieStat
    {
        public string Title { get; set; }
        public int Sales { get; set; }
        public int Rank { get; set; }
    }
}