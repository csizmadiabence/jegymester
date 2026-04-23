using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Services
{
    public static class DataService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static ObservableCollection<Movie> AllMovies { get; set; } = new ObservableCollection<Movie>();
        public static ObservableCollection<Screening> AllScreenings { get; set; } = new ObservableCollection<Screening>();

        public static Task InitializationTask { get; private set; }

        public static void StartLoading()
        {
            InitializationTask = LoadAllDataAsync();
        }

        private static async Task LoadAllDataAsync()
        {
            await Task.WhenAll(FetchMovies(), FetchScreenings());
        }

        public static async Task FetchMovies()
        {
            string apiUrl = "http://localhost:5035/api/Movies"; //
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //
                    var apiMovies = JsonSerializer.Deserialize<List<Movie>>(jsonString, options);

                    if (apiMovies != null)
                    {
                        AllMovies.Clear();
                        foreach (var movie in apiMovies) AllMovies.Add(movie);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching movies: {ex.Message}");
            }
        }

        public static async Task FetchScreenings()
        {
            string apiUrl = "http://localhost:5035/api/Screenings"; //
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //
                    var screenings = JsonSerializer.Deserialize<List<Screening>>(jsonString, options);

                    if (screenings != null)
                    {
                        AllScreenings.Clear();
                        foreach (var s in screenings) AllScreenings.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching screenings: {ex.Message}");
            }
        }
    }
}