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
        //letezik-e vetites
        var screeningExists = await _context.Screenings.AnyAsync(s => s.Id == ticket.ScreeningId);
        if (!screeningExists)
        {
            return NotFound($"A megadott vetítés (ID: {ticket.ScreeningId}) nem létezik.");
        }

        //letezik-e szek
        var seatExists = await _context.Seats.AnyAsync(s => s.Id == ticket.SeatId);
        if (!seatExists)
        {
            return NotFound($"A megadott szék (ID: {ticket.SeatId}) nem létezik.");
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

    //felhasznalo jegyei lekerese
    // path: GET api/tickets/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetUserTickets(int userId)
    {
        //jegy melle vetites es a szek is tarsul, megkeressuk
        var tickets = await _context.Tickets
            .Include(t => t.Screening)
            .Include(t => t.Seat)
            .Where(t => t.UserId == userId && !t.IsCancelled) // Csak az érvényeseket adjuk vissza!
            .ToListAsync();

        return Ok(tickets);
    }

    //egy jegy lekerese id alapjan
    //path: GET api/tickets/7
    [HttpGet("{id}")]
    public async Task<ActionResult<Ticket>> GetTicket(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Screening)
            .Include(t => t.Seat)
            .Include(t => t.User) 
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound("A jegy nem található.");

        return Ok(ticket);
    }

    //osszes jegy, az adminnak kell leginkabb
    //path: GET api/tickets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetAllTickets()
    {
        var tickets = await _context.Tickets
            .Include(t => t.Screening)
            .ToListAsync();

        return Ok(tickets);
    }
}