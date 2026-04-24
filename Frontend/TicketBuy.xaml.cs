using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf
{
    public partial class TicketBuy : Page, INotifyPropertyChanged
    {
        private Screening _currentScreening;
        private int _selectedCount = 0;

        public string SelectedMovieTitle => _currentScreening?.Movie?.Title ?? "Loading movie...";
        public string SelectedShowtime => _currentScreening?.StartTime.ToString("yyyy.MM.dd. HH:mm") ?? "00:00";
        public string SelectedRoomName => _currentScreening?.CinemaHall?.Name ?? "Loading room...";

        public ObservableCollection<TheaterRow> TheaterRows { get; set; } = new ObservableCollection<TheaterRow>();

        public TicketBuy(Screening screening)
        {
            InitializeComponent();
            _currentScreening = screening;
            DataContext = this;

            if (_currentScreening != null)
            {
                LoadHallStructure();
            }
        }

        public TicketBuy() { InitializeComponent(); DataContext = this; }

        private int _maxColumns;
        public int MaxColumns
        {
            get => _maxColumns;
            set { _maxColumns = value; OnPropertyChanged(); }
        }

        private async void LoadHallStructure()
        {
            if (_currentScreening?.CinemaHall == null) return;

            List<int> occupiedIds = await DataService.GetOccupiedSeatIds(_currentScreening.Id);

            OnPropertyChanged(nameof(SelectedMovieTitle));
            OnPropertyChanged(nameof(SelectedShowtime));
            OnPropertyChanged(nameof(SelectedRoomName));

            MaxColumns = _currentScreening.CinemaHall.Rows.Max(r => r.Seats.Count);
            TheaterRows.Clear();

            foreach (var dbRow in _currentScreening.CinemaHall.Rows.OrderBy(r => r.RowNumber))
            {
                var uiRow = new TheaterRow { RowNumber = dbRow.RowNumber, Seats = new ObservableCollection<Seat>() };

                var originalSeats = dbRow.Seats.OrderBy(s => s.Number).ToList();

                int displayCounter = 1;
                foreach (var seat in originalSeats)
                {
                    if (!seat.IsHidden)
                    {
                        seat.Number = displayCounter;
                        displayCounter++;
                    }
                    else
                    {
                        seat.Number = 0;
                    }

                    seat.IsSelected = false;
                    seat.IsOccupied = occupiedIds.Contains(seat.Id);
                }

                originalSeats.Reverse();

                foreach (var seat in originalSeats)
                {
                    uiRow.Seats.Add(seat);
                }

                while (uiRow.Seats.Count < MaxColumns)
                {
                    uiRow.Seats.Add(new Seat { IsHidden = true, Number = 0 });
                }

                TheaterRows.Add(uiRow);
            }
        }

        // --- UI ESEMÉNYEK (Gombok) ---
        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            UpdateBottomBar();
        }

        private void UpdateBottomBar()
        {
            var selectedSeats = TheaterRows.SelectMany(r => r.Seats).Where(s => s.IsSelected).ToList();
            _selectedCount = selectedSeats.Count;
            TicketCountText.Text = _selectedCount.ToString();

            if (_selectedCount > 0)
            {
                var grouped = selectedSeats.GroupBy(s => s.Row).OrderBy(g => g.Key)
                    .Select(g => $"ROW: {g.Key} SEATS: {string.Join(",", g.OrderBy(s => s.Number).Select(s => s.Number))}");
                SelectedSeatsText.Text = string.Join("\n", grouped);
                SelectedSeatsText.Visibility = Visibility.Visible;
                ConfirmButton.IsEnabled = true;
            }
            else
            {
                SelectedSeatsText.Visibility = Visibility.Collapsed;
                ConfirmButton.IsEnabled = false;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = TheaterRows.SelectMany(r => r.Seats).Where(s => s.IsSelected).ToList();
            if (selected.Count == 0) return;
            NavigationService?.Navigate(new CheckoutPage(selected, _currentScreening));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void CancelButton_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new HomePage());
        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e) { if (Window.GetWindow(this) is Window w) w.WindowState = WindowState.Minimized; }
    }
}