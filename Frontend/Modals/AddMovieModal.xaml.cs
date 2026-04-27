using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public partial class AddMovieModal : UserControl
    {
        private readonly MovieService _movieService = new MovieService();
        private Movie _editingMovie;
        private string _tempBackdropUrl;
        private DateTime _tempReleaseDate;

        public event EventHandler MovieSaved;
        public event Action<string, bool> ShowToastRequested;

        public AddMovieModal()
        {
            InitializeComponent();
        }

        public void OpenModal(Movie movie = null)
        {
            _editingMovie = movie;
            ResetForm();

            if (_editingMovie != null)
            {
                PopulateFields(_editingMovie);
            }

            this.Visibility = Visibility.Visible;
        }

        private void ResetForm()
        {
            _tempBackdropUrl = null;
            _tempReleaseDate = DateTime.MinValue;

            MovieModalTitle.Text = _editingMovie == null ? "Add New Movie to Database" : "Edit Movie Details";
            SaveMovieBtn.Content = _editingMovie == null ? "Save Movie" : "Update Movie";

            ClearInputs();
            MovieStatusInput.SelectedIndex = 0;
        }

        private void PopulateFields(Movie movie)
        {
            _tempBackdropUrl = movie.BackdropUrl;
            _tempReleaseDate = movie.ReleaseDate;

            MovieTitleInput.Text = movie.Title;
            MovieDescriptionInput.Text = movie.Description;
            MovieDurationInput.Text = movie.DurationMinutes.ToString();
            MovieGenreInput.Text = movie.Genre;
            MovieRatingInput.Text = movie.ImdbRating;
            MoviePosterInput.Text = movie.PosterUrl;

            var statusItem = MovieStatusInput.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == movie.Status);
            if (statusItem != null) MovieStatusInput.SelectedItem = statusItem;
        }

        private async void ImdbSearch_Click(object sender, RoutedEventArgs e)
        {
            var title = MovieTitleInput.Text.Trim();
            if (string.IsNullOrEmpty(title)) return;

            SetLoadingState(true, (Button)sender);
            try
            {
                var movie = await _movieService.GetMovieFromImdbAsync(title);
                if (movie != null)
                {
                    MapMovieToInputs(movie);
                    ShowToastRequested?.Invoke("IMDb data imported successfully!", true);
                }
                else
                {
                    ShowToastRequested?.Invoke("Movie not found on IMDb.", false);
                }
            }
            finally
            {
                SetLoadingState(false, (Button)sender);
            }
        }

        private void MapMovieToInputs(Movie movie)
        {
            MovieTitleInput.Text = movie.Title;
            MovieDescriptionInput.Text = movie.Description;
            MovieGenreInput.Text = movie.Genre;
            MovieRatingInput.Text = movie.ImdbRating;
            MoviePosterInput.Text = movie.PosterUrl;
            MovieDurationInput.Text = movie.DurationMinutes.ToString();
            _tempBackdropUrl = movie.BackdropUrl;
            _tempReleaseDate = movie.ReleaseDate;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out int duration)) return;

            var movieData = CreateMovieObject(duration);

            SetLoadingState(true, SaveMovieBtn, "Saving...");
            try
            {
                bool success = await SendMovieToApi(movieData);
                if (success)
                {
                    ShowToastRequested?.Invoke($"{movieData.Title} saved successfully!", true);
                    MovieSaved?.Invoke(this, EventArgs.Empty);
                    this.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke($"Error: {ex.Message}", false);
            }
            finally
            {
                SetLoadingState(false, SaveMovieBtn, _editingMovie == null ? "Save Movie" : "Update Movie");
            }
        }

        private bool ValidateInputs(out int duration)
        {
            duration = 0;
            if (string.IsNullOrWhiteSpace(MovieTitleInput.Text))
            {
                ShowToastRequested?.Invoke("Please enter a movie title!", false);
                return false;
            }

            if (!int.TryParse(MovieDurationInput.Text, out duration))
            {
                ShowToastRequested?.Invoke("Invalid duration!", false);
                return false;
            }

            string inputTitle = MovieTitleInput.Text.Trim();
            bool exists = DataService.AllMovies.Any(m =>
                m.Title.Equals(inputTitle, StringComparison.OrdinalIgnoreCase) &&
                (_editingMovie == null || m.Id != _editingMovie.Id));

            if (exists)
            {
                ShowToastRequested?.Invoke("This movie already exists!", false);
                return false;
            }

            return true;
        }

        private Movie CreateMovieObject(int duration) => new Movie
        {
            Id = _editingMovie?.Id ?? 0,
            Title = MovieTitleInput.Text.Trim(),
            Description = MovieDescriptionInput.Text,
            DurationMinutes = duration,
            Genre = MovieGenreInput.Text,
            ImdbRating = MovieRatingInput.Text,
            PosterUrl = MoviePosterInput.Text,
            Status = (MovieStatusInput.SelectedItem as ComboBoxItem)?.Content.ToString(),
            BackdropUrl = string.IsNullOrWhiteSpace(_tempBackdropUrl) ? "https://via.placeholder.com/1280x720.png?text=No+Background" : _tempBackdropUrl,
            ReleaseDate = _tempReleaseDate == DateTime.MinValue ? DateTime.UtcNow : _tempReleaseDate
        };

        private async Task<bool> SendMovieToApi(Movie movie)
        {
            using (var client = new HttpClient())
            {
                string url = "http://localhost:5035/api/movies";
                var json = JsonConvert.SerializeObject(movie);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = (_editingMovie == null)
                    ? await client.PostAsync(url, content)
                    : await client.PutAsync($"{url}/{movie.Id}", content);

                if (response.IsSuccessStatusCode) return true;

                var error = await response.Content.ReadAsStringAsync();
                ShowToastRequested?.Invoke($"Server Error: {error}", false);
                return false;
            }
        }

        private void SetLoadingState(bool isLoading, Button btn, string loadingText = null)
        {
            btn.IsEnabled = !isLoading;
            if (loadingText != null) btn.Content = isLoading ? loadingText : btn.Content;
        }

        private void ClearInputs()
        {
            MovieTitleInput.Clear();
            MovieDescriptionInput.Clear();
            MovieDurationInput.Clear();
            MovieGenreInput.Clear();
            MovieRatingInput.Clear();
            MoviePosterInput.Text = "https://...";
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;
    }
}