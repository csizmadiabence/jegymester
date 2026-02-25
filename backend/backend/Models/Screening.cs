namespace backend.Models;

public class Screening
{
    public int Id { get; set; }

    // Idegen kulcs a filmhez
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public string RoomName { get; set; } = string.Empty; 
    public DateTime StartTime { get; set; }
    public decimal Price { get; set; }
}