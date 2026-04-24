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

    // 1. Összes felhasználó lekérése
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.Include(u => u.Roles).ToListAsync();
    }

    // 2. Felhasználó id alapján lekérése
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return user;
    }

    // 3. REGISZTRÁCIÓ
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            return BadRequest("Ezzel az e-mail címmel már regisztráltak!");

        if (user.Roles == null) user.Roles = new List<Role>();

        if (!user.Roles.Any())
        {
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultRole != null) user.Roles.Add(defaultRole);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // 4. BEJELENTKEZÉS
    [HttpPost("login")]
    public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.Include(u => u.Roles)
                                       .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || user.PasswordHash != request.Password)
        {
            return Unauthorized("Hibás e-mail cím vagy jelszó!");
        }

        return Ok(user);
    }

    // 5. PROFIL MÓDOSÍTÁS
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User updatedUser)
    {
        var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        user.Username = updatedUser.Username;
        user.PhoneNumber = updatedUser.PhoneNumber;

        if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
        {
            user.PasswordHash = updatedUser.PasswordHash;
        }

        user.Roles.Clear();

        if (updatedUser.Roles != null && updatedUser.Roles.Any())
        {
            foreach (var incomingRole in updatedUser.Roles)
            {
                var dbRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == incomingRole.Name);
                if (dbRole != null)
                {
                    user.Roles.Add(dbRole);
                }
            }
        }
        else
        {
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultRole != null) user.Roles.Add(defaultRole);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 6. Törlés
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

// Segédosztály a bejelentkezési adatok fogadásához
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}