using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        if (ticket.UserId == null)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(ticket.GuestEmail ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("Invalid email format.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(ticket.GuestPhone ?? "", @"^\+[0-9]{8,15}$"))
                return BadRequest("Invalid phone format. (e.g. +36301234567)");
        }

        var screeningExists = await _context.Screenings.AnyAsync(s => s.Id == ticket.ScreeningId);
        if (!screeningExists)
        {
            return NotFound($"The screening (ID: {ticket.ScreeningId}) does not exist.");
        }

        var seatExists = await _context.Seats.AnyAsync(s => s.Id == ticket.SeatId);
        if (!seatExists)
        {
            return NotFound($"The specified seat (ID: {ticket.SeatId}) does not exist.");
        }

        bool isOccupied = await _context.Tickets.AnyAsync(t =>
            t.SeatId == ticket.SeatId &&
            t.ScreeningId == ticket.ScreeningId &&
            !t.IsCancelled);

        if (isOccupied)
        {
            return BadRequest("This seat is already occupied!");
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
            .Include(t => t.Seat)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound("The ticket does not exist.");

        if ((ticket.Screening.StartTime - DateTime.Now).TotalHours < 4)
        {
            return BadRequest("Cancellation is not allowed because there are less than 4 hours until the screening!");
        }

        if (ticket.Seat != null)
        {
            ticket.Seat.IsOccupied = false;
        }

        ticket.IsCancelled = true;
        await _context.SaveChangesAsync();
        return Ok("The ticket has been successfully cancelled.");
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> RefundTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.IsCancelled = true;
        await _context.SaveChangesAsync();
        return Ok("The ticket has been successfully cancelled.");
    }

    [HttpGet("{screeningId}/occupied-seats")]
    public async Task<ActionResult<IEnumerable<int>>> GetOccupiedSeats(int screeningId)
    {
        var occupiedSeatIds = await _context.Tickets
            .Where(t => t.ScreeningId == screeningId && !t.IsCancelled)
            .Select(t => t.SeatId)
            .ToListAsync();

        return Ok(occupiedSeatIds);
    }

    //penztaros jegy ellenorzese
    [HttpPatch("{id}/validate")]
    public async Task<IActionResult> ValidateTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.IsValidated = true; //penztaros visszaigazolja
        await _context.SaveChangesAsync();
        return Ok("The ticket has been successfully validated.");
    }

    //felhasznalo jegyei lekerese
    // path: GET api/tickets/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetUserTickets(int userId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Screening)
            .Include(t => t.Seat)
            .Where(t => t.UserId == userId && !t.IsCancelled)
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

        if (ticket == null) return NotFound("The ticket does not exist.");

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