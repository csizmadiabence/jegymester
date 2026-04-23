using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ticketmasterwpf.Models;
using ticketmasterwpf.Controls;

namespace ticketmasterwpf.Modals
{
    public partial class MovieDetailModal : UserControl
    {
        private Movie _selectedMovie;
        public event EventHandler<string> ShowToastRequested;
        public MovieDetailModal()
        {
            InitializeComponent();
        }

        // --- MODÁL MEGNYITÁSA ---
        public void OpenModal(Movie movie)
        {
            if (movie == null) return;
            this.Visibility = Visibility.Visible;
            ModalRoot.Visibility = Visibility.Visible;

            _selectedMovie = movie;

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

        private void GetTicketsNow_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMovie?.Showtimes != null && _selectedMovie.Showtimes.Count > 0)
            {
                string firstTime = _selectedMovie.Showtimes[0];

                CloseModal();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow?.MainFrame != null)
                {
                    mainWindow.MainFrame.Navigate(new TicketBuy(_selectedMovie, firstTime));
                }
            }
        }

        private void Showtime_Click(object sender, RoutedEventArgs e)
        {
            var movie = this.DataContext as Movie;

            string selectedTime = (sender as Button)?.Content?.ToString() ?? "00:00";

            if (movie != null)
            {
                CloseModal();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null && mainWindow.MainFrame != null)
                {
                    mainWindow.MainFrame.Navigate(new TicketBuy(movie, selectedTime));
                }
            }
            else
            {
                ShowToastRequested?.Invoke(this, "Error: Movie data not found!");
            }
        }
    }
}