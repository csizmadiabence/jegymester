namespace backend.Models;

using System.Collections.Generic;

public class TheaterRow
{
    public int Id { get; set; } 
    public int RowNumber { get; set; }
    public List<Seat> Seats { get; set; } = new();
}