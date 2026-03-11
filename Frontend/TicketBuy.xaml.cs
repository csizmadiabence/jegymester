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

namespace ticketmasterwpf
{
    /// <summary>
    /// Interaction logic for GuestPage.xaml
    /// </summary>
    public partial class TicketBuy : Page
    {
        public string SelectedMovieTitle { get; set; } = "Dune: Part Two";
        public string SelectedShowtime { get; set; } = "17:45";
        public ObservableCollection<TheaterRow> TheaterRows { get; set; }
        private int _selectedCount = 0;

        private int _toastGeneration = 0;

        public TicketBuy()
        {
            InitializeComponent();
            GenerateSeats();
            DataContext = this;
        }
        //Székek generálása a 9 sorban, a 8 első sorban 12 szék van, a 9. sorban pedig 16 szék:
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

                ShowToast("Cannot select more than 10 seats.", false);
                return;
            }

            _selectedCount = currentSelected;
            UpdateBottomBar();
        }
        //HA BLOCKER VAN KÖZVETLEN MELLETTE, NEM LEHET KIVÁLASZTANI:
        private bool IsBlocker(ObservableCollection<Seat> rowSeats, int index)
        {
            if (index < 0 || index >= rowSeats.Count) return true;

            var seat = rowSeats[index];

            return seat.IsHidden || seat.IsOccupied || seat.IsSelected;
        }
        //Confirm gomb a továbblépéshez:
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSeats = TheaterRows
                    .SelectMany(row => row.Seats)
                    .Where(seat => seat.IsSelected)
                    .ToList();

            if (selectedSeats.Count == 0)
            {
                ShowToast("Please select at least one seat!", false);
                return;
            }

            bool hasSingleEmptySeat = false;

            for (int r = 0; r < TheaterRows.Count; r++)
            {
                var seats = TheaterRows[r].Seats;
                for (int s = 0; s < seats.Count; s++)
                {
                    var seat = seats[s];
                    if (!seat.IsHidden && !seat.IsOccupied && !seat.IsSelected)
                    {
                        bool isLeftBlocked = IsBlocker(seats, s - 1);
                        bool isRightBlocked = IsBlocker(seats, s + 1);

                        if (isLeftBlocked && isRightBlocked)
                        {
                            hasSingleEmptySeat = true;
                            break;
                        }
                    }
                }
                if (hasSingleEmptySeat) break;
            }

            if (hasSingleEmptySeat)
            {
                SingleSeatModalOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                ShowToast("Seats confirmed! Ready to navigate.", true);
            }
        }
        //Close modal, ha feljön a hibaüzenet:
        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            SingleSeatModalOverlay.Visibility = Visibility.Collapsed;
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

        //Hibás vagy éppen sikeres üzenet felül:
        private async void ShowToast(string message, bool isSuccess)
        {
            _toastGeneration++;
            int currentGen = _toastGeneration;

            Brush toastColor = isSuccess ? Brushes.MediumSeaGreen : Brushes.IndianRed;
            Brush bgColor = isSuccess ? new SolidColorBrush(Color.FromArgb(50, 60, 179, 113)) : new SolidColorBrush(Color.FromArgb(50, 205, 92, 92));

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, null);
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, null);

            if (isSuccess)
            {
                ErrorIconBack.Visibility = Visibility.Collapsed;
                ErrorIconText.Visibility = Visibility.Collapsed;
                SuccessIconBack.Visibility = Visibility.Visible;
                SuccessIconText.Visibility = Visibility.Visible;

                LeftTimerStroke.Stroke = Brushes.MediumSeaGreen;
                RightTimerStroke.Stroke = Brushes.MediumSeaGreen;
                BgPathLeft.Stroke = new SolidColorBrush(Color.FromArgb(40, 60, 179, 113));
                BgPathRight.Stroke = BgPathLeft.Stroke;
            }
            else
            {
                ErrorIconBack.Visibility = Visibility.Visible;
                ErrorIconText.Visibility = Visibility.Visible;
                SuccessIconBack.Visibility = Visibility.Collapsed;
                SuccessIconText.Visibility = Visibility.Collapsed;

                LeftTimerStroke.Stroke = Brushes.IndianRed;
                RightTimerStroke.Stroke = Brushes.IndianRed;
                BgPathLeft.Stroke = new SolidColorBrush(Color.FromArgb(40, 205, 92, 92));
                BgPathRight.Stroke = BgPathLeft.Stroke;
            }

            ErrorText.Text = message;
            ErrorToast.Opacity = 0;
            ErrorToast.Visibility = Visibility.Visible;
            ErrorToast.UpdateLayout();

            double w = ErrorToast.ActualWidth;
            double h = ErrorToast.ActualHeight;
            double halfW = w / 2;
            double r = 12;
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            string leftData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {1:0.##},{1:0.##} 0 0 0 0,{1:0.##} L 0,{2:0.##} A {1:0.##},{1:0.##} 0 0 0 {1:0.##},{3:0.##} L {0:0.##},{3:0.##}", halfW, r, h - r, h);
            string rightData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {2:0.##},{2:0.##} 0 0 1 {3:0.##},{2:0.##} L {3:0.##},{4:0.##} A {2:0.##},{2:0.##} 0 0 1 {1:0.##},{5:0.##} L {0:0.##},{5:0.##}", halfW, w - r, r, w, h - r, h);

            LeftTimerStroke.Data = Geometry.Parse(leftData);
            RightTimerStroke.Data = Geometry.Parse(rightData);
            BgPathLeft.Data = LeftTimerStroke.Data;
            BgPathRight.Data = RightTimerStroke.Data;

            double pathLen = halfW + h + halfW + 10;
            LeftTimerStroke.StrokeDashArray = new DoubleCollection { pathLen, pathLen };
            RightTimerStroke.StrokeDashArray = new DoubleCollection { pathLen, pathLen };
            LeftTimerStroke.StrokeDashOffset = 0;
            RightTimerStroke.StrokeDashOffset = 0;

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            var vanishAnim = new DoubleAnimation(pathLen, TimeSpan.FromSeconds(5));
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);

            await Task.Delay(5000);

            if (_toastGeneration != currentGen)
                return;

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4));
            fadeOut.Completed += (s, e) => {
                ErrorToast.Visibility = Visibility.Collapsed;
                if (isSuccess)
                {
                    var selectedSeats = TheaterRows
                            .SelectMany(row => row.Seats)
                            .Where(seat => seat.IsSelected)
                            .ToList();

                    NavigationService.Navigate(new CheckoutPage(selectedSeats, SelectedMovieTitle, SelectedShowtime));
                }
            };
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    public class Seat : INotifyPropertyChanged
    {
        public int Row { get; set; }
        public int Number { get; set; }
        public bool IsHidden { get; set; }

        private bool _isOccupied;
        public bool IsOccupied
        {
            get => _isOccupied;
            set { _isOccupied = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TheaterRow
    {
        public int RowNumber { get; set; }
        public ObservableCollection<Seat> Seats { get; set; }
    }
}