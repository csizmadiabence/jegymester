namespace backend.Models;
using System.Text.Json.Serialization;

public class Screening
{
    public int Id { get; set; }

    // Idegen kulcs a filmhez
    public int MovieId { get; set; }

    public int CinemaHallId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal Price { get; set; }

    public Movie? Movie { get; set; }

    public CinemaHall? CinemaHall { get; set; }
}