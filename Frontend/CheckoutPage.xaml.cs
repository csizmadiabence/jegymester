using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticketmasterwpf.Models;

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
                AppToast.ShowToast("Email is required!", false);
                return;
            }

            ActionButtonGrid.IsEnabled = false;
            PaymentProcessing.Visibility = Visibility.Visible;

            await Task.Delay(2000); // Fizetés szimulálása

            // Átváltás a Siker képernyőre
            PurchaseOverlay.Visibility = Visibility.Collapsed;
            SuccessOverlay.Visibility = Visibility.Visible;
            StartConfettiExplosion();

            AppToast.ShowToast("Payment Successful!", true);
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
    }
}