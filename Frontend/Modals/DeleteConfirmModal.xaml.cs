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

            if (item is Movie m)
                DeleteMessageText.Text = $"Biztosan törlöd a(z) {m.Title} filmet?";
            else if (item is Screening s)
                DeleteMessageText.Text = $"Biztosan törlöd a vetítést: {s.RoomName} ({s.StartTime:HH:mm})?";

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