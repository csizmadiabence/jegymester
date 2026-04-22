using System;

namespace backend.Models;

public class Cashier : User
{
    public Cashier()
    {
        Role = UserRole.Cashier;
    }

    public bool ValidateTicket(bool isTicketValidInDatabase, bool isAlreadyUsed)
    {
        if (!isTicketValidInDatabase)
        {
            throw new InvalidOperationException("Érvénytelen vagy nem létező jegy!");
        }

        if (isAlreadyUsed)
        {
            throw new InvalidOperationException("Ezt a jegyet már felhasználták ezen a vetítésen!");
        }

        return true; 
    }

    
    public void ValidateOfflineTicketPurchase(string? customerEmail, string? customerPhone)
    {
        
        if (string.IsNullOrWhiteSpace(customerEmail) || string.IsNullOrWhiteSpace(customerPhone))
        {
            throw new ArgumentException("Helyszíni vásárlás esetén nem regisztrált vásárlónál kötelező az e-mail cím és a telefonszám megadása!");
        }
    }
}