using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Modals
{
    public partial class AddScreeningModal : UserControl
    {
        // Események a visszajelzéshez
        public event EventHandler ScreeningSaved;
        public event Action<string, bool> ShowToastRequested;

        // Filmek listája a ComboBox-hoz
        public static readonly DependencyProperty AllMoviesProperty =
            DependencyProperty.Register("AllMovies", typeof(ObservableCollection<Movie>), typeof(AddScreeningModal));

        public ObservableCollection<Movie> AllMovies
        {
            get => (ObservableCollection<Movie>)GetValue(AllMoviesProperty);
            set => SetValue(AllMoviesProperty, value);
        }

        public static readonly DependencyProperty AllCinemaHallsProperty = DependencyProperty.Register("AllCinemaHalls", typeof(ObservableCollection<CinemaHall>), typeof(AddScreeningModal));

        public ObservableCollection<CinemaHall> AllCinemaHalls
        {
            get => (ObservableCollection<CinemaHall>)GetValue(AllCinemaHallsProperty);
            set => SetValue(AllCinemaHallsProperty, value);
        }

        private Screening _editingScreening = null;

        public AddScreeningModal()
        {
            InitializeComponent();
            DateInput.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void OpenModal(Screening screening = null)
        {
            _editingScreening = screening;

            if (_editingScreening == null)
            {
                ModalTitle.Text = "Schedule New Screening";
                SaveScreeningBtn.Content = "Save Screening";
                HallSelector.SelectedIndex = -1;
                ClearInputs();
            }
            else
            {
                ModalTitle.Text = "Edit Screening Details";
                SaveScreeningBtn.Content = "Update Screening";

                MovieSelector.SelectedValue = _editingScreening.MovieId;
                DateInput.Text = _editingScreening.StartTime.ToString("yyyy-MM-dd");
                TimeInput.Text = _editingScreening.StartTime.ToString("HH:mm");
                PriceInput.Text = _editingScreening.Price.ToString();
                HallSelector.SelectedValue = _editingScreening.CinemaHallId;
            }
            this.Visibility = Visibility.Visible;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (MovieSelector.SelectedValue == null || HallSelector.SelectedValue == null)
            {
                ShowToastRequested?.Invoke("Please select both a movie and a hall!", false);
                return;
            }

            string priceRaw = PriceInput.Text.Replace(',', '.');
            if (!decimal.TryParse(priceRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            {
                ShowToastRequested?.Invoke("Please enter a valid price!", false);
                return;
            }

            if (ticketmasterwpf.Services.DataService.CurrentUser == null ||
                !ticketmasterwpf.Services.DataService.CurrentUser.Roles.Any(r => r.Name == "Admin"))
            {
                ShowToastRequested?.Invoke("Access denied: Admin rights required!", false);
                return;
            }

            var mainWin = Application.Current.MainWindow as MainWindow;
            mainWin?.ShowLoading();

            try
            {
                DateTime datePart = DateTime.Parse(DateInput.Text);
                TimeSpan timePart = TimeSpan.Parse(TimeInput.Text);
                DateTime startDateTime = datePart.Date.Add(timePart);

                int selectedMovieId = (int)MovieSelector.SelectedValue;
                int selectedHallId = (int)HallSelector.SelectedValue;

                bool screeningExists = ticketmasterwpf.Services.DataService.AllScreenings.Any(s =>
                    s.MovieId == selectedMovieId &&
                    s.StartTime == startDateTime &&
                    s.CinemaHallId == selectedHallId &&
                    (_editingScreening == null || s.Id != _editingScreening.Id));

                if (screeningExists)
                {
                    ShowToastRequested?.Invoke("Error: This exact screening already exists!", false);
                    return;
                }

                var selectedMovie = AllMovies.FirstOrDefault(m => m.Id == selectedMovieId);
                if (selectedMovie == null) return;

                DateTime endDateTime = startDateTime.AddMinutes(selectedMovie.DurationMinutes + 15);
                var overlap = ticketmasterwpf.Services.DataService.AllScreenings.FirstOrDefault(s =>
                    s.CinemaHallId == selectedHallId &&
                    s.StartTime.Date == startDateTime.Date &&
                    (_editingScreening == null || s.Id != _editingScreening.Id) &&
                    (startDateTime < s.StartTime.AddMinutes((AllMovies.FirstOrDefault(m => m.Id == s.MovieId)?.DurationMinutes ?? 120) + 15) && s.StartTime < endDateTime));

                if (overlap != null)
                {
                    ShowToastRequested?.Invoke($"Time overlap! Room occupied until {overlap.StartTime.AddMinutes(135):HH:mm}.", false);
                    return;
                }

                var screeningData = new Screening
                {
                    Id = _editingScreening?.Id ?? 0,
                    MovieId = selectedMovieId,
                    CinemaHallId = selectedHallId,
                    StartTime = startDateTime,
                    Price = price
                };

                using (var client = new HttpClient())
                {
                    string url = "http://localhost:5035/api/Screenings";
                    var json = JsonSerializer.Serialize(screeningData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = (_editingScreening == null)
                        ? await client.PostAsync(url, content)
                        : await client.PutAsync($"{url}/{screeningData.Id}", content);

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
                        ShowToastRequested?.Invoke($"Server Error: {error}", false);
                    }
                }
            }
            catch (Exception ex) 
            { 
                ShowToastRequested?.Invoke($"Error: {ex.Message}", false); 
            }
            finally 
            { 
                mainWin?.HideLoading(); 
            }
        }

        private void ClearInputs()
        {
            MovieSelector.SelectedIndex = -1;
            DateInput.Text = DateTime.Today.ToString("yyyy-MM-dd");
            TimeInput.Text = "18:30";
            PriceInput.Text = "2500";
            MovieSelector.SelectedIndex = -1;
            HallSelector.SelectedIndex = -1;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}