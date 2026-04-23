using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ticketmasterwpf.Models;

namespace ticketmasterwpf
{
    /// <summary>
    /// Interaction logic for GuestPage.xaml
    /// </summary>
    public partial class TicketBuy : Page
    {
        public string SelectedMovieTitle { get; private set; }
        public string SelectedShowtime { get; private set; }
        public ObservableCollection<TheaterRow> TheaterRows { get; set; }
        private int _selectedCount = 0;

        public TicketBuy(Movie movie, string time)
        {
            InitializeComponent();

            // Ha a movie null, ne menjünk tovább, mert hiba lesz
            if (movie == null)
            {
                // Alapértelmezett értékek, hogy ne szálljon el a program
                SelectedMovieTitle = "Ismeretlen film";
                SelectedShowtime = time ?? "00:00";
            }
            else
            {
                SelectedMovieTitle = movie.Title;
                SelectedShowtime = time;
            }

            GenerateSeats();
            DataContext = this;
        }

        public TicketBuy()
        {
            InitializeComponent();
            GenerateSeats();
            DataContext = this;
        }

        private void GenerateSeats()
        {
            TheaterRows = new ObservableCollection<TheaterRow>();

            for (int r = 1; r <= 9; r++)
            {
                var row = new TheaterRow { RowNumber = r, Seats = new ObservableCollection<Seat>() };

                if (r < 9)
                {
                    for (int i = 12; i >= 9; i--) row.Seats.Add(new Seat { Row = r, Number = i });
                    for (int i = 0; i < 4; i++) row.Seats.Add(new Seat { IsHidden = true });
                    for (int i = 8; i >= 1; i--) row.Seats.Add(new Seat { Row = r, Number = i });
                }
                else
                {
                    for (int i = 16; i >= 1; i--) row.Seats.Add(new Seat { Row = r, Number = i });
                }
                TheaterRows.Add(row);
            }

        }
        //Updatelje a kiválaszott székeket a confirm gomb felett:
        private void UpdateBottomBar()
        {
            var selectedSeats = TheaterRows.SelectMany(r => r.Seats).Where(s => s.IsSelected).ToList();

            _selectedCount = selectedSeats.Count;
            TicketCountText.Text = _selectedCount.ToString();

            if (_selectedCount > 0)
            {
                var groupedSeats = selectedSeats
                    .GroupBy(s => s.Row)
                    .OrderBy(g => g.Key)
                    .Select(g => $"ROW: {g.Key} SEATS: {string.Join(",", g.OrderBy(s => s.Number).Select(s => s.Number))}");

                SelectedSeatsText.Text = string.Join("\n", groupedSeats);
                SelectedSeatsText.Visibility = Visibility.Visible;

                ConfirmButton.IsEnabled = true;
                ConfirmButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF9800"));
                ConfirmButton.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                SelectedSeatsText.Text = "";
                SelectedSeatsText.Visibility = Visibility.Collapsed;

                ConfirmButton.IsEnabled = false;
                ConfirmButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EEEEEE"));
                ConfirmButton.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999999"));
            }
        }

        //CSAK BUTTONOK:
        //Székválasztás gomb:
        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var seat = toggle?.DataContext as Seat;

            if (seat == null) return;

            int currentSelected = TheaterRows.SelectMany(r => r.Seats).Count(s => s.IsSelected);

            if (currentSelected > 10 && seat.IsSelected)
            {
                seat.IsSelected = false;
                toggle.IsChecked = false;

                AppToast.ShowToast("Cannot select more than 10 seats.", false);
                return;
            }

            _selectedCount = currentSelected;
            UpdateBottomBar();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSeats = TheaterRows
                    .SelectMany(row => row.Seats)
                    .Where(seat => seat.IsSelected)
                    .ToList();

            if (selectedSeats.Count == 0)
            {
                AppToast.ShowToast("Válassz legalább egy széket!", false);
                return;
            }

            // --- AZ ELLENŐRZŐ LOGIKA ---
            bool hasSingleEmptySeat = false;

            foreach (var row in TheaterRows)
            {
                var seats = row.Seats;
                for (int i = 0; i < seats.Count; i++)
                {
                    var seat = seats[i];

                    // Csak azt a széket nézzük, ami ÜRES marad
                    if (!seat.IsHidden && !seat.IsOccupied && !seat.IsSelected)
                    {
                        // Ha balról ÉS jobbról is "fal" vagy foglalt szék van, akkor lyuk maradt
                        if (IsSeatBlocked(seats, i - 1) && IsSeatBlocked(seats, i + 1))
                        {
                            hasSingleEmptySeat = true;
                            break;
                        }
                    }
                }
                if (hasSingleEmptySeat) break;
            }

            // --- DÖNTÉS ---
            if (hasSingleEmptySeat)
            {
                // Az új, különálló modál megnyitása
                // Ha írtál neki Open() metódust, akkor azt hívd, ha nem, akkor sima Visibility
                SingleSeatPopup.Visibility = Visibility.Visible;
            }
            else
            {
                // Ha nincs hiba, irány a fizetés
                NavigationService?.Navigate(new CheckoutPage(selectedSeats, SelectedMovieTitle, SelectedShowtime));
            }
        }

        // Ezt a segédfüggvényt ne felejtsd el a fájlban hagyni!
        private bool IsSeatBlocked(ObservableCollection<Seat> rowSeats, int index)
        {
            if (index < 0 || index >= rowSeats.Count) return true;
            var s = rowSeats[index];
            return s.IsHidden || s.IsOccupied || s.IsSelected;
        }
        //Close modal, ha feljön a hibaüzenet:
        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            SingleSeatPopup.Visibility = Visibility.Collapsed;
        }
        //Cancel gomb, visszadob a föoldalra:
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }
        //Exit és minimize gomb:
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}