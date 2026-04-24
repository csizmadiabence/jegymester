using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// db konfig path = appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();

    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Cashier" },
            new Role { Name = "User" }
        );
        context.SaveChanges();
    }

    if (!context.Users.Any(u => u.Email == "admin@ticketmaster.com"))
    {
        var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");

        var adminUser = new User
        {
            Username = "Admin",
            Email = "admin@ticketmaster.com",
            PhoneNumber = "+36300000000",
            PasswordHash = "admin",
            Roles = new List<Role>()
        };

        if (adminRole != null)
        {
            adminUser.Roles.Add(adminRole);
        }

        context.Users.Add(adminUser);
        context.SaveChanges();
    }

    if (!context.CinemaHalls.Any())
    {
        var halls = new List<CinemaHall>
        {
            CreateHallStructure("Screen 1", rows: 8, totalSlots: 16, rowsWithAisle: 7, aisleStart: 5, aisleWidth: 3),
            
            CreateHallStructure("Screen 2", rows: 6, totalSlots: 9, rowsWithAisle: 5, aisleStart: 5, aisleWidth: 2),
            
            CreateHallStructure("Screen 3", rows: 8, totalSlots: 15, rowsWithAisle: 7, aisleStart: 9, aisleWidth: 3),
            
            CreateHallStructure("Screen 4", rows: 6, totalSlots: 11, rowsWithAisle: 5, aisleStart: 7, aisleWidth: 2)
        };

        context.CinemaHalls.AddRange(halls);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();

static CinemaHall CreateHallStructure(string name, int rows, int totalSlots, int rowsWithAisle = 0, int aisleStart = 0, int aisleWidth = 0)
{
    var hall = new CinemaHall
    {
        Name = name,
        Rows = new List<TheaterRow>()
    };

    for (int r = 1; r <= rows; r++)
    {
        var theaterRow = new TheaterRow
        {
            RowNumber = r,
            Seats = new List<Seat>()
        };

        for (int s = 1; s <= totalSlots; s++)
        {
            bool isAisle = false;

            if (r <= rowsWithAisle)
            {
                if (s >= aisleStart && s < aisleStart + aisleWidth)
                {
                    isAisle = true;
                }
            }

            theaterRow.Seats.Add(new Seat
            {
                Row = r,
                Number = s,
                IsHidden = isAisle,
                IsOccupied = false
            });
        }

        hall.Rows.Add(theaterRow);
    }

    return hall;
}