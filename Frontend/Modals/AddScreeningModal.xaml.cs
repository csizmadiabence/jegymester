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
            }
            this.Visibility = Visibility.Visible;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (MovieSelector.SelectedValue == null)
            {
                ShowToastRequested?.Invoke("Please select a movie!", false);
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
                    s.RoomName == roomName &&
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
                    MovieId = selectedMovieId,
                    RoomName = roomName,
                    StartTime = startDateTime,
                    Price = price
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
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}