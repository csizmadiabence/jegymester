using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public class PaymentEventArgs : EventArgs
    {
        public int? UserId { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public partial class BookingModal : UserControl
    {
        public event EventHandler<PaymentEventArgs> PaymentConfirmed;
        public event Action<string, bool> ShowToastRequested;

        public BookingModal(string movieTitle, string seatsInfo, string totalPrice)
        {
            InitializeComponent();
            MovieTitleTxt.Text = movieTitle;
            SeatsInfoTxt.Text = seatsInfo;
            TotalPriceTxt.Text = totalPrice;

            if (DataService.CurrentUser != null)
            {
                GuestEmailInput.Text = DataService.CurrentUser.Email;

                string fullPhone = DataService.CurrentUser.PhoneNumber ?? "";

                if (!string.IsNullOrEmpty(fullPhone))
                {
                    bool prefixMatched = false;
                    foreach (ComboBoxItem item in PhonePrefixCombo.Items)
                    {
                        string tag = item.Tag.ToString();

                        if (fullPhone.StartsWith(tag))
                        {
                            PhonePrefixCombo.SelectedItem = item;
                            PhoneInput.Text = fullPhone.Substring(tag.Length);
                            prefixMatched = true;
                            break;
                        }
                    }

                    if (!prefixMatched)
                    {
                        PhoneInput.Text = fullPhone.Replace("+", "");
                    }
                }
            }
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            string email = GuestEmailInput.Text.Trim();
            string prefix = (PhonePrefixCombo.SelectedItem as ComboBoxItem)?.Tag.ToString();
            string rawInput = PhoneInput.Text.Trim();
            string cleanNumber = System.Text.RegularExpressions.Regex.Replace(rawInput, @"[^\d]", "");

            string prefixDigits = prefix.Replace("+", "");
            if (cleanNumber.StartsWith(prefixDigits))
            {
                cleanNumber = cleanNumber.Substring(prefixDigits.Length);
            }

            string fullPhone = prefix + cleanNumber;

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowToastRequested?.Invoke("Please enter a valid email address!", false);
                return;
            }

            if (!Regex.IsMatch(cleanNumber, @"^[0-9]{7,12}$"))
            {
                ShowToastRequested?.Invoke("Please enter a valid phone number!", false);
                return;
            }

            var args = new PaymentEventArgs();

            if (DataService.CurrentUser != null)
            {
                args.UserId = DataService.CurrentUser.Id;
                args.email = email;
                args.phone = fullPhone;
            }
            else
            {
                args.UserId = null;
                args.email = email;
                args.phone = fullPhone;
            }

            PaymentConfirmed?.Invoke(this, args);

            (Window.GetWindow(this) as MainWindow)?.HideModal();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.HideModal();
        }
    }
}