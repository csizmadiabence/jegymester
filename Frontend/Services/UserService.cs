using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Services
{
    public class UserService
    {
        private readonly string _apiUrl = "http://localhost:5035/api/Users";

        public async Task<(bool Success, string Message)> SaveUserAsync(User user, bool isNew)
        {
            try
            {
                using var client = new HttpClient();
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (isNew)
                    response = await client.PostAsync(_apiUrl, content);
                else
                    response = await client.PutAsync($"{_apiUrl}/{user.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Success");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return (false, errorMsg);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Network error: {ex.Message}");
            }
        }
    }
}