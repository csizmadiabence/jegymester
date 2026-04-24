using System;
using System.Text.Json.Serialization;

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
    }
}