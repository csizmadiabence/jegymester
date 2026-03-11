using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeatsController(AppDbContext context)
    {
        _context = context;
    }

    // ALL szek lekerdezese
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Seat>>> GetSeats()
    {
        // Lekérjük az összes széket az adatbázisból
        var seats = await _context.Seats.ToListAsync();
        return Ok(seats);
    }

    //szek hozzaadasa
    [HttpPost]
    public async Task<ActionResult<Seat>> CreateSeat(Seat seat)
    {
        
        _context.Seats.Add(seat);
        await _context.SaveChangesAsync();
        return Ok(seat);
    }

    //egy szek lekerdezese id alapjan
    [HttpGet("{id}")]
    public async Task<ActionResult<Seat>> GetSeat(int id)
    {
        var seat = await _context.Seats.FindAsync(id);

        if (seat == null)
        {
            return NotFound($"A {id} azonosítóval nem található szék.");
        }

        return Ok(seat);
    }

    //szek torlese id alapjan
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSeat(int id)
    {
        var seat = await _context.Seats.FindAsync(id);
        if (seat == null)
        {
            return NotFound("A törölni kívánt szék nem létezik.");
        }

        _context.Seats.Remove(seat);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 sikeres
    }
}