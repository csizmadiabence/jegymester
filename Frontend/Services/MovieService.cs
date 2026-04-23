using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Services
{
    public class MovieService
    {
        private const string TmdbApiKey = "76676fa4c0832937412c82b2234c1cda";
        private readonly HttpClient _httpClient;

        public MovieService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<Movie> GetMovieFromImdbAsync(string title)
        {
            string searchUrl = $"https://api.themoviedb.org/3/search/movie?api_key={TmdbApiKey}&query={Uri.EscapeDataString(title)}&language=en-US";

            try
            {
                var searchResponse = await _httpClient.GetStringAsync(searchUrl);
                var searchData = JsonConvert.DeserializeObject<dynamic>(searchResponse);

                if (searchData.results != null && searchData.results.Count > 0)
                {
                    string movieId = (string)searchData.results[0].id;

                    string detailsUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={TmdbApiKey}&language=en-US";
                    var detailsResponse = await _httpClient.GetStringAsync(detailsUrl);
                    var detailsData = JsonConvert.DeserializeObject<dynamic>(detailsResponse);

                    var movie = new Movie
                    {
                        Title = (string)detailsData.title,
                        Description = (string)detailsData.overview,

                        ImdbRating = ((double?)detailsData.vote_average)?.ToString("0.0") ?? "N/A",

                        PosterUrl = detailsData.poster_path != null ? $"https://image.tmdb.org/t/p/w500{(string)detailsData.poster_path}" : null,

                        BackdropUrl = detailsData.backdrop_path != null ? $"https://image.tmdb.org/t/p/w1280{(string)detailsData.backdrop_path}" : null,

                        DurationMinutes = detailsData.runtime != null ? (int)detailsData.runtime : 0
                    };

                    if (detailsData.genres != null)
                    {
                        var genresList = new List<string>();
                        foreach (var genre in detailsData.genres)
                        {
                            genresList.Add((string)genre.name);
                        }
                        movie.Genre = string.Join(", ", genresList);
                    }

                    string releaseDateStr = (string)detailsData.release_date;
                    if (!string.IsNullOrEmpty(releaseDateStr) && DateTime.TryParse(releaseDateStr, out DateTime parsedDate))
                    {
                        movie.ReleaseDate = parsedDate;
                    }
                    else
                    {
                        movie.ReleaseDate = DateTime.MinValue;
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