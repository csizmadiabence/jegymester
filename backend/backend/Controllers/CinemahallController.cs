using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CinemaHallsController : ControllerBase
{
    private readonly AppDbContext _context;
    public CinemaHallsController(AppDbContext context) { _context = context; }

    [HttpGet] 
    public async Task<ActionResult<IEnumerable<CinemaHall>>> GetHalls()
    {
        return await _context.CinemaHalls
            .Include(h => h.Rows)
            .ThenInclude(r => r.Seats)
            .ToListAsync();
    }

    [HttpPost] 
    public async Task<ActionResult<CinemaHall>> CreateHall(CinemaHall hall)
    {
        _context.CinemaHalls.Add(hall);
        await _context.SaveChangesAsync();
        return Ok(hall);
    }
}