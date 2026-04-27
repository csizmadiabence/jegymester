using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ticketmasterwpf.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public partial class MovieDetailModal : UserControl
    {
        private Movie _selectedMovie;
        private DateTime _currentDate;
        public event EventHandler<string> ShowToastRequested;
        public MovieDetailModal()
        {
            InitializeComponent();
        }

        // --- MODÁL MEGNYITÁSA ---
        public void OpenModal(Movie movie, DateTime selectedDate)
        {
            if (movie == null) return;
            this.Visibility = Visibility.Visible;
            ModalRoot.Visibility = Visibility.Visible;

            _selectedMovie = movie;
            _currentDate = selectedDate;

            this.DataContext = _selectedMovie;

            PosterDisplay.BorderBrush = new System.Windows.Media.SolidColorBrush(_selectedMovie.PlaceholderColor);
            PosterDisplay.BorderThickness = new Thickness(1);
            PosterDisplay.Opacity = 1.0;

            if (_selectedMovie.Showtimes == null || _selectedMovie.Showtimes.Count == 0)
            {
                ShowtimesItemsControl.Visibility = Visibility.Collapsed;
                NoShowtimesMessage.Visibility = Visibility.Visible;
            }
            else
            {
                ShowtimesItemsControl.Visibility = Visibility.Visible;
                NoShowtimesMessage.Visibility = Visibility.Collapsed;
            }

            var sb = (Storyboard)this.Resources["OpenModal"];
            sb?.Begin();

            this.Focus();
        }

        // --- MODÁL BEZÁRÁSA ---
        public void CloseModal()
        {
            var sb = (Storyboard)this.Resources["CloseModal"];
            sb?.Begin();

            this.DataContext = null;
            _selectedMovie = null;
        }

        // ================= ESEMÉNYEK KEZELÉSE =================

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            ModalRoot.Visibility = Visibility.Collapsed;
        }

        private void CloseModal_Click(object sender, MouseButtonEventArgs e)
        {
            ModalRoot.Visibility = Visibility.Collapsed;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) CloseModal();
        }

        private async void Showtime_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var movie = this.DataContext as Movie;
            string selectedTime = button.Content?.ToString() ?? "00:00";

            if (movie != null)
            {
                var tempScreening = movie.Screenings?.FirstOrDefault(s =>
                    s.StartTime.Date == _currentDate.Date &&
                    s.StartTime.ToString("HH:mm") == selectedTime);

                if (tempScreening != null)
                {
                    if (tempScreening.StartTime <= DateTime.Now)
                    {
                        ShowToastRequested?.Invoke(this, "Sorry, this screening has already started!");
                        CloseModal();
                        return;
                    }

                    var fullScreening = DataService.AllScreenings.FirstOrDefault(s => s.Id == tempScreening.Id);

                    string originalText = button.Content.ToString();
                    button.Content = "Loading...";
                    button.IsEnabled = false;

                    try
                    {
                        if (fullScreening != null)
                        {
                            var detailedScreening = await DataService.GetScreeningById(fullScreening.Id);
                            var occupiedSeats = await DataService.GetOccupiedSeatIds(fullScreening.Id);

                            if (detailedScreening != null)
                            {
                                CloseModal();
                                var mainWindow = Application.Current.MainWindow as MainWindow;
                                mainWindow?.MainFrame.Navigate(new TicketBuy(detailedScreening, occupiedSeats));
                            }
                            else
                            {
                                ShowToastRequested?.Invoke(this, "Error loading theater data!");
                            }
                        }
                        else
                        {
                            var detailedScreening = await DataService.GetScreeningById(tempScreening.Id);
                            var occupiedSeats = await DataService.GetOccupiedSeatIds(tempScreening.Id);

                            if (detailedScreening != null)
                            {
                                CloseModal();
                                var mainWindow = Application.Current.MainWindow as MainWindow;
                                mainWindow?.MainFrame.Navigate(new TicketBuy(detailedScreening, occupiedSeats));
                            }
                        }
                    }
                    finally
                    {
                        button.Content = originalText;
                        button.IsEnabled = true;
                    }
                }
            }
        }

        private async void GetEarliestTickets_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMovie == null) return;

            try
            {
                var nextScreening = DataService.AllScreenings
                    .Where(s => s.MovieId == _selectedMovie.Id && s.StartTime >= DateTime.Now)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault(); 

                if (nextScreening != null)
                {
                    var detailedScreening = await DataService.GetScreeningById(nextScreening.Id);
                    var occupiedSeats = await DataService.GetOccupiedSeatIds(nextScreening.Id);

                    if (detailedScreening != null)
                    {
                        CloseModal();

                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow?.MainFrame.Navigate(new TicketBuy(detailedScreening, occupiedSeats));
                    }
                    else
                    {
                        ShowToastRequested?.Invoke(this, "Error: Could not load screening details.");
                    }
                }
                else
                {
                    ShowToastRequested?.Invoke(this, "No more screenings available for today!");
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke(this, "An error occurred: " + ex.Message);
            }
        }
    }
}