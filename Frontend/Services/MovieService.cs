using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Services
{
    public class MovieService
    {
        private const string ApiKey = "d7a2ce4d";
        private readonly HttpClient _httpClient;

        public MovieService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<Movie> GetMovieFromImdbAsync(string title)
        {
            string url = $"http://www.omdbapi.com/?t={title}&apikey={ApiKey}";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<dynamic>(response);

                if (data.Response == "True")
                {
                    var movie = new Movie
                    {
                        Title = (string)data.Title,
                        Description = (string)data.Plot,
                        ImdbRating = (string)data.imdbRating,
                        PosterUrl = (string)data.Poster,
                        Genre = (string)data.Genre
                    };

                    string runtimeStr = (string)data.Runtime;
                    if (!string.IsNullOrEmpty(runtimeStr) && runtimeStr != "N/A")
                    {
                        string minutesOnly = runtimeStr.Replace(" min", "");
                        if (int.TryParse(minutesOnly, out int mins))
                        {
                            movie.DurationMinutes = mins;
                        }
                    }

                    return movie;
                }
            }
            catch (Exception)
            {
                
            }

            return null;
        }
    }
}