using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    public partial class HomePage : Page
    {
        // Ezeket a tulajdonságokat köti be a XAML (Binding)
        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<Movie> ComingSoonMovies { get; set; }
        public ObservableCollection<DateItem> AvailableDates { get; set; }
        public Movie FeaturedMovie { get; set; }
        public enum UserRole { Guest, User, Cashier, Admin }
        private int _toastGeneration = 0;

        public HomePage()
        {
            InitializeComponent();

            // 1. DÁTUMOK GENERÁLÁSA (Csak egyszer!)
            AvailableDates = new ObservableCollection<DateItem>();
            for (int i = 0; i < 7; i++)
            {
                DateTime d = DateTime.Now.AddDays(i);
                AvailableDates.Add(new DateItem
                {
                    DayName = i == 0 ? "TODAY" : i == 1 ? "TOMORROW" : d.ToString("dddd", new System.Globalization.CultureInfo("en-US")).ToUpper(),
                    DateNumber = d.ToString("dd MMM"),
                    IsSelected = i == 0
                });
            }

            // 2. AKTUÁLIS FILMEK
            Movies = new ObservableCollection<Movie>
            {
                new Movie { Title = "Dune: Part Two", Genre = "Sci-Fi / Action", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#2C3E50"), Showtimes = new ObservableCollection<string> { "14:30", "17:45", "20:00" } },
                new Movie { Title = "Oppenheimer", Genre = "Biography / Drama", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#34495E"), Showtimes = new ObservableCollection<string> { "13:00", "16:30", "19:15" } },
                new Movie { Title = "Deadpool & Wolverine", Genre = "Action / Comedy", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#8E44AD"), Showtimes = new ObservableCollection<string> { "10:15", "18:20", "21:30" } },
                new Movie { Title = "Kung Fu Panda 4", Genre = "Animation / Family", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#16A085"), Showtimes = new ObservableCollection<string> { "09:30", "14:00" } },
                new Movie { Title = "The Fall Guy", Genre = "Action / Comedy", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#D35400"), Showtimes = new ObservableCollection<string> { "16:00", "18:45", "22:00" } }
            };

            // 3. VÁRHATÓ FILMEK
            ComingSoonMovies = new ObservableCollection<Movie>
            {
                new Movie { Title = "Joker: Folie à Deux", Genre = "Drama / Thriller", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#7B241C"), Showtimes = new ObservableCollection<string> { "COMING OCT" } },
                new Movie { Title = "Gladiator II", Genre = "Action / Adventure", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#784212"), Showtimes = new ObservableCollection<string> { "COMING NOV" } },
                new Movie { Title = "Moana 2", Genre = "Animation / Adventure", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#1A5276"), Showtimes = new ObservableCollection<string> { "COMING NOV" } }
            };

            // Kiemelt film (Hero Section)
            FeaturedMovie = Movies[0];

            ApplyTestRole("Guest");

            this.DataContext = this;
        }

        // --- ESEMÉNYKEZELŐK ---

        public void ApplyTestRole(string role)
        {
            // 1. Alaphelyzet: Minden speciális fül rejtve, Login gomb látszik
            TicketsTab.Visibility = Visibility.Collapsed;
            ProfileTab.Visibility = Visibility.Collapsed;
            CashierTab.Visibility = Visibility.Collapsed;
            AdminTab.Visibility = Visibility.Collapsed;

            // Alapértelmezett gombállapot
            LoginBtn.Visibility = Visibility.Visible;
            LogoutBtn.Visibility = Visibility.Collapsed;

            // 2. Ha NEM vendég (Guest), akkor elrejtjük a Logint és mutatjuk a Logoutot
            if (role != "Guest" && !string.IsNullOrEmpty(role))
            {
                LoginBtn.Visibility = Visibility.Collapsed;
                LogoutBtn.Visibility = Visibility.Visible;
            }

            // 3. Szerepkör specifikus fülek megjelenítése
            switch (role)
            {
                case "User":
                    TicketsTab.Visibility = Visibility.Visible;
                    ProfileTab.Visibility = Visibility.Visible;
                    break;
                case "Cashier":
                    CashierTab.Visibility = Visibility.Visible; // Pénztáros funkciók
                    ProfileTab.Visibility = Visibility.Visible;
                    break;
                case "Admin":
                    AdminTab.Visibility = Visibility.Visible;   // Adminisztrátori kezelőfelület
                    ProfileTab.Visibility = Visibility.Visible;
                    break;
            }
        }
        //Felső szekció tabjai (Home / Coming Soon):
        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (MoviesItemsControl == null) return;

            if (sender is RadioButton rb)
            {
                MoviesItemsControl.ItemsSource = (rb.Name == "HomeTab") ? Movies : ComingSoonMovies;

                var listAnim = (Storyboard)Resources["ListRefreshAnimation"];
                listAnim?.Begin();

                MoviesScrollViewer?.ScrollToHome();
            }
        }
        //Dátumválasztó gombok:
        private void DateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.DataContext is DateItem selectedDate)
            {
                foreach (var date in AvailableDates) date.IsSelected = false;
                selectedDate.IsSelected = true;

                var listAnim = (Storyboard)Resources["ListRefreshAnimation"];
                listAnim.Begin();

                // Itt töltheted be az új adatokat a 'Movies' listába az adott naphoz
                // Pl: RefreshMoviesForDate(selectedDate.DateNumber);
            }
        }
        //Filmkártyák eseménykezelői:
        private void MovieCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is Movie m) UpdateAppBackground(m.PlaceholderColor);
        }

        private void MovieCard_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateAppBackground((Color)ColorConverter.ConvertFromString("#2D313A"));
        }

        private void UpdateAppBackground(Color target)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // Az eredeti Splash-stílusú sötétszürke (#20232A)
                var anim = new ColorAnimation();
                anim.To = (Color)ColorConverter.ConvertFromString("#20232A");
                anim.Duration = TimeSpan.FromMilliseconds(500); // Finom átmenet
                mainWindow.BGGradientStop.BeginAnimation(GradientStop.ColorProperty, anim);
            }
        }
        //GOMBOK:
        //Jegyvásárlás adott filmre és időpontra:
        private void Showtime_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new TicketBuy());
        //Login oldalra dob át:
        private void Login_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new LoginPage());
        //Ha beléptél, kapsz egy kedves üzenetet:
        public void WelcomeUser(string username)
        {
            this.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(150);

                ShowToast($"Welcome back, {username}!", true);

            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }
        //Logout gomb, kidob simán a főoldalra:
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            TicketsTab.Visibility = Visibility.Collapsed;
            ProfileTab.Visibility = Visibility.Collapsed;
            CashierTab.Visibility = Visibility.Collapsed;
            AdminTab.Visibility = Visibility.Collapsed;

            LogoutBtn.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;

            HomeTab.IsChecked = true;

            ShowToast($"You have logged out succesfully!", true);

        }
        //Scrollolás a filmkártyák között:
        private void ScrollLeft_Click(object sender, RoutedEventArgs e) => MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset - 350);
        private void ScrollRight_Click(object sender, RoutedEventArgs e) => MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset + 350);

        private void MoviesScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
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

        //Hibás vagy éppen sikeres üzenet felül:
        private async void ShowToast(string message, bool isSuccess)
        {
            _toastGeneration++;
            int currentGen = _toastGeneration;

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

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            var vanishAnim = new DoubleAnimation(pathLen, TimeSpan.FromSeconds(5));
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);

            await Task.Delay(5000);

            if (_toastGeneration != currentGen)
                return;

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4));
            fadeOut.Completed += (s, e) => {
                ErrorToast.Visibility = Visibility.Collapsed;
            };
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    // --- ADATMODELLEK ---

    public class Movie
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public Color PlaceholderColor { get; set; }
        public ObservableCollection<string> Showtimes { get; set; }
    }

    public class DateItem : System.ComponentModel.INotifyPropertyChanged
    {
        public string DayName { get; set; }
        public string DateNumber { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}