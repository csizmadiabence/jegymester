namespace backend.Models;

public enum UserRole
{
    User,       
    Cashier,    
    Admin       
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } //szemelyes adat modositasahoz
    public UserRole Role { get; set; } = UserRole.User;
}