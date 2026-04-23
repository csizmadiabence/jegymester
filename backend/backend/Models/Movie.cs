namespace backend.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? ImdbRating { get; set; }
    public string? PosterUrl { get; set; }
    public string? Year { get; set; }
    public string? Genre { get; set; }
    public string Status { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string BackdropUrl { get; set; }

    public List<Screening>? Screenings { get; set; }
}