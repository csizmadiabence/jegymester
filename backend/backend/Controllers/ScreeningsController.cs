using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScreeningsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Screening>>> GetScreenings()
    {
        var screenings = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.CinemaHall)
            .ToListAsync();

        return Ok(screenings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Screening>> GetScreening(int id)
    {
        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.CinemaHall)
                .ThenInclude(ch => ch.Rows)
                .ThenInclude(r => r.Seats)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screening == null) return NotFound();

        return screening;
    }

    [HttpPost] //uj vetites felvetele (admin)
    public IActionResult CreateScreening([FromBody] Screening newScreening)
    {
        // letezik-e a film
        var movieExists = _context.Movies.Any(m => m.Id == newScreening.MovieId);
        if (!movieExists)
        {
            return BadRequest($"Nem létezik film a megadott ID-val: {newScreening.MovieId}");
        }

        // mentes
        _context.Screenings.Add(newScreening);
        _context.SaveChanges();

        return Ok(newScreening);
    }

    [HttpDelete("{id}")] // delete
    public IActionResult DeleteScreening(int id)
    {
        
        var screening = _context.Screenings.Find(id);
        if (screening == null)
        {
            return NotFound($"A {id} azonosítóval nem található vetítés.");
        }

        _context.Screenings.Remove(screening);
        _context.SaveChanges();
        //remelhetoleg 204-es kod jon vissza sikeres torleskor
        return NoContent();

    }

    [HttpPut("{id}")] // edithez
    public async Task<IActionResult> PutScreening(int id, Screening screening)
    {
        if (id != screening.Id)
        {
            return BadRequest("ID mismatch");
        }

        _context.Entry(screening).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Screenings.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
}