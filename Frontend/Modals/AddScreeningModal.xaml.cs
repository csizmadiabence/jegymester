using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public partial class AddScreeningModal : UserControl
    {
        public event EventHandler ScreeningSaved;
        public event Action<string, bool> ShowToastRequested;

        private Screening _editingScreening = null;

        #region Dependency Properties
        public static readonly DependencyProperty AllMoviesProperty =
            DependencyProperty.Register(nameof(AllMovies), typeof(ObservableCollection<Movie>), typeof(AddScreeningModal));

        public ObservableCollection<Movie> AllMovies
        {
            get => (ObservableCollection<Movie>)GetValue(AllMoviesProperty);
            set => SetValue(AllMoviesProperty, value);
        }

        public static readonly DependencyProperty AllCinemaHallsProperty =
            DependencyProperty.Register(nameof(AllCinemaHalls), typeof(ObservableCollection<CinemaHall>), typeof(AddScreeningModal));

        public ObservableCollection<CinemaHall> AllCinemaHalls
        {
            get => (ObservableCollection<CinemaHall>)GetValue(AllCinemaHallsProperty);
            set => SetValue(AllCinemaHallsProperty, value);
        }
        #endregion

        public AddScreeningModal()
        {
            InitializeComponent();
            DateInput.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void OpenModal(Screening screening = null)
        {
            _editingScreening = screening;
            SetupUI();
            this.Visibility = Visibility.Visible;
        }

        private void SetupUI()
        {
            if (_editingScreening == null)
            {
                ModalTitle.Text = "Schedule New Screening";
                SaveScreeningBtn.Content = "Save Screening";
                ClearInputs();
            }
            else
            {
                ModalTitle.Text = "Edit Screening Details";
                SaveScreeningBtn.Content = "Update Screening";

                MovieSelector.SelectedValue = _editingScreening.MovieId;
                DateInput.Text = _editingScreening.StartTime.ToString("yyyy-MM-dd");
                TimeInput.Text = _editingScreening.StartTime.ToString("HH:mm");
                PriceInput.Text = _editingScreening.Price.ToString(CultureInfo.CurrentCulture);
                HallSelector.SelectedValue = _editingScreening.CinemaHallId;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(out DateTime startDateTime, out decimal price)) return;

            int movieId = (int)MovieSelector.SelectedValue;
            int hallId = (int)HallSelector.SelectedValue;

            // Ütközés és létezés ellenőrzése
            if (CheckForConflicts(movieId, hallId, startDateTime)) return;

            var screeningData = new Screening
            {
                Id = _editingScreening?.Id ?? 0,
                MovieId = movieId,
                CinemaHallId = hallId,
                StartTime = startDateTime,
                Price = price
            };

            await ExecuteSave(screeningData);
        }

        private bool ValidateForm(out DateTime startDateTime, out decimal price)
        {
            startDateTime = DateTime.MinValue;
            price = 0;

            if (MovieSelector.SelectedValue == null || HallSelector.SelectedValue == null)
            {
                ShowToastRequested?.Invoke("Please select both a movie and a hall!", false);
                return false;
            }

            try
            {
                DateTime datePart = DateTime.Parse(DateInput.Text);
                TimeSpan timePart = TimeSpan.Parse(TimeInput.Text);
                startDateTime = datePart.Date.Add(timePart);

                string priceText = PriceInput.Text.Replace(',', '.');
                if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                {
                    ShowToastRequested?.Invoke("Invalid price format!", false);
                    return false;
                }
            }
            catch
            {
                ShowToastRequested?.Invoke("Invalid date or time format!", false);
                return false;
            }

            return true;
        }

        private bool CheckForConflicts(int movieId, int hallId, DateTime start)
        {
            var selectedMovie = AllMovies.FirstOrDefault(m => m.Id == movieId);
            if (selectedMovie == null) return true;

            int duration = selectedMovie.DurationMinutes;
            DateTime end = start.AddMinutes(duration + 15); // +15 perc takarítás

            // 1. Pontos egyezés ellenőrzése
            bool exists = DataService.AllScreenings.Any(s =>
                s.MovieId == movieId && s.StartTime == start && s.CinemaHallId == hallId &&
                (_editingScreening == null || s.Id != _editingScreening.Id));

            if (exists)
            {
                ShowToastRequested?.Invoke("This exact screening already exists!", false);
                return true;
            }

            // 2. Időbeli átfedés ellenőrzése a teremben
            var overlaps = DataService.AllScreenings.Where(s =>
                s.CinemaHallId == hallId &&
                s.StartTime.Date == start.Date &&
                (_editingScreening == null || s.Id != _editingScreening.Id));

            foreach (var other in overlaps)
            {
                var otherMovie = AllMovies.FirstOrDefault(m => m.Id == other.MovieId);
                int otherDuration = otherMovie?.DurationMinutes ?? 120;
                DateTime otherEnd = other.StartTime.AddMinutes(otherDuration + 15);

                if (start < otherEnd && other.StartTime < end)
                {
                    ShowToastRequested?.Invoke($"Overlap! Room occupied: {other.StartTime:HH:mm} - {otherEnd:HH:mm}.", false);
                    return true;
                }
            }

            return false;
        }

        private async Task ExecuteSave(Screening screening)
        {
            SetLoadingState(true);
            try
            {
                using var client = new HttpClient();
                string url = "http://localhost:5035/api/Screenings";
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var content = new StringContent(JsonSerializer.Serialize(screening, options), Encoding.UTF8, "application/json");

                var response = (_editingScreening == null)
                    ? await client.PostAsync(url, content)
                    : await client.PutAsync($"{url}/{screening.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    ShowToastRequested?.Invoke("Screening saved successfully!", true);
                    ScreeningSaved?.Invoke(this, EventArgs.Empty);
                    this.Visibility = Visibility.Collapsed;
                    ClearInputs();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    ShowToastRequested?.Invoke($"Server error: {error}", false);
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke($"Network error: {ex.Message}", false);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            SaveScreeningBtn.IsEnabled = !isLoading;
            SaveScreeningBtn.Content = isLoading ? "Saving..." : (_editingScreening == null ? "Save Screening" : "Update Screening");
        }

        private void ClearInputs()
        {
            MovieSelector.SelectedIndex = -1;
            HallSelector.SelectedIndex = -1;
            DateInput.Text = DateTime.Today.ToString("yyyy-MM-dd");
            TimeInput.Text = "18:30";
            PriceInput.Text = "2500";
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;
    }
}