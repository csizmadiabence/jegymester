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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ticketmasterwpf
{
    public partial class CheckoutPage : Page
    {
        public string SelectedMovieTitle { get; set; }
        public string SelectedShowtime { get; set; }
        public List<SeatDisplayModel> SelectedSeatsList { get; set; }
        public string TotalAmount { get; set; }

        public CheckoutPage(List<Seat> seats, string title, string time)
        {
            InitializeComponent();

            SelectedMovieTitle = title;
            SelectedShowtime = time;

            SelectedSeatsList = seats.Select(s => new SeatDisplayModel
            {
                SeatInfo = $"ROW: {s.Row} | SEAT: {s.Number}"
            }).ToList();

            int total = seats.Count * 3090;
            TotalAmount = $"{total:N0} Ft";

            this.DataContext = this;
        }
        //Vissza gomb:
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
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

    public class SeatDisplayModel
    {
        public string SeatInfo { get; set; }
        public string Price { get; set; } = "3,090 Ft";
        public string Type { get; set; } = "FULLPRICE";
    }
}