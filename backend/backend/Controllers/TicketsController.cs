using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController] 
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TicketsController(AppDbContext context)
    {
        _context = context;
    }

    //jegyvasarlas
    [HttpPost]
    public async Task<ActionResult<Ticket>> BuyTicket(Ticket ticket)
    {
        // szabaly: ha nincs userId, az e-mail és telefon kotelezo
        if (ticket.UserId == null && (string.IsNullOrEmpty(ticket.GuestEmail) || string.IsNullOrEmpty(ticket.GuestPhone)))
        {
            return BadRequest("Nem regisztrált felhasználóknak kötelező megadni az e-mailt és a telefonszámot!");
        }

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return Ok(ticket);
    }

    //jegy torlese (4 oras szabaly)
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelTicket(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Screening)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound("A jegy nem létezik.");

        //legkesobb a vetites kezdete elott 4 oraval torolheto
        if ((ticket.Screening.StartTime - DateTime.Now).TotalHours < 4)
        {
            return BadRequest("A törlés nem engedélyezett, mert kevesebb mint 4 óra van a vetítésig!");
        }

        ticket.IsCancelled = true;
        await _context.SaveChangesAsync();
        return Ok("Jegy sikeresen törölve.");
    }

    //penztaros jegy ellenorzese
    [HttpPatch("{id}/validate")]
    public async Task<IActionResult> ValidateTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.IsValidated = true; //penztaros visszaigazolja
        await _context.SaveChangesAsync();
        return Ok("Jegy érvényesítve.");
    }
}