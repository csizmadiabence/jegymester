using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

//db műveletek osztálya
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    
    public DbSet<User> Users { get; set; }           
    public DbSet<Movie> Movies { get; set; }         
    public DbSet<Screening> Screenings { get; set; } 
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Seat> Seats { get; set; }
}