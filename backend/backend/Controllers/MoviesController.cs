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
        return await _context.Movies.ToListAsync();
    }

    [HttpPost] //uj film
    public async Task<ActionResult<Movie>> PostMovie(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return Ok(movie);
    }

    [HttpDelete("{id}")] // piros torles gomb
    public async Task<IActionResult> DeleteMovie(int id)
    {
        // id alapjan megkeressuk a filmet
        var movie = await _context.Movies.FindAsync(id);

        // ha nincs 404-es hiba
        if (movie == null)
        {
            return NotFound("A film nem található.");
        }

        //ha megvan toroljuk es mentjuk
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return NoContent(); // 204-es valasz, sikeres torles
    }
    [HttpPut("{id}")]
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
}