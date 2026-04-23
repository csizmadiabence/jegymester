namespace backend.Models;

using System.Collections.Generic;

public class CinemaHall
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TheaterRow> Rows { get; set; } = new();
}