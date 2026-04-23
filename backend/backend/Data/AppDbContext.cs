using backend.Models;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<Role> Roles { get; set; }
    public DbSet<CinemaHall> CinemaHalls { get; set; }
    public DbSet<TheaterRow> TheaterRows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Cashier" },
            new Role { Id = 3, Name = "User" }
        );
    }
    
}