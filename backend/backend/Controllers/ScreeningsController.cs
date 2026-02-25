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

    [HttpGet] //vetitesek listazasa
    public async Task<ActionResult<IEnumerable<Screening>>> GetScreenings()
    {
        return await _context.Screenings.Include(s => s.Movie).ToListAsync();
    }

    [HttpPost] //uj vetites felvetele (admin)
    public async Task<ActionResult<Screening>> PostScreening(Screening screening)
    {
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        return Ok(screening);
    }
}