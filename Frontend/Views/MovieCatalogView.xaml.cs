using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Views
{
    public partial class MovieCatalogView : UserControl
    {

        public event EventHandler<Movie> MovieDetailRequested;

        public MovieCatalogView()
        {
            InitializeComponent();
        }

        // ================= ÜZEMMÓD VÁLTÁS (A HomePage hívja meg) =================
        public void SetMode(bool isNowShowing, IEnumerable movieSource)
        {
            // Feliratok cseréje
            NowShowingHeader.Visibility = isNowShowing ? Visibility.Visible : Visibility.Collapsed;
            SoonHeader.Visibility = isNowShowing ? Visibility.Collapsed : Visibility.Visible;

            // Adatok cseréje
            MoviesItemsControl.ItemsSource = movieSource;

            // Frissítő animáció lejátszása és lista visszagörgetése az elejére
            var anim = (Storyboard)this.Resources["ListRefreshAnimation"];
            anim?.Begin();
            MoviesScrollViewer?.ScrollToHome();
        }

        // ================= DÁTUM VÁLASZTÓ =================

        private void DateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.DataContext is DateItem selectedDate)
            {
                // Ha lekérjük a főoldal adatbázisát (DataContext)
                if (this.DataContext is HomePage homePage)
                {
                    foreach (var date in homePage.AvailableDates) date.IsSelected = false;
                    selectedDate.IsSelected = true;

                    var anim = (Storyboard)this.Resources["ListRefreshAnimation"];
                    anim?.Begin();
                }
            }
        }

        // ================= HÁTTÉR ANIMÁCIÓ ÉS VÁSÁRLÁS =================

        private void MovieCard_Click(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element?.DataContext is Movie selectedMovie)
            {
                MovieDetailRequested?.Invoke(this, selectedMovie);
            }
        }
        private void MovieCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is Movie m) UpdateAppBackground(m.PlaceholderColor);
        }

        private void MovieCard_MouseLeave(object sender, MouseEventArgs e) => UpdateAppBackground((Color)ColorConverter.ConvertFromString("#20232A"));

        private void UpdateAppBackground(Color target)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                var anim = new ColorAnimation { To = target, Duration = TimeSpan.FromMilliseconds(500) };
                mainWindow.BGGradientStop.BeginAnimation(GradientStop.ColorProperty, anim);
            }
        }

        private void Showtime_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetNavigationService(this)?.Navigate(new TicketBuy());
        }

        // ================= GÖRGETÉS KEZELÉS =================
        private void ScrollLeft_Click(object sender, RoutedEventArgs e) => MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset - 350);
        private void ScrollRight_Click(object sender, RoutedEventArgs e) => MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset + 350);

        private void MoviesScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
    }
}