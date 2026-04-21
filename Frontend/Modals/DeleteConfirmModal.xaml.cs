using System;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Modals
{
    public partial class DeleteConfirmModal : UserControl
    {
        // 1. Esemény, ami visszaadja a törlendő filmet a HomePage-nek
        public event EventHandler<Movie> OnDeleteConfirmed;

        // Eltároljuk, hogy melyik filmet akarják épp törölni
        private Movie _movieToDelete;

        public DeleteConfirmModal()
        {
            InitializeComponent();
        }

        // Főoldal hívja meg a filmmel együtt
        public void OpenModal(Movie movie)
        {
            _movieToDelete = movie;

            if (movie != null)
            {
                // Így a felhasználó látja is a nevét annak, amit töröl!
                DeleteMessageText.Text = $"Are you sure you want to delete '{movie.Title}'?\nThis action cannot be undone.";
            }

            this.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            _movieToDelete = null;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_movieToDelete != null)
            {
                // 2. Szólunk a főoldalnak, hogy tényleg törölheti!
                OnDeleteConfirmed?.Invoke(this, _movieToDelete);
            }

            this.Visibility = Visibility.Collapsed;
            _movieToDelete = null;
        }
    }
}