using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ticketmasterwpf
{
    public partial class CheckoutPage : Page
    {
        public string SelectedMovieTitle { get; set; }
        public string SelectedShowtime { get; set; }
        public List<SeatDisplayModel> SelectedSeatsList { get; set; }
        public string TotalAmount { get; set; }

        private dynamic currentUser = null;

        public CheckoutPage(List<Seat> seats, string title, string time)
        {
            InitializeComponent();

            SelectedMovieTitle = title;
            SelectedShowtime = time;

            SelectedSeatsList = seats.Select(s => new SeatDisplayModel
            {
                SeatInfo = $"ROW: {s.Row} | SEAT: {s.Number}"
            }).ToList();

            int total = seats.Count * 3090;
            TotalAmount = $"{total:N0} Ft";

            this.DataContext = this;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null) window.WindowState = WindowState.Minimized;
        }
        //TOVÁBBLÉPÉS A VÁSÁRLÁSHOZ
        private void ProceedToPayment_Click(object sender, RoutedEventArgs e)
        {
            OverlayMovieTitle.Text = SelectedMovieTitle;
            OverlayTotalPrice.Text = TotalAmount;
            OverlaySeatInfo.Text = string.Join(", ", SelectedSeatsList.Select(s => s.SeatInfo.Replace("ROW: ", "R").Replace(" | SEAT: ", " S")));

            if (currentUser != null)
            {
                GuestEmailBox.Text = currentUser.Email;
                GuestPhoneBox.Text = currentUser.Phone;
            }

            PurchaseOverlay.Visibility = Visibility.Visible;
        }
        //VÁSÁRLÁS MEGSZAKÍTÁS
        private void CancelPurchase_Click(object sender, RoutedEventArgs e)
        {
            PurchaseOverlay.Visibility = Visibility.Collapsed;
        }

        //VÁSÁRLÁS VÉGLEGESÍTÉS
        private async void FinalPay_Click(object sender, RoutedEventArgs e)
        {
            // Egyszerű validáció
            if (string.IsNullOrWhiteSpace(GuestEmailBox.Text))
            {
                ShowToast("Email is required!", false);
                return;
            }

            ActionButtonGrid.IsEnabled = false;
            PaymentProcessing.Visibility = Visibility.Visible;

            await Task.Delay(2000); // Fizetés szimulálása

            // Átváltás a Siker képernyőre
            PurchaseOverlay.Visibility = Visibility.Collapsed;
            SuccessOverlay.Visibility = Visibility.Visible;
            StartConfettiExplosion();

            ShowToast("Payment Successful!", true);
            ResetPurchaseProcess();

        }
        //TODO
        private void ViewTicket_Click(object sender, RoutedEventArgs e)
        {
            TicketMovieName.Text = SelectedMovieTitle;
            TicketDateTime.Text = SelectedShowtime;
            string generatedID = "TIC-" + new Random().Next(10000000, 99999999).ToString();
            TicketID.Text = generatedID;
            TicketSeats.Text = string.Join(", ", SelectedSeatsList.Select(s => s.SeatInfo.Replace("ROW: ", "R").Replace(" | SEAT: ", " S")));

            Random rnd = new Random();
            List<double> barWidths = new List<double>();
            double[] possibleWidths = { 1.0, 2.0, 3.0 };

            for (int i = 0; i < 40; i++)
            {
                barWidths.Add(possibleWidths[rnd.Next(possibleWidths.Length)]);
            }
            BarcodeVector.ItemsSource = barWidths;

            SuccessOverlay.Visibility = Visibility.Collapsed;
            TicketDigitalOverlay.Visibility = Visibility.Visible;
        }

        //RandomBarcode
        private void GenerateVectorBarcode()
        {
            List<int> bars = new List<int>();
            for (int i = 0; i < 45; i++)
            {
                bars.Add(1);
            }
            BarcodeVector.ItemsSource = bars;
        }

        private void CloseTicket_Click(object sender, RoutedEventArgs e)
        {
            TicketDigitalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ResetPurchaseProcess()
        {
            ActionButtonGrid.IsEnabled = true;
            PaymentProcessing.Visibility = Visibility.Collapsed;
        }

        //CONFETTI A VÁSÁRLÁSNÁL
        private void StartConfettiExplosion()
        {
            ConfettiCanvas.Children.Clear();
            Random rnd = new Random();
            Color[] colors = { Colors.Gold, Colors.DodgerBlue, Colors.DeepPink, Colors.LimeGreen, Colors.DarkOrange, Colors.Cyan };

            for (int i = 0; i < 35; i++)
            {
                Rectangle confetti = new Rectangle
                {
                    Width = rnd.Next(2, 8),
                    Height = rnd.Next(6, 14),
                    Fill = new SolidColorBrush(colors[rnd.Next(colors.Length)]),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new TransformGroup
                    {
                        Children = new TransformCollection { new TranslateTransform(), new RotateTransform() }
                    },
                    Opacity = 0
                };

                Canvas.SetLeft(confetti, 120);
                Canvas.SetTop(confetti, 80); ;
                ConfettiCanvas.Children.Add(confetti);

                double angle = rnd.NextDouble() * 2 * Math.PI;
                double distance = rnd.Next(40, 100);
                double targetX = Math.Cos(angle) * distance;
                double targetY = Math.Sin(angle) * distance;

                double durationValue = rnd.NextDouble() * 2.0 + 2.0;
                TimeSpan duration = TimeSpan.FromSeconds(durationValue);

                var sb = new Storyboard();

                var animX = new DoubleAnimation(0, targetX, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                var animY = new DoubleAnimation(0, targetY, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                var animRot = new DoubleAnimation(0, rnd.Next(180, 540), duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                var animOpacity = new DoubleAnimationUsingKeyFrames();
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, TimeSpan.FromSeconds(0.3)));
                animOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, duration));

                Storyboard.SetTarget(animX, confetti);
                Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

                Storyboard.SetTarget(animY, confetti);
                Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));

                Storyboard.SetTarget(animRot, confetti);
                Storyboard.SetTargetProperty(animRot, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));

                Storyboard.SetTarget(animOpacity, confetti);
                Storyboard.SetTargetProperty(animOpacity, new PropertyPath("Opacity"));

                sb.Children.Add(animX);
                sb.Children.Add(animY);
                sb.Children.Add(animRot);
                sb.Children.Add(animOpacity);

                sb.Begin();
            }
        }
        // --- TOAST LOGIKA ---
        private int _toastGeneration = 0;
        private async void ShowToast(string message, bool isSuccess)
        {
            _toastGeneration++;
            int currentGen = _toastGeneration;

            if (isSuccess)
            {
                ErrorIconBack.Visibility = Visibility.Collapsed; ErrorIconText.Visibility = Visibility.Collapsed;
                SuccessIconBack.Visibility = Visibility.Visible; SuccessIconText.Visibility = Visibility.Visible;
                LeftTimerStroke.Stroke = RightTimerStroke.Stroke = Brushes.MediumSeaGreen;
                BgPathLeft.Stroke = BgPathRight.Stroke = new SolidColorBrush(Color.FromArgb(40, 60, 179, 113));
            }
            else
            {
                ErrorIconBack.Visibility = Visibility.Visible; ErrorIconText.Visibility = Visibility.Visible;
                SuccessIconBack.Visibility = Visibility.Collapsed; SuccessIconText.Visibility = Visibility.Collapsed;
                LeftTimerStroke.Stroke = RightTimerStroke.Stroke = Brushes.IndianRed;
                BgPathLeft.Stroke = BgPathRight.Stroke = new SolidColorBrush(Color.FromArgb(40, 205, 92, 92));
            }

            ErrorText.Text = message;
            ErrorToast.Opacity = 0;
            ErrorToast.Visibility = Visibility.Visible;
            ErrorToast.UpdateLayout();

            double w = ErrorToast.ActualWidth; double h = ErrorToast.ActualHeight;
            double halfW = w / 2; double r = 12;
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            string leftData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {1:0.##},{1:0.##} 0 0 0 0,{1:0.##} L 0,{2:0.##} A {1:0.##},{1:0.##} 0 0 0 {1:0.##},{3:0.##} L {0:0.##},{3:0.##}", halfW, r, h - r, h);
            string rightData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {2:0.##},{2:0.##} 0 0 1 {3:0.##},{2:0.##} L {3:0.##},{4:0.##} A {2:0.##},{2:0.##} 0 0 1 {1:0.##},{5:0.##} L {0:0.##},{5:0.##}", halfW, w - r, r, w, h - r, h);

            LeftTimerStroke.Data = Geometry.Parse(leftData); RightTimerStroke.Data = Geometry.Parse(rightData);
            BgPathLeft.Data = LeftTimerStroke.Data; BgPathRight.Data = RightTimerStroke.Data;

            double pathLen = halfW + h + halfW + 10;
            LeftTimerStroke.StrokeDashArray = RightTimerStroke.StrokeDashArray = new DoubleCollection { pathLen, pathLen };
            LeftTimerStroke.StrokeDashOffset = RightTimerStroke.StrokeDashOffset = 0;

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            var vanishAnim = new DoubleAnimation(pathLen, TimeSpan.FromSeconds(5));
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);

            await Task.Delay(5000);
            if (_toastGeneration != currentGen) return;

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4));
            fadeOut.Completed += (s, e) => { ErrorToast.Visibility = Visibility.Collapsed; };
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    public class SeatDisplayModel
    {
        public string SeatInfo { get; set; }
        public string Price { get; set; } = "3,090 Ft";
        public string Type { get; set; } = "FULLPRICE";
    }
}