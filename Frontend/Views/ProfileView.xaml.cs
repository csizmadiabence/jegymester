using System;
using System.Windows;
using System.Windows.Controls;

namespace ticketmasterwpf.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Profil adatok mentve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}