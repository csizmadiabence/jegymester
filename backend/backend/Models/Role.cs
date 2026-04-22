namespace backend.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } // admin, cashier, user
    public List<User> Users { get; set; }
}