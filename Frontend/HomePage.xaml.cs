using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; // Szükséges az INotifyPropertyChanged-hez
using System.Linq;
using System.Runtime.CompilerServices;
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
    // A felület most már figyeli a változásokat (INotifyPropertyChanged)
    public partial class HomePage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public ObservableCollection<Movie> ComingSoonMovies { get; set; }
        public ObservableCollection<DateItem> AvailableDates { get; set; }
        public Movie FeaturedMovie { get; set; }

        // Ez a tulajdonság tárolja az Admin felületen éppen kijelölt filmet
        private Movie _selectedMovie;
        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged(); // Jelzi a XAML-nek, hogy frissítse a szerkesztőt
            }
        }

        public enum UserRole { Guest, User, Cashier, Admin }
        private int _toastGeneration = 0;

        public HomePage()
        {
            InitializeComponent();

            // 1. DÁTUMOK GENERÁLÁSA
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
                new Movie { Title = "Dune: Part Two", Genre = "Sci-Fi / Action", Duration = "130", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#2C3E50"), Showtimes = new ObservableCollection<string> { "14:30", "17:45", "20:00" } },
                new Movie { Title = "Oppenheimer", Genre = "Biography / Drama", Duration = "130", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#34495E"), Showtimes = new ObservableCollection<string> { "13:00", "16:30", "19:15" } },
                new Movie { Title = "Deadpool & Wolverine", Genre = "Action / Comedy", Duration = "130", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#8E44AD"), Showtimes = new ObservableCollection<string> { "10:15", "18:20", "21:30" } },
                new Movie { Title = "Kung Fu Panda 4", Genre = "Animation / Family", Duration = "130", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#16A085"), Showtimes = new ObservableCollection<string> { "09:30", "14:00" } },
                new Movie { Title = "The Fall Guy", Genre = "Action / Comedy", Duration = "130", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#D35400"), Showtimes = new ObservableCollection<string> { "16:00", "18:45", "22:00" } }
            };

            // 3. VÁRHATÓ FILMEK
            ComingSoonMovies = new ObservableCollection<Movie>
            {
                new Movie { Title = "Joker: Folie à Deux", Genre = "Drama / Thriller", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#7B241C"), Showtimes = new ObservableCollection<string> { "COMING OCT" } },
                new Movie { Title = "Gladiator II", Genre = "Action / Adventure", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#784212"), Showtimes = new ObservableCollection<string> { "COMING NOV" } },
                new Movie { Title = "Moana 2", Genre = "Animation / Adventure", PlaceholderColor = (Color)ColorConverter.ConvertFromString("#1A5276"), Showtimes = new ObservableCollection<string> { "COMING NOV" } }
            };

            FeaturedMovie = Movies[0];

            // Kezdetben az első filmet jelöljük ki az Admin panelen is
            SelectedMovie = Movies[0];

            ApplyTestRole("Guest");
            this.DataContext = this;
        }

        // --- ADMIN DASHBOARD ESEMÉNYEK ---

        // Amikor az Admin rákattint egy sorra a táblázatban
        private void AdminMoviesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Movie selected)
            {
                SelectedMovie = selected;
            }
        }

        private int currentPage = 1;
        private const int itemsPerPage = 8;
        public void UpdatePagination(int totalMovies)
        {
            int totalPages = (int)Math.Ceiling((double)totalMovies / itemsPerPage);

            PageNumbers.Clear();
            for (int i = 1; i <= totalPages; i++)
            {
                PageNumbers.Add(i);
            }
        }

        // LAPOZÓ LOGIKA
        private void PageNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Content != null)
            {
                currentPage = int.Parse(rb.Content.ToString());
                UpdateMovieDisplay();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < PageNumbers.Count)
            {
                currentPage++;
                SyncPaginationSelection();
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                SyncPaginationSelection();
            }
        }

        private void SyncPaginationSelection()
        {
            UpdateMovieDisplay();
        }

        private void UpdateMovieDisplay()
        {
            ShowToast($"Page {currentPage} loaded", true);
        }

        // MENÜ ÉS SZERKESZTÉS MOVIE
        private void EditMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Movie movie)
            {
                MovieModalTitle.Text = "Edit Movie Details";
                SaveMovieBtn.Content = "Update Movie";

                MovieTitleInput.Text = movie.Title;
                MovieDurationInput.Text = movie.Duration;
                MovieGenreInput.Text = movie.Genre;
                MoviePosterInput.Text = "https://...";

                AddMovieModal.Visibility = Visibility.Visible;
            }
        }

        private Movie _movieToBeDeleted;
        private void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            DeleteConfirmModal.Visibility = Visibility.Visible;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_movieToBeDeleted != null)
            {
                Movies.Remove(_movieToBeDeleted);
                UpdatePagination(Movies.Count);
                ShowToast("Movie deleted successfully", true);

                _movieToBeDeleted = null;
            }
            DeleteConfirmModal.Visibility = Visibility.Collapsed;
        }
        private void CloseDeleteModal_Click(object sender, RoutedEventArgs e)
        {
            DeleteConfirmModal.Visibility = Visibility.Collapsed;
            _movieToBeDeleted = null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            AddMovieModal.Visibility = Visibility.Collapsed;
            ShowToast("Changes saved", true);
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            AddMovieModal.Visibility = Visibility.Collapsed;
        }

        private void OpenAddMovie_Click(object sender, RoutedEventArgs e)
        {
            MovieModalTitle.Text = "Add New Movie to Database";
            SaveMovieBtn.Content = "Save Movie";

            MovieTitleInput.Clear();
            MovieDurationInput.Clear();
            MovieGenreInput.SelectedIndex = -1;
            MoviePosterInput.Text = "https://...";

            AddMovieModal.Visibility = Visibility.Visible;
        }
        //SCREENINGS

        private void OpenAddScreening_Click(object sender, RoutedEventArgs e)
        {
            AddScreeningModal.Visibility = Visibility.Visible;
        }

        private void CloseScreeningModal_Click(object sender, RoutedEventArgs e)
        {
            AddScreeningModal.Visibility = Visibility.Collapsed;
        }

        private void SaveScreening_Click(object sender, RoutedEventArgs e)
        {

            AddScreeningModal.Visibility = Visibility.Collapsed;
            ShowToast("New screening scheduled successfully!", true);
        }
        // --- EREDETI ESEMÉNYKEZELŐK ---

        public void ApplyTestRole(string role)
        {
            ProfileTab.Visibility = Visibility.Collapsed;
            CashierTab.Visibility = Visibility.Collapsed;
            AdminTab.Visibility = Visibility.Collapsed;

            LoginBtn.Visibility = Visibility.Visible;
            LoggedInPanel.Visibility = Visibility.Collapsed;

            if (role != "Guest" && !string.IsNullOrEmpty(role))
            {
                LoginBtn.Visibility = Visibility.Collapsed;
                LoggedInPanel.Visibility = Visibility.Visible;
            }

            switch (role)
            {
                case "User":
                    ProfileTab.Visibility = Visibility.Visible;
                    TopUserNameTxt.Text = "John Doe";
                    break;

                case "Cashier":
                    CashierTab.Visibility = Visibility.Visible;
                    ProfileTab.Visibility = Visibility.Visible;
                    TopUserNameTxt.Text = "Cashier";
                    break;

                case "Admin":
                    AdminTab.Visibility = Visibility.Visible;
                    ProfileTab.Visibility = Visibility.Visible;
                    TopUserNameTxt.Text = "Admin";
                    break;
            }
        }

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

        private void DateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.DataContext is DateItem selectedDate)
            {
                foreach (var date in AvailableDates) date.IsSelected = false;
                selectedDate.IsSelected = true;
                var listAnim = (Storyboard)Resources["ListRefreshAnimation"];
                listAnim.Begin();
            }
        }

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
                var anim = new ColorAnimation();
                anim.To = (Color)ColorConverter.ConvertFromString("#20232A");
                anim.Duration = TimeSpan.FromMilliseconds(500);
                mainWindow.BGGradientStop.BeginAnimation(GradientStop.ColorProperty, anim);
            }
        }

        private void Showtime_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new TicketBuy());
        private void Login_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new LoginPage());

        public void WelcomeUser(string username)
        {
            this.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(150);
                ShowToast($"Welcome back, {username}!", true);
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ApplyTestRole("Guest");
            ProfileTab.Visibility = CashierTab.Visibility = AdminTab.Visibility = Visibility.Collapsed;
            LogoutBtn.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;
            HomeTab.IsChecked = true;
            ShowToast($"You have logged out succesfully!", true);
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e) => MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset - 350);
        private void ScrollRight_Click(object sender, RoutedEventArgs e) => MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset + 350);

        private void MoviesScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoviesScrollViewer.ScrollToHorizontalOffset(MoviesScrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null) window.WindowState = WindowState.Minimized;
        }

        //Hibás vagy éppen sikeres üzenet felül:
        private async void ShowToast(string message, bool isSuccess)
        {
            _toastGeneration++;
            int currentGen = _toastGeneration;

            if (isSuccess)
            {
                ErrorIconBack.Visibility = Visibility.Collapsed; ErrorIconText.Visibility = Visibility.Collapsed;
                SuccessIconBack.Visibility = Visibility.Visible; SuccessIconText.Visibility = Visibility.Visible;
                LeftTimerStroke.Stroke = RightTimerStroke.Stroke = Brushes.MediumSeaGreen;
                BgPathLeft.Stroke = BgPathRight.Stroke = new SolidColorBrush(Color.FromArgb(40, 60, 179, 113));
            }
            else
            {
                ErrorIconBack.Visibility = Visibility.Visible; ErrorIconText.Visibility = Visibility.Visible;
                SuccessIconBack.Visibility = Visibility.Collapsed; SuccessIconText.Visibility = Visibility.Collapsed;
                LeftTimerStroke.Stroke = RightTimerStroke.Stroke = Brushes.IndianRed;
                BgPathLeft.Stroke = BgPathRight.Stroke = new SolidColorBrush(Color.FromArgb(40, 205, 92, 92));
            }

            ErrorText.Text = message;
            ErrorToast.Opacity = 0;
            ErrorToast.Visibility = Visibility.Visible;
            ErrorToast.UpdateLayout();

            double w = ErrorToast.ActualWidth; double h = ErrorToast.ActualHeight;
            double halfW = w / 2; double r = 12;
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            string leftData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {1:0.##},{1:0.##} 0 0 0 0,{1:0.##} L 0,{2:0.##} A {1:0.##},{1:0.##} 0 0 0 {1:0.##},{3:0.##} L {0:0.##},{3:0.##}", halfW, r, h - r, h);
            string rightData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {2:0.##},{2:0.##} 0 0 1 {3:0.##},{2:0.##} L {3:0.##},{4:0.##} A {2:0.##},{2:0.##} 0 0 1 {1:0.##},{5:0.##} L {0:0.##},{5:0.##}", halfW, w - r, r, w, h - r, h);

            LeftTimerStroke.Data = Geometry.Parse(leftData); RightTimerStroke.Data = Geometry.Parse(rightData);
            BgPathLeft.Data = LeftTimerStroke.Data; BgPathRight.Data = RightTimerStroke.Data;

            double pathLen = halfW + h + halfW + 10;
            LeftTimerStroke.StrokeDashArray = RightTimerStroke.StrokeDashArray = new DoubleCollection { pathLen, pathLen };
            LeftTimerStroke.StrokeDashOffset = RightTimerStroke.StrokeDashOffset = 0;

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            var vanishAnim = new DoubleAnimation(pathLen, TimeSpan.FromSeconds(5));
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);

            await Task.Delay(5000);
            if (_toastGeneration != currentGen) return;

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4));
            fadeOut.Completed += (s, e) => { ErrorToast.Visibility = Visibility.Collapsed; };
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // --- ADATMODELLEK (MÓDOSÍTVA) ---

    // A Movie osztálynak is tudnia kell szólni a UI-nak, ha változik a címe
    public class Movie : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }
        private string _duration;
        public string Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(); }
        }

        public string Genre { get; set; }
        public Color PlaceholderColor { get; set; }
        public ObservableCollection<string> Showtimes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class DateItem : INotifyPropertyChanged
    {
        public string DayName { get; set; }
        public string DateNumber { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}