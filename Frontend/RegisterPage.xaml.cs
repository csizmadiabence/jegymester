using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ticketmasterwpf.Services;
using ticketmasterwpf.Models;

namespace ticketmasterwpf
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();

            SetupValidation(NameInput, NameBorder);
            SetupValidation(EmailInput, EmailBorder);
            SetupValidation(PhoneInput, PhoneBorder);
            SetupValidation(PasswordInput, PasswordBorder);
            SetupValidation(PasswordConfirmInput, PasswordConfirmBorder);
        }

        private ToolTip activeErrorTip = null;

        //ELLENŐRZÉSEK:
        private void SetupValidation(Control inputControl, Border parentBorder)
        {
            if (inputControl is TextBox textBox)
            {
                textBox.TextChanged += (s, e) =>
                {
                    bool isValid = CheckTextBoxValidity(textBox);
                    ApplyValidationStyle(parentBorder, isValid);
                };

                textBox.GotFocus += (s, e) =>
                {
                    bool isValid = CheckTextBoxValidity(textBox);
                    ApplyValidationStyle(parentBorder, isValid);
                };

                textBox.LostFocus += (s, e) =>
                {
                    if (activeErrorTip != null) activeErrorTip.IsOpen = false;
                };
            }
            else if (inputControl is PasswordBox passwordBox)
            {
                passwordBox.PasswordChanged += (s, e) =>
                {
                    string errorMsg;
                    bool isValid = CheckPasswordValidity(passwordBox, out errorMsg);
                    ApplyValidationStyle(parentBorder, isValid, errorMsg);
                };

                passwordBox.GotFocus += (s, e) =>
                {
                    string errorMsg;
                    bool isValid = CheckPasswordValidity(passwordBox, out errorMsg);
                    ApplyValidationStyle(parentBorder, isValid, errorMsg);
                };

                passwordBox.LostFocus += (s, e) =>
                {
                    if (activeErrorTip != null) activeErrorTip.IsOpen = false;
                };
            }
        }
        private bool CheckTextBoxValidity(TextBox tb)
        {
            string input = tb.Text.Trim().Replace(" ", "").Replace("-", "");
            if (string.IsNullOrWhiteSpace(input)) return false;

            if (tb.Name == "EmailInput") return Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (tb.Name == "PhoneInput") return Regex.IsMatch(input, @"^[0-9]{6,15}$");

            return true;
        }

        private bool CheckPasswordValidity(PasswordBox pb, out string errorMessage)
        {
            string pass = pb.Password;
            errorMessage = "";

            bool isLongEnough = pass.Length >= 6;
            bool hasUpperCase = pass.Any(char.IsUpper);
            bool hasSpecial = pass.Any(ch => !char.IsLetterOrDigit(ch));

            if (!isLongEnough || !hasUpperCase || !hasSpecial)
            {
                errorMessage = "The password must be at least 6 characters long, contain 1 uppercase letter, and 1 special character!";
                return false;
            }

            if (pb.Name == "PasswordConfirmInput")
            {
                if (pass != PasswordInput.Password)
                {
                    errorMessage = "The two passwords do not match!";
                    return false;
                }
            }

            return true;
        }
        private void ApplyValidationStyle(Border border, bool isValid, string customMessage = null)
        {
            if (!isValid)
            {
                border.BorderBrush = Brushes.IndianRed;

                if (border.IsKeyboardFocusWithin)
                {
                    if (activeErrorTip != null) activeErrorTip.IsOpen = false;

                    string finalMessage = !string.IsNullOrEmpty(customMessage)
                                          ? customMessage
                                          : border.ToolTip?.ToString();

                    activeErrorTip = new ToolTip
                    {
                        Content = finalMessage,
                        PlacementTarget = border,
                        Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                        StaysOpen = true
                    };
                    activeErrorTip.IsOpen = true;
                }
            }
            else
            {
                border.ClearValue(Border.BorderBrushProperty);
                if (activeErrorTip != null) activeErrorTip.IsOpen = false;
            }
        }


        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        //JELSZÓ MEGTEKINTÉS:
        private void ShowPassword()
        {
            PasswordVisibleInput.Text = PasswordInput.Password;
            PasswordInput.Visibility = Visibility.Collapsed;
            PasswordVisibleInput.Visibility = Visibility.Visible;
        }

        private void HidePassword()
        {
            PasswordVisibleInput.Visibility = Visibility.Collapsed;
            PasswordInput.Visibility = Visibility.Visible;
            PasswordInput.Focus();
        }

        private void TogglePasswordButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowPassword();
        }

        private void TogglePasswordButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HidePassword();
        }

        private void TogglePasswordButton_MouseLeave(object sender, MouseEventArgs e)
        {
            HidePassword();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TogglePasswordButton.Visibility = PasswordInput.Password.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowConfirmPassword()
        {
            PasswordConfirmVisibleInput.Text = PasswordConfirmInput.Password;
            PasswordConfirmInput.Visibility = Visibility.Collapsed;
            PasswordConfirmVisibleInput.Visibility = Visibility.Visible;
        }

        private void HideConfirmPassword()
        {
            PasswordConfirmVisibleInput.Visibility = Visibility.Collapsed;
            PasswordConfirmInput.Visibility = Visibility.Visible;
            PasswordConfirmInput.Focus();
        }

        private void TogglePasswordConfirmButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowConfirmPassword();
        }

        private void TogglePasswordConfirmButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HideConfirmPassword();
        }

        private void TogglePasswordConfirmButton_MouseLeave(object sender, MouseEventArgs e)
        {
            HideConfirmPassword();
        }

        private void PasswordConfirmInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TogglePasswordConfirmButton.Visibility = PasswordConfirmInput.Password.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        //KEZELŐ GOMBOK:
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

        //REGISZTRÁCIÓS GOMB:
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg = "";

            if (string.IsNullOrWhiteSpace(NameInput.Text)) errorMsg = "Please enter your name!";
            else if (!CheckTextBoxValidity(EmailInput)) errorMsg = "Invalid email format!";
            else if (!CheckTextBoxValidity(PhoneInput)) errorMsg = "Invalid phone number!";
            else if (string.IsNullOrEmpty(PasswordInput.Password)) errorMsg = "Please enter a password!";
            else if (PasswordInput.Password != PasswordConfirmInput.Password) errorMsg = "The two passwords do not match!";
            string passError;
            if (string.IsNullOrEmpty(errorMsg) && !CheckPasswordValidity(PasswordInput, out passError))
                errorMsg = passError;

            if (!string.IsNullOrEmpty(errorMsg))
            {
                AppToast.ShowToast(errorMsg, false);
                return;
            }

            string prefix = ((ComboBoxItem)PhonePrefixCombo.SelectedItem).Tag.ToString();
            string pureNumber = PhoneInput.Text.Trim().Replace(" ", "").Replace("-", "");

            if (pureNumber.StartsWith("06")) pureNumber = pureNumber.Substring(2);
            if (pureNumber.StartsWith("+36")) pureNumber = pureNumber.Substring(3);

            string finalPhone = prefix + pureNumber;

            var newUser = new User
            {
                Username = NameInput.Text,
                Email = EmailInput.Text,
                PhoneNumber = finalPhone,
                PasswordHash = PasswordInput.Password,
                Roles = new()
            };

            bool isSuccess = await DataService.RegisterUserAsync(newUser);

            if (isSuccess)
            {
                AppToast.ShowToast("Successful registration!", true);
                await Task.Delay(1000);
                NavigationService.Navigate(new LoginPage());
            }
            else
            {
                AppToast.ShowToast("Error occurred! The email might already be taken.", false);
            }
        }
    }
}