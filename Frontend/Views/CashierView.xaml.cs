using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Views
{
    public class OrderItem
    {
        public int ScreeningId { get; set; }
        public int SeatId { get; set; }
        public string MovieTitle { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string PriceString => $"{Price} Ft";
    }

    public class CashierTicketItem
    {
        public int Id { get; set; }
        public string SeatDisplay { get; set; }
        public string StatusText { get; set; }
        public bool CanValidate { get; set; }
        public bool CanCancel { get; set; }
    }

    public class AvailableSeatItem
    {
        public int SeatId { get; set; }
        public string DisplayName { get; set; }
    }

    public partial class CashierView : UserControl
    {
        public event EventHandler<string> VerifyTicketRequested;
        public event EventHandler<ObservableCollection<OrderItem>> IssueAllTicketsRequested;
        public event EventHandler<int> ValidateSingleTicketRequested;
        public event EventHandler<int> CancelSingleTicketRequested;

        public event Action<string, bool> ShowToastRequested;

        // Kosár tartalma
        public ObservableCollection<OrderItem> CurrentOrder { get; set; }
        public static readonly DependencyProperty MoviesProperty = DependencyProperty.Register("Movies", typeof(ObservableCollection<Movie>), typeof(CashierView));

        public ObservableCollection<Movie> Movies
        {
            get => (ObservableCollection<Movie>)GetValue(MoviesProperty);
            set => SetValue(MoviesProperty, value);
        }

        public static readonly DependencyProperty ScreeningsProperty = DependencyProperty.Register("Screenings", typeof(ObservableCollection<Screening>), typeof(CashierView));

        public ObservableCollection<Screening> Screenings
        {
            get => (ObservableCollection<Screening>)GetValue(ScreeningsProperty);
            set => SetValue(ScreeningsProperty, value);
        }

        private void SingleSeatValidate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int ticketId))
            {
                ValidateSingleTicketRequested?.Invoke(this, ticketId);
            }
        }

        public CashierView()
        {
            InitializeComponent();
            CurrentOrder = new ObservableCollection<OrderItem>();
            OrderItemsControl.ItemsSource = CurrentOrder;

            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshFutureScreenings();
        }

        private void PosScreeningSelector_DropDownOpened(object sender, EventArgs e)
        {
            RefreshFutureScreenings();
        }

        public void RefreshFutureScreenings()
        {
            if (Screenings != null)
            {
                var currentSelection = PosScreeningSelector.SelectedItem as Screening;

                var futureScreenings = Screenings
                    .Where(s => s.StartTime > DateTime.Now)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                PosScreeningSelector.ItemsSource = futureScreenings;

                if (currentSelection != null && futureScreenings.Any(s => s.Id == currentSelection.Id))
                {
                    PosScreeningSelector.SelectedItem = futureScreenings.First(s => s.Id == currentSelection.Id);
                }
            }
        }

        // ================= KOSÁR KEZELÉSE =================
        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (PosScreeningSelector.SelectedItem is Screening s &&
                PosTicketType.SelectedItem is ComboBoxItem type &&
                PosSeatSelector.SelectedItem is AvailableSeatItem selectedSeat &&
                selectedSeat.SeatId != -1)
            {
                decimal basePrice = s.Price;

                double multiplier = double.Parse(type.Tag.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                decimal finalPrice = basePrice * (decimal)multiplier;

                string typeName = type.Content.ToString().Split('(')[0].Trim();

                CurrentOrder.Add(new OrderItem
                {
                    ScreeningId = s.Id,
                    SeatId = selectedSeat.SeatId,
                    MovieTitle = s.Movie?.Title ?? "Unknown",
                    Description = $"{typeName} - {selectedSeat.DisplayName} ({s.StartTime:HH:mm})",
                    Price = (int)finalPrice
                });

                PosSeatSelector.Items.Remove(selectedSeat);
                if (PosSeatSelector.Items.Count > 0) PosSeatSelector.SelectedIndex = 0;

                UpdateTotal();
            }
        }

        private void RemoveFromOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderItem item)
            {
                CurrentOrder.Remove(item);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            int total = CurrentOrder.Sum(x => x.Price);
            TotalPriceText.Text = $"{total} Ft";
        }

        // ================= GOMBOK (FŐOLDAL HÍVÁSA) =================
        private void VerifyTicket_Click(object sender, RoutedEventArgs e)
        {
            string ticketId = TicketIdInput.Text.Trim();
            if (!string.IsNullOrEmpty(ticketId))
            {
                VerifyTicketRequested?.Invoke(this, ticketId);
            }
        }

        private void IssueTicket_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentOrder.Count > 0)
            {
                var orderCopy = new ObservableCollection<OrderItem>(CurrentOrder);

                IssueAllTicketsRequested?.Invoke(this, orderCopy);

                CurrentOrder.Clear();
                UpdateTotal();
            }
        }

        private void SingleSeatCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int ticketId))
            {
                CancelSingleTicketRequested?.Invoke(this, ticketId);
            }
        }

        //SZÉKVÁLASZTÁSHOZ KAPCSOLÓDÓ LOGIKA: Amikor a felhasználó kiválaszt egy vetítést, lekérjük a foglalt székeket, és csak a szabad székeket jelenítjük meg a PosSeatSelector-ban.

        private async void PosScreeningSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PosSeatSelector == null) return;
            PosSeatSelector.Items.Clear();

            if (PosScreeningSelector.SelectedItem is Screening s)
            {
                PosSeatSelector.IsEnabled = false;

                var occupiedIds = await ticketmasterwpf.Services.DataService.GetOccupiedSeatIds(s.Id);
                var hall = ticketmasterwpf.Services.DataService.AllCinemaHalls.FirstOrDefault(h => h.Id == s.CinemaHallId);

                int freeSeatsCount = 0;

                if (hall?.Rows != null)
                {
                    foreach (var row in hall.Rows)
                    {
                        if (row.Seats == null) continue;

                        int displayCounter = 1;

                        foreach (var seat in row.Seats)
                        {
                            if (seat.IsHidden) continue;

                            if (!occupiedIds.Contains(seat.Id))
                            {
                                PosSeatSelector.Items.Add(new AvailableSeatItem
                                {
                                    SeatId = seat.Id,
                                    DisplayName = $"R{row.RowNumber} S{displayCounter}"
                                });
                                freeSeatsCount++;
                            }
                            displayCounter++;
                        }
                    }
                }

                if (freeSeatsCount == 0)
                {
                    PosSeatSelector.Items.Add(new AvailableSeatItem { SeatId = -1, DisplayName = "-- Screening Sold Out! --" });
                    ShowToastRequested?.Invoke("This screening is sold out!", false);
                }

                PosSeatSelector.IsEnabled = true;
                PosSeatSelector.SelectedIndex = 0;
            }
            else
            {
                PosSeatSelector.Items.Add(new AvailableSeatItem { SeatId = -1, DisplayName = "-- Select a screening! --" });
                ShowToastRequested?.Invoke("Please select a screening!", false);
                PosSeatSelector.SelectedIndex = 0;
            }
        }

        // ================= VIZUÁLIS VISSZAJELZÉS (FŐOLDAL HÍVJA MEG) =================

        public void ShowOrderDetails(IEnumerable<CashierTicketItem> tickets)
        {
            Task.Run(() => Console.Beep(800, 100));

            ValidationResultPanel.Visibility = Visibility.Visible;
            GroupValidationPanel.Visibility = Visibility.Visible;
            OrderSeatsList.ItemsSource = tickets;

            ValidationIcon.Text = "🎟️";
            ValidationTitle.Text = "ORDER FOUND";
            ValidationTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FACFE"));

            ValidationResultPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A4FACFE"));
            ValidationResultPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FACFE"));

            ValidationDetails.Text = "Please select the arriving guests from the list below:";
        }

        public void ShowValidationResult(bool isValid, string details)
        {
            ValidationResultPanel.Visibility = Visibility.Visible;
            ValidationDetails.Text = details;

            if (GroupValidationPanel != null)
            {
                GroupValidationPanel.Visibility = Visibility.Collapsed;
            }

            if (isValid)
            {
                Task.Run(() => Console.Beep(1200, 150));

                ValidationResultPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A16C098"));
                ValidationResultPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16C098"));
                ValidationIcon.Text = "✅";
                ValidationTitle.Text = "VALIDATED!";
                ValidationTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16C098"));
            }
            else
            {
                Task.Run(() =>
                {
                    Console.Beep(350, 200);
                    Console.Beep(350, 200);
                });

                ValidationResultPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AFF5555"));
                ValidationResultPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5555"));
                ValidationIcon.Text = "❌";
                ValidationTitle.Text = "INVALID OR USED";
                ValidationTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5555"));
            }
        }
    }
}