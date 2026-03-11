using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private int _toastGeneration = 0;
        public LoginPage()
        {
            InitializeComponent();
        }

        private bool isPasswordVisible = false;

        //Ellenőrzés a bejelentkezéshez:
        private void EmailInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool shouldShow = !string.IsNullOrEmpty(EmailInput.Text) && EmailInput.Text != "email@domain.com";

            if (shouldShow && !isPasswordVisible)
            {
                isPasswordVisible = true;
                PasswordBorder.Visibility = Visibility.Visible;

                DoubleAnimation anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
                PasswordScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
            else if (!shouldShow && isPasswordVisible)
            {
                isPasswordVisible = true;
            }
        }
        //Bejelentkezés gomb:
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailInput.Text) || string.IsNullOrWhiteSpace(PasswordInput.Password))
            {
                ShowToast("Minden mezőt ki kell tölteni!", false);
                return;
            }

            string role = "User";
            if (EmailInput.Text.Contains("admin")) role = "Admin";
            else if (EmailInput.Text.Contains("cashier")) role = "Cashier";

            HomePage home = new HomePage();
            home.ApplyTestRole(role);
            string displayName = EmailInput.Text.Split('@')[0];

            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(home);

                home.WelcomeUser(displayName);
            }
        }
        //Átnavigál a reg oldalra:
        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
        //Vissza a főoldalra:
        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }
        //Exit és minimize gombok:
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
                    NavigationService ns = NavigationService.GetNavigationService(this);
                    ns?.Navigate(new LoginPage());
                }
            };
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}
