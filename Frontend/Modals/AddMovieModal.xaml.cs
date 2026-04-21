using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public partial class AddMovieModal : UserControl
    {
        private MovieService _movieService = new MovieService();
        public event EventHandler MovieSaved;

        public AddMovieModal()
        {
            InitializeComponent();
        }

        private Movie _editingMovie = null;

        public void OpenModal(Movie movie = null)
        {
            _editingMovie = movie;

            if (_editingMovie == null)
            {
                MovieModalTitle.Text = "Add New Movie to Database";
                SaveMovieBtn.Content = "Save Movie";
                ClearInputs();
                MovieStatusInput.SelectedIndex = 0;
            }
            else
            {
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

            // Keresés gomb letiltása (vizuális visszajelzés)
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
                }
                else
                {
                    MessageBox.Show("Movie not found on IMDb.");
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
                MessageBox.Show("Please enter a movie title!");
                return;
            }

            if (!int.TryParse(MovieDurationInput.Text, out int duration))
            {
                MessageBox.Show("Please enter a valid duration in minutes!");
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
                Status = (MovieStatusInput.SelectedItem as ComboBoxItem)?.Content.ToString()
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
                        MessageBox.Show($"{movieData.Title} sikeresen mentve!");
                        MovieSaved?.Invoke(this, EventArgs.Empty);
                        ClearInputs();
                        this.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Szerver hiba: {errorMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hálózati hiba: {ex.Message}");
            }
            finally
            {
                SaveMovieBtn.IsEnabled = true;
                SaveMovieBtn.Content = "Save Movie";
            }
        }

        // Segédmetódus a mezők kiürítéséhez
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