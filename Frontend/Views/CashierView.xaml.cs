using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Views
{
    // A kosár elemeinek modellje
    public class OrderItem
    {
        public string MovieTitle { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string PriceString => $"${Price}.00";
    }

    public partial class CashierView : UserControl
    {
        // Ezeken keresztül szólunk a főoldalnak (HomePage)
        public event EventHandler<string> VerifyTicketRequested;
        public event EventHandler<ObservableCollection<OrderItem>> IssueAllTicketsRequested;

        // Kosár tartalma
        public ObservableCollection<OrderItem> CurrentOrder { get; set; }

        public CashierView()
        {
            InitializeComponent();
            CurrentOrder = new ObservableCollection<OrderItem>();
            OrderItemsControl.ItemsSource = CurrentOrder; // Bekötjük a XAML-be a listát
        }

        // ================= KOSÁR KEZELÉSE =================
        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (PosMovieSelector.SelectedItem is Movie selectedMovie &&
                PosTicketType.SelectedItem is ComboBoxItem selectedType &&
                int.TryParse(PosQuantity.Text, out int qty) && qty > 0)
            {
                int pricePerTicket = int.Parse(selectedType.Tag.ToString());
                string typeName = selectedType.Content.ToString().Split('(')[0].Trim();

                CurrentOrder.Add(new OrderItem
                {
                    MovieTitle = selectedMovie.Title,
                    Description = $"{qty}x {typeName} ticket",
                    Price = pricePerTicket * qty
                });

                UpdateTotal();
                PosQuantity.Text = "1"; // Visszaállítjuk 1-re a mennyiséget
            }
            else
            {
                MessageBox.Show("Please select a movie and valid quantity!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            TotalPriceText.Text = $"${total}.00";
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
                // Elküldjük a teljes kosarat a HomePage-nek
                IssueAllTicketsRequested?.Invoke(this, CurrentOrder);

                // Sikeres küldés után ürítünk
                CurrentOrder.Clear();
                UpdateTotal();
            }
        }

        // ================= VIZUÁLIS VISSZAJELZÉS (FŐOLDAL HÍVJA MEG) =================
        public void ShowValidationResult(bool isValid, string details)
        {
            ValidationResultPanel.Visibility = Visibility.Visible;
            ValidationDetails.Text = details;

            if (isValid)
            {
                ValidationResultPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A16C098"));
                ValidationResultPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16C098"));
                ValidationIcon.Text = "✅";
                ValidationTitle.Text = "VALID TICKET";
                ValidationTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16C098"));
            }
            else
            {
                ValidationResultPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AFF5555"));
                ValidationResultPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5555"));
                ValidationIcon.Text = "❌";
                ValidationTitle.Text = "INVALID OR USED";
                ValidationTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5555"));
            }
        }
    }
}