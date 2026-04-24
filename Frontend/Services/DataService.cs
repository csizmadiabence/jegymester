using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Services
{
    public static class DataService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static ObservableCollection<Movie> AllMovies { get; set; } = new ObservableCollection<Movie>();
        public static ObservableCollection<Screening> AllScreenings { get; set; } = new ObservableCollection<Screening>();
        public static ObservableCollection<CinemaHall> AllCinemaHalls { get; set; } = new ObservableCollection<CinemaHall>();
        public static User CurrentUser { get; set; } = null;
        public static ObservableCollection<User> AllUsers { get; set; } = new ObservableCollection<User>();

        public static Task InitializationTask { get; private set; }

        public static void StartLoading()
        {
            InitializationTask = LoadAllDataAsync();
        }

        private static async Task LoadAllDataAsync()
        {
            await Task.WhenAll(FetchMovies(), FetchScreenings(), FetchCinemaHalls());
        }

        public static async Task FetchMovies()
        {
            string apiUrl = "http://localhost:5035/api/Movies";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiMovies = JsonSerializer.Deserialize<List<Movie>>(jsonString, options);

                    if (apiMovies != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AllMovies.Clear();
                            foreach (var movie in apiMovies) AllMovies.Add(movie);
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching movies: {ex.Message}"); }
        }

        public static async Task FetchScreenings()
        {
            string apiUrl = "http://localhost:5035/api/Screenings";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var screenings = JsonSerializer.Deserialize<List<Screening>>(jsonString, options);

                    if (screenings != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AllScreenings.Clear();
                            foreach (var s in screenings) AllScreenings.Add(s);
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching screenings: {ex.Message}"); }
        }

        public static async Task FetchCinemaHalls()
        {
            string apiUrl = "http://localhost:5035/api/CinemaHalls";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var halls = JsonSerializer.Deserialize<List<CinemaHall>>(jsonString, options);

                    if (halls != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AllCinemaHalls.Clear();
                            foreach (var h in halls) AllCinemaHalls.Add(h);
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching halls: {ex.Message}"); }
        }

        public static async Task FetchUsers()
        {
            string apiUrl = "http://localhost:5035/api/Users";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var users = JsonSerializer.Deserialize<List<User>>(jsonString, options);

                    if (users != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AllUsers.Clear();
                            foreach (var user in users) AllUsers.Add(user);
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching users: {ex.Message}"); }
        }

        public static async Task<List<int>> GetOccupiedSeatIds(int screeningId)
        {
            string apiUrl = $"http://localhost:5035/api/Tickets";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var allTickets = JsonSerializer.Deserialize<List<Ticket>>(jsonString, options);

                    return allTickets?
                        .Where(t => t.ScreeningId == screeningId && !t.IsCancelled)
                        .Select(t => t.SeatId)
                        .ToList() ?? new List<int>();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching tickets: {ex.Message}"); }
            return new List<int>();
        }

        public static async Task<bool> RegisterUserAsync(User newUser)
        {
            try
            {
                string json = JsonSerializer.Serialize(newUser);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:5035/api/Users", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // Bejelentkezés hívása
        public static async Task<bool> LoginUserAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                string json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:5035/api/Users/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    CurrentUser = JsonSerializer.Deserialize<User>(responseString, options);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        // Logout hívása
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}