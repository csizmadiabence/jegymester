using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MoviesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet] // filmek listazasa
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
    {
        return await _context.Movies
        .Include(m => m.Screenings)
        .ToListAsync();
    }

    [HttpPost] //uj film
    public async Task<ActionResult<Movie>> PostMovie(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return Ok(movie);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var movie = await _context.Movies
                                  .Include(m => m.Screenings)
                                  .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null) return NotFound();

        // Debug: Írd ki a konzolra, hogy hány vetítést talál
        Console.WriteLine($"Törlés előtt: {movie.Screenings.Count} vetítés tartozik a filmhez.");

        _context.Screenings.RemoveRange(movie.Screenings);
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}")] // edithez
    public async Task<IActionResult> PutMovie(int id, Movie movie)
    {
        if (id != movie.Id) return BadRequest("Az ID-k nem egyeznek.");

        _context.Entry(movie).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Movies.Any(e => e.Id == id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpGet("by-date/{date}")] // film listázása adott napra
    public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByDate(DateTime date)
    {
        var movies = await _context.Movies
            .Include(m => m.Screenings)
            .Where(m => m.Screenings.Any(s => s.StartTime.Date == date.Date))
            .ToListAsync();

        return Ok(movies);
    }
}