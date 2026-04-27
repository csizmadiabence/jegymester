using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf
{
    public partial class CheckoutPage : Page, INotifyPropertyChanged
    {
        private List<Seat> _seats;
        private Screening _screening;
        private List<int> _savedTicketIds = new List<int>();
        private int _basePrice;

        public string SelectedMovieTitle { get; set; }
        public string SelectedShowtime { get; set; }
        public string SelectedRoomName { get; set; }
        public string SelectedDuration { get; set; }
        public string SelectedMoviePoster { get; set; }

        public ObservableCollection<SeatDisplayModel> SelectedSeatsList { get; set; }
        private string _totalAmount;
        public string TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        public CheckoutPage(List<Seat> seats, Screening screening)
        {
            InitializeComponent();

            _seats = seats;
            _screening = screening;
            _basePrice = (int)(screening?.Price ?? 3090);

            SelectedMovieTitle = screening?.Movie?.Title ?? "Unknown Movie";
            SelectedShowtime = screening?.StartTime.ToString("yyyy.MM.dd. HH:mm") ?? "00:00";
            SelectedRoomName = screening?.CinemaHall?.Name ?? "Unknown Room";
            SelectedDuration = screening?.Movie?.Duration.ToString() ?? "120 min";
            SelectedMoviePoster = screening?.Movie?.PosterUrl ?? "/Resources/Posters/default.jpg";

            SelectedSeatsList = new ObservableCollection<SeatDisplayModel>();
            foreach (var s in seats)
            {
                SelectedSeatsList.Add(new SeatDisplayModel { Seat = s, SeatInfo = $"ROW: {s.Row} | SEAT: {s.Number}", Price = _basePrice });
            }

            RecalculateTotal();
            this.DataContext = this;
        }

        private void TicketType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item && cb.Tag is SeatDisplayModel model)
            {
                double multiplier = double.Parse(item.Tag.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                model.Price = (int)(_basePrice * multiplier);
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            int total = SelectedSeatsList.Sum(s => s.Price);
            TotalAmount = $"{total:N0} Ft";
        }

        // --- MODÁLOK MEGHÍVÁSA ---

        private void ProceedToPayment_Click(object sender, RoutedEventArgs e)
        {
            string seatsSummary = string.Join(", ", SelectedSeatsList.Select(s => $"R{s.Seat.Row}S{s.Seat.Number}"));

            var bookingModal = new Modals.BookingModal(SelectedMovieTitle, seatsSummary, TotalAmount);

            bookingModal.PaymentConfirmed += async (s, args) =>
            {
                await ExecuteFinalPayment(args.email, args.phone);
            };

            (Window.GetWindow(this) as MainWindow)?.ShowModal(bookingModal);
        }

        private async Task ExecuteFinalPayment(string email, string phone)
        {
            try
            {
                _savedTicketIds.Clear();
                using (var client = new HttpClient())
                {
                    foreach (var seatModel in SelectedSeatsList)
                    {
                        var ticketData = new
                        {
                            ScreeningId = _screening.Id,
                            SeatId = seatModel.Seat.Id,
                            Price = seatModel.Price,
                            PurchaseDate = DateTime.Now,
                            UserId = DataService.CurrentUser?.Id,
                            GuestEmail = string.IsNullOrEmpty(email) ? null : email,
                            GuestPhone = string.IsNullOrEmpty(phone) ? null : phone
                        };

                        string json = JsonSerializer.Serialize(ticketData);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync("http://localhost:5035/api/Tickets", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            var savedTicket = JsonSerializer.Deserialize<Ticket>(responseJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (savedTicket != null) _savedTicketIds.Add(savedTicket.Id);
                        }
                        else
                        {
                            string errorDetails = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"SERVER ERROR DETAILS: {errorDetails}");
                            throw new Exception("Server-side validation error!");
                        }
                    }
                }

                (Window.GetWindow(this) as MainWindow)?.HideModal();
                SuccessOverlay.Visibility = Visibility.Visible;
                TriggerConfetti();
                AppToast.ShowToast("Purchase successful!", true);
            }
            catch (Exception ex)
            {
                AppToast.ShowToast("Payment failed! Check the logs.", false);
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private void ShowDigitalTicket()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
            string mainId = _savedTicketIds.Count > 0 ? _savedTicketIds[0].ToString() : "000";
            string finalTicketId = $"TID-{timestamp}-{mainId}";
            string mySeats = string.Join(", ", SelectedSeatsList.Select(s => $"R{s.Seat.Row}S{s.Seat.Number}"));

            var ticketModal = new Modals.DigitalTicketModal(
                SelectedMovieTitle,
                SelectedShowtime,
                SelectedRoomName,
                mySeats,
                finalTicketId,
                SelectedMoviePoster,
                TotalAmount,
                true
            );

            (Window.GetWindow(this) as MainWindow)?.HideModal();
            (Window.GetWindow(this) as MainWindow)?.ShowModal(ticketModal);
        }

        private void ViewTicket_Click(object sender, RoutedEventArgs e)
        {
            SuccessOverlay.Visibility = Visibility.Collapsed;
            ShowDigitalTicket();
        }

        private void TriggerConfetti()
        {
            ConfettiCanvas.Children.Clear();
            this.UpdateLayout();

            double centerX = ConfettiCanvas.ActualWidth / 2;
            double centerY = (ConfettiCanvas.ActualHeight / 2) - 90;

            if (centerX <= 0) centerX = this.ActualWidth / 2;
            if (centerY <= 0) centerY = (this.ActualHeight / 2) - 90;

            Random rnd = new Random();
            Color[] colors = { Colors.Gold, Colors.DodgerBlue, Colors.DeepPink, Colors.LimeGreen, Colors.DarkOrange, Colors.Cyan };

            for (int i = 0; i < 45; i++)
            {
                Rectangle confetti = new Rectangle
                {
                    Width = rnd.Next(2, 10),
                    Height = rnd.Next(6, 16),
                    Fill = new SolidColorBrush(colors[rnd.Next(colors.Length)]),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new TransformGroup
                    {
                        Children = new TransformCollection { new TranslateTransform(), new RotateTransform() }
                    },
                    Opacity = 0
                };

                Canvas.SetLeft(confetti, centerX);
                Canvas.SetTop(confetti, centerY);
                ConfettiCanvas.Children.Add(confetti);

                double angle = rnd.NextDouble() * 2 * Math.PI;
                double distance = rnd.Next(50, 150);
                double targetX = Math.Cos(angle) * distance;
                double targetY = Math.Sin(angle) * distance;

                TimeSpan duration = TimeSpan.FromSeconds(rnd.NextDouble() * 1.5 + 1.5);
                var sb = new Storyboard();

                var animX = new DoubleAnimation(0, targetX, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                var animY = new DoubleAnimation(0, targetY, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                var animRot = new DoubleAnimation(0, rnd.Next(180, 720), duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                var animOpacity = new DoubleAnimationUsingKeyFrames();
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, TimeSpan.FromSeconds(0.2)));
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, duration.Subtract(TimeSpan.FromSeconds(0.4))));
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, duration));

                Storyboard.SetTarget(animX, confetti);
                Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
                Storyboard.SetTarget(animY, confetti);
                Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
                Storyboard.SetTarget(animRot, confetti);
                Storyboard.SetTargetProperty(animRot, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
                Storyboard.SetTarget(animOpacity, confetti);
                Storyboard.SetTargetProperty(animOpacity, new PropertyPath("Opacity"));

                sb.Children.Add(animX); sb.Children.Add(animY); sb.Children.Add(animRot); sb.Children.Add(animOpacity);
                sb.Completed += (s, args) => ConfettiCanvas.Children.Remove(confetti);
                sb.Begin();
            }
        }

        // --- SEGÉD GOMBOK ---
        private void Back_Click(object sender, RoutedEventArgs e) { if (NavigationService.CanGoBack) NavigationService.GoBack(); }
        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e) { if (Window.GetWindow(this) is Window w) w.WindowState = WindowState.Minimized; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SeatDisplayModel : INotifyPropertyChanged
    {
        public Seat Seat { get; set; }
        public string SeatInfo { get; set; }

        private int _price;
        public int Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}