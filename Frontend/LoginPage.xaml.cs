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
    public partial class LoginPage : Page
    {
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
                AppToast.ShowToast("Minden mezőt ki kell tölteni!", false);
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
    }
}
