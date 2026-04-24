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

            try
            {
                DateTime datePart = DateTime.Parse(DateInput.Text);
                TimeSpan timePart = TimeSpan.Parse(TimeInput.Text);
                DateTime startDateTime = datePart.Date.Add(timePart);

                int selectedMovieId = (int)MovieSelector.SelectedValue;
                string roomName = "Main Hall";

                bool screeningExists = ticketmasterwpf.Services.DataService.AllScreenings.Any(s =>
                    s.MovieId == selectedMovieId &&
                    s.StartTime == startDateTime &&
                    s.CinemaHall?.Name == roomName &&
                    (_editingScreening == null || s.Id != _editingScreening.Id));

                if (screeningExists)
                {
                    ShowToastRequested?.Invoke("Error: This movie is already scheduled for this time and room!", false);
                    return;
                }

                SaveScreeningBtn.IsEnabled = false;
                SaveScreeningBtn.Content = "Saving...";

                string priceRaw = PriceInput.Text.Replace(',', '.');
                decimal price = decimal.Parse(priceRaw, System.Globalization.CultureInfo.InvariantCulture);

                var screeningData = new Screening
                {
                    Id = _editingScreening?.Id ?? 0,
                    MovieId = (int)MovieSelector.SelectedValue,
                    CinemaHallId = (int)HallSelector.SelectedValue,
                    StartTime = DateTime.Parse($"{DateInput.Text} {TimeInput.Text}"),
                    Price = decimal.Parse(PriceInput.Text)
                };

                using (var client = new HttpClient())
                {
                    string url = "http://localhost:5035/api/Screenings";
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    var json = JsonSerializer.Serialize(screeningData, options);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response;

                    if (_editingScreening == null)
                    {
                        response = await client.PostAsync(url, content);
                    }
                    else
                    {
                        response = await client.PutAsync($"{url}/{screeningData.Id}", content);
                    }

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
                SaveScreeningBtn.IsEnabled = true;
                SaveScreeningBtn.Content = _editingScreening == null ? "Save Screening" : "Update Screening";
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