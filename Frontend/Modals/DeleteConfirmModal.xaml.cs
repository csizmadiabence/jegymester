using System;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Modals
{
    public partial class DeleteConfirmModal : UserControl
    {
        public event Action<object, object> OnDeleteConfirmed;

        private object _itemToDelete;

        public DeleteConfirmModal()
        {
            InitializeComponent();
        }

        // Főoldal hívja meg a filmmel együtt
        public void OpenModal(object item)
        {
            _itemToDelete = item;

            ModalTitleText.Text = "Confirm Deletion";
            ConfirmBtn.Content = "Delete";

            if (item is Movie m)
            {
                DeleteMessageText.Text = $"Are you sure you want to delete the movie: {m.Title}?";
            }
            else if (item is Screening s)
            {
                DeleteMessageText.Text = $"Are you sure you want to delete the screening: {s.CinemaHall?.Name} ({s.StartTime:HH:mm})?";
            }
            else if (item is GroupedTicket adminTicket)
            {
                ModalTitleText.Text = "Confirm Refund";
                ConfirmBtn.Content = "Refund";
                DeleteMessageText.Text = $"Are you sure you want to refund this ticket? ({adminTicket.CombinedSeats})";
            }
            else if (item is Views.ProfileView.GroupedTicket userTicket)
            {
                ModalTitleText.Text = "Cancel Ticket";
                ConfirmBtn.Content = "Cancel Ticket";
                DeleteMessageText.Text = $"Are you sure you want to cancel your reservation for {userTicket.MainTicket.MovieTitle}? ({userTicket.CombinedSeats})";
            }

            this.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            _itemToDelete = null;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_itemToDelete != null)
            {
                OnDeleteConfirmed?.Invoke(this, _itemToDelete);
            }

            this.Visibility = Visibility.Collapsed;
            _itemToDelete = null;
        }
    }
}