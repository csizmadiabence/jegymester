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

        // A helyes, mindenhol használt Action forma a Toasthoz
        public event Action<string, bool> ShowToastRequested;

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

            var btnGetTickets = ModalRoot.FindName("BtnGetTickets") as Button;
            if (btnGetTickets != null)
                btnGetTickets.Visibility = movie.Status == "Upcoming" ? Visibility.Collapsed : Visibility.Visible;

            if (movie.Status == "Upcoming")
            {
                ShowtimesItemsControl.Visibility = Visibility.Collapsed;
                NoShowtimesMessage.Visibility = Visibility.Collapsed;
                UpcomingMessagePanel.Visibility = Visibility.Visible;
            }
            else if (_selectedMovie.Showtimes == null || _selectedMovie.Showtimes.Count == 0)
            {
                ShowtimesItemsControl.Visibility = Visibility.Collapsed;
                NoShowtimesMessage.Visibility = Visibility.Visible;
                UpcomingMessagePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowtimesItemsControl.Visibility = Visibility.Visible;
                NoShowtimesMessage.Visibility = Visibility.Collapsed;
                UpcomingMessagePanel.Visibility = Visibility.Collapsed;
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
            if (!(sender is Button button)) return;
            if (!(this.DataContext is Movie movie)) return;

            string selectedTime = button.Content?.ToString() ?? "00:00";

            var tempScreening = movie.Screenings?.FirstOrDefault(s =>
                s.StartTime.Date == _currentDate.Date &&
                s.StartTime.ToString("HH:mm") == selectedTime);

            if (tempScreening != null)
            {
                if (tempScreening.StartTime <= DateTime.Now)
                {
                    ShowToastRequested?.Invoke("Sorry, this screening has already started!", false);
                    CloseModal();
                    return;
                }

                // UI gomb frissítése és letiltása
                string originalText = button.Content.ToString();
                button.Content = "Loading...";
                button.IsEnabled = false;

                // Töltőképernyő bekapcsolása
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowLoading();

                try
                {
                    // Lekérjük a pontos azonosítót (ha van teljes verzió a DataService-ben, ha nincs, a temp-et használjuk)
                    var fullScreening = DataService.AllScreenings.FirstOrDefault(s => s.Id == tempScreening.Id);
                    int targetScreeningId = fullScreening != null ? fullScreening.Id : tempScreening.Id;

                    var detailedScreening = await DataService.GetScreeningById(targetScreeningId);
                    var occupiedSeats = await DataService.GetOccupiedSeatIds(targetScreeningId);

                    if (detailedScreening != null)
                    {
                        CloseModal();
                        mainWindow?.MainFrame.Navigate(new TicketBuy(detailedScreening, occupiedSeats));
                    }
                    else
                    {
                        ShowToastRequested?.Invoke("Error loading theater data!", false);
                    }
                }
                catch (Exception ex)
                {
                    ShowToastRequested?.Invoke($"Error: {ex.Message}", false);
                }
                finally
                {
                    // Gomb visszaállítása és töltőképernyő kikapcsolása
                    button.Content = originalText;
                    button.IsEnabled = true;
                    mainWindow?.HideLoading();
                }
            }
        }

        private async void GetEarliestTickets_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMovie == null) return;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowLoading();

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
                        // Nem deklaráljuk újra a mainWindow-t, csak használjuk!
                        mainWindow?.MainFrame.Navigate(new TicketBuy(detailedScreening, occupiedSeats));
                    }
                    else
                    {
                        ShowToastRequested?.Invoke("Error: Could not load screening details.", false);
                    }
                }
                else
                {
                    ShowToastRequested?.Invoke("No more screenings available for today!", false);
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke($"An error occurred: {ex.Message}", false);
            }
            finally
            {
                mainWindow?.HideLoading();
            }
        }
    }
}