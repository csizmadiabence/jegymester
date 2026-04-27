using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf
{
    public partial class TicketBuy : Page, INotifyPropertyChanged
    {
        private Screening _currentScreening;
        private int _selectedCount = 0;
        private List<int> _occupiedSeats;

        public string SelectedMovieTitle => _currentScreening?.Movie?.Title ?? "Loading movie...";
        public string SelectedShowtime => _currentScreening?.StartTime.ToString("yyyy.MM.dd. HH:mm") ?? "00:00";
        public string SelectedRoomName => _currentScreening?.CinemaHall?.Name ?? "Loading room...";

        public ObservableCollection<TheaterRow> TheaterRows { get; set; } = new ObservableCollection<TheaterRow>();

        public TicketBuy(Screening screening, List<int> occupiedSeats)
        {
            InitializeComponent();
            _currentScreening = screening;
            _occupiedSeats = occupiedSeats ?? new List<int>();
            DataContext = this;

            LoadHallStructure();
        }

        public TicketBuy() { InitializeComponent(); DataContext = this; }

        private int _maxColumns;
        public int MaxColumns
        {
            get => _maxColumns;
            set { _maxColumns = value; OnPropertyChanged(); }
        }

        private void LoadHallStructure()
        {
            if (_currentScreening?.CinemaHall == null)
            {
                MessageBox.Show("Hiba: A terem adatai nem érkeztek meg az API-tól!");
                return;
            }

            var previouslySelectedIds = TheaterRows.SelectMany(r => r.Seats)
                                                   .Where(s => s.IsSelected)
                                                   .Select(s => s.Id)
                                                   .ToList();

            List<int> occupiedIds = _occupiedSeats;

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
                    else { seat.Number = 0; }

                    seat.IsOccupied = occupiedIds.Contains(seat.Id);
                    seat.IsSelected = !seat.IsOccupied && previouslySelectedIds.Contains(seat.Id);
                }

                originalSeats.Reverse();
                foreach (var seat in originalSeats) { uiRow.Seats.Add(seat); }

                while (uiRow.Seats.Count < MaxColumns)
                {
                    uiRow.Seats.Add(new Seat { IsHidden = true, Number = 0 });
                }
                TheaterRows.Add(uiRow);
            }

            UpdateBottomBar();
        }

        // --- UI ESEMÉNYEK (Gombok) ---
        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
            var seat = toggle?.DataContext as Seat;
            if (seat == null) return;

            var selectedCount = TheaterRows.SelectMany(r => r.Seats).Count(s => s.IsSelected);
            if (seat.IsSelected && selectedCount > 10)
            {
                seat.IsSelected = false;
                AppToast.ShowToast("Maximum 10 tickets per transaction!", false);
                return;
            }

            UpdateBottomBar();
        }

        private bool ValidateSeatSelection(out string errorMessage)
        {
            errorMessage = "";
            foreach (var row in TheaterRows)
            {
                var seats = row.Seats.Where(s => !s.IsHidden).OrderBy(s => s.Number).ToList();

                for (int i = 0; i < seats.Count; i++)
                {
                    if (!seats[i].IsSelected && !seats[i].IsOccupied)
                    {
                        bool hasEmptyNeighbor = false;
                        if (i > 0 && !seats[i - 1].IsSelected && !seats[i - 1].IsOccupied) hasEmptyNeighbor = true;
                        if (i < seats.Count - 1 && !seats[i + 1].IsSelected && !seats[i + 1].IsOccupied) hasEmptyNeighbor = true;

                        if (!hasEmptyNeighbor)
                        {
                            bool leftBlocked = (i == 0) || seats[i - 1].IsSelected || seats[i - 1].IsOccupied;
                            bool rightBlocked = (i == seats.Count - 1) || seats[i + 1].IsSelected || seats[i + 1].IsOccupied;
                            bool isUsersFault = (i > 0 && seats[i - 1].IsSelected) || (i < seats.Count - 1 && seats[i + 1].IsSelected);

                            if (leftBlocked && rightBlocked && isUsersFault)
                            {
                                errorMessage = $"A single empty seat cannot be left in Row {row.RowNumber}!";
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
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

            if (!ValidateSeatSelection(out string error))
            {
                if (SingleSeatPopup != null)
                {
                    SingleSeatPopup.Visibility = Visibility.Visible;
                }
                return;
            }

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