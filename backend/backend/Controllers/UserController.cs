using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly AppDbContext _context;

	public UsersController(AppDbContext context)
	{
		_context = context;
	}

	//osszes felhasznalo lekerese
	[HttpGet]
	public async Task<ActionResult<IEnumerable<User>>> GetUsers()
	{
		return await _context.Users.ToListAsync();
	}

	//felhasznalo id alapjan lekerese
	[HttpGet("{id}")]
	public async Task<ActionResult<User>> GetUser(int id)
	{
		var user = await _context.Users.FindAsync(id);
		if (user == null) return NotFound();
		return user;
	}

	//regisztracio 
	[HttpPost]
	public async Task<ActionResult<User>> CreateUser(User user)
	{
        if (user.Roles == null)
        {
            user.Roles = new List<Role>();
        }

        if (!user.Roles.Any())
        {
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultRole != null) user.Roles.Add(defaultRole);
        }

        _context.Users.Add(user);
		await _context.SaveChangesAsync();

		//201-es statusz good
		return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
	}

	//delete
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteUser(int id)
	{
		var user = await _context.Users.FindAsync(id);
		if (user == null) return NotFound();

		_context.Users.Remove(user);
		await _context.SaveChangesAsync();
		return NoContent();
	}
}