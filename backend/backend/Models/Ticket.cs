using System.Text.Json.Serialization;
namespace backend.Models;

public class Ticket
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }

    [JsonIgnore]
    public Screening? Screening { get; set; }

    // Opcionalis csak ha van login
    public int? UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }

    public int SeatId { get; set; }
    [JsonIgnore] 
    public Seat? Seat { get; set; }
    
    // Kotelezo nem regisztralt vasarlas eseten
    public string? GuestEmail { get; set; }
    public string? GuestPhone { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.Now;

    public bool IsValidated { get; set; } = false; // penztaros allitja
    public bool IsCancelled { get; set; } = false; // torles allapot
    
    public int Price { get; set; }
    // 4 oras szabaly
    public bool CanBeCancelled
    {
        get
        {
            if (Screening == null) return false;
           
            return (Screening.StartTime - DateTime.Now).TotalHours >= 4;
        }
    }
}
