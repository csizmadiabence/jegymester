using System;
using System.Windows;
using System.Windows.Controls;

namespace ticketmasterwpf.Modals
{
    public partial class AddScreeningModal : UserControl
    {
        // 1. Esemény, ami szól a főoldalnak, ha mentettünk
        public event EventHandler<string> OnScreeningSaved;

        public AddScreeningModal()
        {
            InitializeComponent();
        }

        public void OpenModal()
        {
            MovieSelector.SelectedIndex = -1;
            DateInput.Clear();
            TimeInput.Clear();
            this.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Itt majd később összerakjuk a valós adatot, egyelőre csak küldünk egy üzenetet
            string dummyData = "Új vetítés mentve!";

            // 2. Szólunk a HomePage-nek!
            OnScreeningSaved?.Invoke(this, dummyData);

            this.Visibility = Visibility.Collapsed;
        }
    }
}