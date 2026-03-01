using System;
using System.Collections.Generic;
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

        private int _toastGeneration = 0;

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
            string input = tb.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (tb.Name == "EmailInput") return Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (tb.Name == "PhoneInput") return Regex.IsMatch(input, @"^((\+36)|(06))[0-9]{1,9}$");
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
                errorMessage = "A jelszó legyen minimum 6 karakter, 1 nagybetű és 1 különleges karakter!";
                return false;
            }

            if (pb.Name == "PasswordConfirmInput")
            {
                if (pass != PasswordInput.Password)
                {
                    errorMessage = "A két jelszó nem egyezik!";
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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg = "";

            if (string.IsNullOrWhiteSpace(NameInput.Text)) errorMsg = "Kérlek, add meg a nevedet!";
            else if (!CheckTextBoxValidity(EmailInput)) errorMsg = "Érvénytelen e-mail formátum!";
            else if (!CheckTextBoxValidity(PhoneInput)) errorMsg = "Érvénytelen telefonszám!";
            else if (string.IsNullOrEmpty(PasswordInput.Password)) errorMsg = "Kérlek, adj meg egy jelszót!";
            else if (PasswordInput.Password != PasswordConfirmInput.Password) errorMsg = "A két jelszó nem egyezik!";

            string passError;
            if (string.IsNullOrEmpty(errorMsg) && !CheckPasswordValidity(PasswordInput, out passError))
                errorMsg = passError;

            if (!string.IsNullOrEmpty(errorMsg))
            {
                ShowToast(errorMsg, false);
                return;
            }

            ShowToast("Sikeres regisztráció!", true);
        }

        private async void ShowToast(string message, bool isSuccess)
        {
            // 1. Sorszám kiosztása ennek a konkrét megjelenítésnek
            _toastGeneration++;
            int currentGen = _toastGeneration;

            // Színek beállítása
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

            // Animációk elindítása
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            var vanishAnim = new DoubleAnimation(pathLen, TimeSpan.FromSeconds(5));
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);

            // Várakozás 5 másodpercet
            await Task.Delay(5000);

            // 2. ELLENŐRZÉS: Ha időközben jött egy újabb hívás (spam), akkor ez a régi futás egyszerűen leáll itt!
            if (_toastGeneration != currentGen)
                return;

            // Ha ide eljutottunk, mi vagyunk a legutolsó hívás, nyugodtan eltűnhetünk.
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
