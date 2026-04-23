using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public partial class AddMovieModal : UserControl
    {
        private MovieService _movieService = new MovieService();
        public event EventHandler MovieSaved;

        public event Action<string, bool> ShowToastRequested;

        public AddMovieModal()
        {
            InitializeComponent();
        }

        private Movie _editingMovie = null;
        private string _tempBackdropUrl = null;
        private DateTime _tempReleaseDate = DateTime.MinValue;

        public void OpenModal(Movie movie = null)
        {
            _editingMovie = movie;

            if (_editingMovie == null)
            {
                _tempBackdropUrl = null;
                _tempReleaseDate = DateTime.MinValue;

                MovieModalTitle.Text = "Add New Movie to Database";
                SaveMovieBtn.Content = "Save Movie";
                ClearInputs();
                MovieStatusInput.SelectedIndex = 0;
            }
            else
            {
                _tempBackdropUrl = _editingMovie.BackdropUrl;
                _tempReleaseDate = _editingMovie.ReleaseDate;

                MovieModalTitle.Text = "Edit Movie Details";
                SaveMovieBtn.Content = "Update Movie";

                MovieTitleInput.Text = _editingMovie.Title;
                MovieDescriptionInput.Text = _editingMovie.Description;
                MovieDurationInput.Text = _editingMovie.DurationMinutes.ToString();
                MovieGenreInput.Text = _editingMovie.Genre;
                MovieRatingInput.Text = _editingMovie.ImdbRating;
                MoviePosterInput.Text = _editingMovie.PosterUrl;

                foreach (ComboBoxItem item in MovieStatusInput.Items)
                {
                    if (item.Content.ToString() == _editingMovie.Status)
                    {
                        MovieStatusInput.SelectedItem = item;
                        break;
                    }
                }
            }

            this.Visibility = Visibility.Visible;
        }

        private async void ImdbSearch_Click(object sender, RoutedEventArgs e)
        {
            string title = MovieTitleInput.Text;
            if (string.IsNullOrWhiteSpace(title)) return;

            var btn = (Button)sender;
            btn.IsEnabled = false;

            try
            {
                var movie = await _movieService.GetMovieFromImdbAsync(title);

                if (movie != null)
                {
                    // Adatok automatikus kitöltése
                    MovieTitleInput.Text = movie.Title;
                    MovieDescriptionInput.Text = movie.Description;
                    MovieGenreInput.Text = movie.Genre;
                    MovieRatingInput.Text = movie.ImdbRating;
                    MoviePosterInput.Text = movie.PosterUrl;
                    MovieDurationInput.Text = movie.DurationMinutes.ToString();

                    _tempBackdropUrl = movie.BackdropUrl;
                    _tempReleaseDate = movie.ReleaseDate;

                    ShowToastRequested?.Invoke("IMDb data imported successfully!", true);
                }
                else
                {
                    ShowToastRequested?.Invoke("Movie not found on IMDb.", false);
                }
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MovieTitleInput.Text))
            {
                ShowToastRequested?.Invoke("Please enter a movie title!", false);
                return;
            }

            if (!int.TryParse(MovieDurationInput.Text, out int duration))
            {
                ShowToastRequested?.Invoke("Please enter a valid duration in minutes!", false);
                return;
            }

            string inputTitle = MovieTitleInput.Text.Trim();

            bool movieExists = ticketmasterwpf.Services.DataService.AllMovies.Any(m =>
                m.Title.Equals(inputTitle, StringComparison.OrdinalIgnoreCase) &&
                (_editingMovie == null || m.Id != _editingMovie.Id));

            if (movieExists)
            {
                ShowToastRequested?.Invoke("Error: This movie already exists in the database!", false);
                return;
            }

            var movieData = new Movie
            {
                Id = _editingMovie?.Id ?? 0,
                Title = MovieTitleInput.Text,
                Description = MovieDescriptionInput.Text,
                DurationMinutes = duration,
                Genre = MovieGenreInput.Text,
                ImdbRating = MovieRatingInput.Text,
                PosterUrl = MoviePosterInput.Text,
                Status = (MovieStatusInput.SelectedItem as ComboBoxItem)?.Content.ToString(),
                BackdropUrl = string.IsNullOrWhiteSpace(_tempBackdropUrl)
                  ? "https://via.placeholder.com/1280x720.png?text=No+Background"
                  : _tempBackdropUrl,
                ReleaseDate = _tempReleaseDate == DateTime.MinValue ? DateTime.UtcNow : _tempReleaseDate
            };

            SaveMovieBtn.IsEnabled = false;
            SaveMovieBtn.Content = "Saving...";

            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = "http://localhost:5035/api/movies";
                    var json = JsonConvert.SerializeObject(movieData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response;

                    if (_editingMovie == null)
                    {
                        response = await client.PostAsync(apiUrl, content);
                    }
                    else
                    {
                        response = await client.PutAsync($"{apiUrl}/{movieData.Id}", content);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        ShowToastRequested?.Invoke($"{movieData.Title} saved successfully!", true);
                        MovieSaved?.Invoke(this, EventArgs.Empty);
                        this.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        ShowToastRequested?.Invoke($"Server hiba: {errorMsg}", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke($"Network error: {ex.Message}", false);
            }
            finally
            {
                SaveMovieBtn.IsEnabled = true;
                SaveMovieBtn.Content = "Save Movie";
            }
        }

        private void ClearInputs()
        {
            MovieTitleInput.Text = "";
            MovieDescriptionInput.Text = "";
            MovieDurationInput.Text = "";
            MovieGenreInput.Text = "";
            MovieRatingInput.Text = "";
            MoviePosterInput.Text = "https://...";
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}