using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services; // Adatszerviz importálása

namespace ticketmasterwpf
{
    public partial class HomePage : Page, INotifyPropertyChanged
    {
        // Adatgyűjtemények a felülethez
        public ObservableCollection<Screening> AllScreeningsForAdmin { get; set; } = new ObservableCollection<Screening>();
        public ObservableCollection<Movie> AllMoviesForAdmin { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> PagedAdminMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Screening> PagedScreenings { get; set; } = new ObservableCollection<Screening>();
        public ObservableCollection<Movie> Movies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> SelectableMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<PageItem> MoviePageNumbers { get; set; } = new ObservableCollection<PageItem>();
        public ObservableCollection<PageItem> ScreeningPageNumbers { get; set; } = new ObservableCollection<PageItem>();
        public ObservableCollection<Movie> ComingSoonMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<DateItem> AvailableDates { get; set; } = new ObservableCollection<DateItem>();
        public ObservableCollection<object> Customers { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<object> AllTickets { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<object> UserTickets { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<int> PageNumbers { get; set; } = new ObservableCollection<int>();

        private string _currentSearch = "";
        private string _currentSort = "ID_DESC";
        private DateTime _selectedDate = DateTime.Today;

        // --- ROTÁCIÓS VÁLTOZÓK ---
        private List<Movie> _featuredMoviesList = new List<Movie>();
        private int _currentFeaturedIndex = 0;
        private DispatcherTimer _rotationTimer;

        // Változók a szűréshez
        private string _screeningSearch = "";
        private string _screeningSort = "Date (Newest)";

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                UpdateHomeCatalogByDate(_selectedDate);
            }
        }

        private Movie _featuredMovie;
        public Movie FeaturedMovie
        {
            get => _featuredMovie ?? new Movie { Title = "Loading...", Genre = "-", ImdbRating = "0.0" };
            set
            {
                _featuredMovie = value;
                OnPropertyChanged();
            }
        }

        private Movie _selectedMovie;
        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set { _selectedMovie = value; OnPropertyChanged(); }
        }

        private const int itemsPerPage = 8;

        // Pagination változók
        private int _moviePage = 1;
        private int _screeningPage = 1;

        private string _moviePaginationStatus;
        public string MoviePaginationStatus
        {
            get => _moviePaginationStatus;
            set { _moviePaginationStatus = value; OnPropertyChanged(); }
        }

        private string _screeningPaginationStatus;
        public string ScreeningPaginationStatus
        {
            get => _screeningPaginationStatus;
            set { _screeningPaginationStatus = value; OnPropertyChanged(); }
        }

        private object _itemToDelete;

        public HomePage()
        {
            InitializeComponent();

            MovieDetailPopup.ShowToastRequested += (s, message) =>
            {
                AppToast.ShowToast(message, true);
            };
            AddMoviePopup.ShowToastRequested += (message, isSuccess) =>
            {
                AppToast.ShowToast(message, isSuccess);
            };
            AddScreeningPopup.ShowToastRequested += (message, isSuccess) =>
            {
                AppToast.ShowToast(message, isSuccess);
            };

            for (int i = 0; i < 7; i++)
            {
                DateTime d = DateTime.Today.AddDays(i);
                AvailableDates.Add(new DateItem
                {
                    DayName = i == 0 ? "TODAY" : i == 1 ? "TOMORROW" : d.ToString("dddd", new CultureInfo("en-US")).ToUpper(),
                    DateNumber = d.ToString("dd MMM"),
                    FullDate = d,
                    IsSelected = i == 0
                });
            }

            // --- IDŐZÍTŐ INICIALIZÁLÁSA (5 mp) ---
            _rotationTimer = new DispatcherTimer();
            _rotationTimer.Interval = TimeSpan.FromSeconds(5);
            _rotationTimer.Tick += RotationTimer_Tick;

            ApplyTestRole("Guest");
            this.DataContext = this;

            // 1. ÁTVESSZÜK AZ ADATOKAT A DATASERVICE-BŐL
            AllMoviesForAdmin = DataService.AllMovies;
            AllScreeningsForAdmin = DataService.AllScreenings;

            // 2. KÜLÖNLEGES LISTÁK FELTÖLTÉSE (Kiemelt, Selectable, Coming Soon)
            InitializeSpecialLists();

            // 3. UI FRISSÍTÉSE
            RefreshAdminPage();
            RefreshScreeningsPage();
            UpdateHomeCatalogByDate(DateTime.Today);
        }

        private void InitializeSpecialLists()
        {
            if (AllMoviesForAdmin == null || !AllMoviesForAdmin.Any()) return;

            SelectableMovies.Clear();
            ComingSoonMovies.Clear();

            var activeOrUpcomingMovies = AllMoviesForAdmin.Where(m => m.Status != "Not Active").ToList();

            foreach (var movie in AllMoviesForAdmin)
            {
                if (movie.Status != "Not Active")
                    SelectableMovies.Add(movie);

                if (string.Equals(movie.Status, "Upcoming", StringComparison.OrdinalIgnoreCase))
                    ComingSoonMovies.Add(movie);
            }

            _featuredMoviesList = activeOrUpcomingMovies
                .Where(m => double.TryParse(m.ImdbRating, NumberStyles.Any, CultureInfo.InvariantCulture, out var rating) && rating > 0)
                .OrderByDescending(m => m.ReleaseDate.Year)
                .ThenByDescending(m => double.TryParse(m.ImdbRating, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : 0)
                .Take(3)
                .ToList();

            if (!_featuredMoviesList.Any() && activeOrUpcomingMovies.Any())
            {
                _featuredMoviesList = activeOrUpcomingMovies.OrderByDescending(m => m.ReleaseDate).Take(3).ToList();
            }

            if (_featuredMoviesList.Any())
            {
                _currentFeaturedIndex = 0;
                FeaturedMovie = _featuredMoviesList[0];
                UpdateDots(0);

                if (_featuredMoviesList.Count > 1) _rotationTimer.Start();
                else _rotationTimer.Stop();
            }
        }

        // ================= ROTÁCIÓS LOGIKA =================

        private void RotationTimer_Tick(object sender, EventArgs e) => MoveToNextFeatured();

        private async void MoveToNextFeatured(int targetIndex = -1)
        {
            if (_featuredMoviesList == null || _featuredMoviesList.Count <= 1) return;

            var sb = (Storyboard)this.Resources["BannerFadeStoryboard"];
            sb.Begin();

            await Task.Delay(300);

            if (targetIndex == -1)
                _currentFeaturedIndex = (_currentFeaturedIndex + 1) % _featuredMoviesList.Count;
            else
                _currentFeaturedIndex = targetIndex;

            FeaturedMovie = _featuredMoviesList[_currentFeaturedIndex];
            UpdateDots(_currentFeaturedIndex);
        }

        private void Dot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && int.TryParse(rb.Tag.ToString(), out int index))
            {
                if (index == _currentFeaturedIndex || index >= _featuredMoviesList.Count) return;

                _rotationTimer.Stop();
                MoveToNextFeatured(index);
                _rotationTimer.Start();
            }
        }

        private void UpdateDots(int activeIndex)
        {
            if (BannerDotsContainer == null) return;
            for (int i = 0; i < BannerDotsContainer.Children.Count; i++)
            {
                if (BannerDotsContainer.Children[i] is RadioButton rb)
                {
                    rb.Visibility = (i < _featuredMoviesList.Count) ? Visibility.Visible : Visibility.Collapsed;
                    if (i == activeIndex) rb.IsChecked = true;
                }
            }
        }

        // ================= API HÍVÁSOK (CSAK MENTÉS/TÖRLÉS) =================

        private async void AddMoviePopup_OnMovieSaved(object sender, Movie e)
        {
            string apiUrl = "http://localhost:5035/api/Movies";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = JsonSerializer.Serialize(e);
                    StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response = (e.Id == 0)
                        ? await client.PostAsync(apiUrl, content)
                        : await client.PutAsync($"{apiUrl}/{e.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        AppToast.ShowToast("Movie saved successfully!", true);

                        // ÚJ: A DataService hívása a törölt helyett
                        await DataService.FetchMovies();

                        InitializeSpecialLists();
                        RefreshAdminPage();
                        UpdateHomeCatalogByDate(_selectedDate);
                    }
                }
                catch { AppToast.ShowToast("Error occurred while saving!", false); }
            }
        }

        private async void DeletePopup_OnDeleteConfirmed(object sender, object itemToDelete)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    if (itemToDelete is Movie movie)
                    {
                        var linkedScreenings = AllScreeningsForAdmin.Where(s => s.MovieId == movie.Id).ToList();

                        if (linkedScreenings.Any())
                        {
                            AppToast.ShowToast($"Cannot delete! {linkedScreenings.Count} screenings are still linked to this movie.", false);
                            return;
                        }

                        var response = await client.DeleteAsync($"http://localhost:5035/api/Movies/{movie.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            AppToast.ShowToast("Movie successfully deleted!", true);
                            await DataService.FetchMovies();

                            InitializeSpecialLists();
                            RefreshAdminPage();
                            UpdateHomeCatalogByDate(_selectedDate);
                        }
                    }
                    else if (itemToDelete is Screening screening)
                    {
                        var response = await client.DeleteAsync($"http://localhost:5035/api/Screenings/{screening.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            AppToast.ShowToast("Screening successfully deleted!", true);
                            await DataService.FetchScreenings();
                            await DataService.FetchMovies();

                            RefreshScreeningsPage();
                            UpdateHomeCatalogByDate(_selectedDate);
                        }
                    }
                }
                catch (Exception ex) { AppToast.ShowToast("Error: " + ex.Message, false); }
            }
        }

        private async void AddScreeningPopup_ScreeningSaved(object sender, EventArgs e)
        {
            AppToast.ShowToast("Screening successfully added!", true);

            await DataService.FetchScreenings();
            await DataService.FetchMovies();

            RefreshScreeningsPage();
            UpdateHomeCatalogByDate(_selectedDate);
        }

        private async void AddMoviePopup_RefreshRequested(object sender, EventArgs e)
        {
            await DataService.FetchMovies();
            InitializeSpecialLists();
            RefreshAdminPage();
            UpdateHomeCatalogByDate(_selectedDate);
        }

        // ================= UI LOGIKA & NAVIGÁCIÓ =================

        public void UpdateHomeCatalogByDate(DateTime targetDate)
        {
            Movies.Clear();
            _selectedDate = targetDate;

            foreach (var movie in AllMoviesForAdmin)
            {
                if (string.Equals(movie.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(movie.Status, "Upcoming", StringComparison.OrdinalIgnoreCase))
                {
                    var dailyScreenings = movie.Screenings != null
                        ? movie.Screenings.Where(s => s.StartTime.Date == targetDate.Date).OrderBy(s => s.StartTime).ToList()
                        : new List<Screening>();

                    if (dailyScreenings.Any())
                    {
                        if (movie.Showtimes == null) movie.Showtimes = new ObservableCollection<string>();
                        movie.Showtimes.Clear();

                        foreach (var s in dailyScreenings)
                        {
                            movie.Showtimes.Add(s.StartTime.ToString("HH:mm"));
                        }
                        Movies.Add(movie);
                    }
                }
            }

            if (HomeTab.IsChecked == true)
            {
                MovieCatalog?.SetMode(true, Movies);
            }
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (MovieCatalog == null) return;

            if (sender is RadioButton rb)
            {
                if (rb.Name == "HomeTab")
                {
                    MovieCatalog.Visibility = Visibility.Visible;
                    MovieCatalog.SetMode(true, Movies);
                }
                else if (rb.Name == "SoonTab")
                {
                    MovieCatalog.Visibility = Visibility.Visible;
                    MovieCatalog.SetMode(false, ComingSoonMovies);
                }
                else
                {
                    MovieCatalog.Visibility = Visibility.Collapsed;
                }
            }
        }

        // ================= ADMIN PANEL ESEMÉNYEK =================

        private void AdminPanel_AddMovieRequested(object sender, EventArgs e) => AddMoviePopup.OpenModal(null);
        private void AdminPanel_EditMovieRequested(object sender, Movie movie) => AddMoviePopup.OpenModal(movie);
        private void AdminPanel_DeleteMovieRequested(object sender, Movie movie) { _itemToDelete = movie; DeletePopup.OpenModal(movie); }
        private void AdminPanel_AddScreeningRequested(object sender, EventArgs e) => AddScreeningPopup.OpenModal();
        private void AdminPanel_EditScreeningRequested(object sender, Screening screening) => AddScreeningPopup.OpenModal(screening);
        private void AdminPanel_DeleteScreeningRequested(object sender, Screening screening) { _itemToDelete = screening; DeletePopup.OpenModal(screening); }

        private void AdminPanel_PageNumberRequested(object sender, int page)
        {
            if (AdminTab.IsChecked == true)
            {
                // Attól függően állítjuk a lapot, hogy melyik táblázatot nézzük
                if (AdminPanel.SubMovies.IsChecked == true) { _moviePage = page; RefreshAdminPage(); }
                else if (AdminPanel.SubScreenings.IsChecked == true) { _screeningPage = page; RefreshScreeningsPage(); }
            }
        }

        private void AdminPanel_NextPageRequested(object sender, EventArgs e)
        {
            if (AdminPanel.SubMovies.IsChecked == true)
            {
                int maxPage = (int)Math.Ceiling((double)AllMoviesForAdmin.Count / itemsPerPage);
                if (_moviePage < maxPage) { _moviePage++; RefreshAdminPage(); }
            }
            else if (AdminPanel.SubScreenings.IsChecked == true)
            {
                int maxPage = (int)Math.Ceiling((double)AllScreeningsForAdmin.Count / itemsPerPage);
                if (_screeningPage < maxPage) { _screeningPage++; RefreshScreeningsPage(); }
            }
        }

        private void AdminPanel_PrevPageRequested(object sender, EventArgs e)
        {
            if (AdminPanel.SubMovies.IsChecked == true && _moviePage > 1)
            {
                _moviePage--; RefreshAdminPage();
            }
            else if (AdminPanel.SubScreenings.IsChecked == true && _screeningPage > 1)
            {
                _screeningPage--; RefreshScreeningsPage();
            }
        }

        private void AdminPanel_SearchChanged(object sender, string searchText)
        {
            _currentSearch = searchText;
            _moviePage = 1;
            RefreshAdminPage();
        }

        private void AdminPanel_SortChanged(object sender, string sortTag)
        {
            _currentSort = sortTag;
            RefreshAdminPage();
        }

        private void AdminPanel_ScreeningSearchChanged(object sender, string searchText)
        {
            _screeningSearch = searchText;
            _screeningPage = 1;
            RefreshScreeningsPage();
        }

        private void AdminPanel_ScreeningSortChanged(object sender, string sortTag)
        {
            _screeningSort = sortTag;
            RefreshScreeningsPage();
        }

        private void RefreshAdminPage()
        {
            if (AllMoviesForAdmin == null) return;

            var query = AllMoviesForAdmin.Where(m =>
                string.IsNullOrEmpty(_currentSearch) ||
                m.Title.ToLower().Contains(_currentSearch.ToLower())
            );

            switch (_currentSort)
            {
                case "ID_ASC": query = query.OrderBy(m => m.Id); break;
                case "STATUS": query = query.OrderBy(m => m.Status); break;
                case "TITLE_ASC": query = query.OrderBy(m => m.Title); break;
                case "NEWEST":
                default: query = query.OrderByDescending(m => m.Id); break;
            }

            var filteredList = query.ToList();
            var pagedData = filteredList.Skip((_moviePage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            PagedAdminMovies.Clear();
            foreach (var movie in pagedData) PagedAdminMovies.Add(movie);

            UpdateMoviePagination(filteredList.Count);
        }

        private void RefreshScreeningsPage()
        {
            if (AllScreeningsForAdmin == null) return;

            var query = AllScreeningsForAdmin.AsQueryable();

            if (!string.IsNullOrEmpty(_screeningSearch) && _screeningSearch != "Search by movie title...")
            {
                query = query.Where(s => s.Movie != null && s.Movie.Title.ToLower().Contains(_screeningSearch.ToLower()));
            }

            switch (_screeningSort)
            {
                case "Movie A-Z": query = query.OrderBy(s => s.Movie != null ? s.Movie.Title : ""); break;
                case "Price": query = query.OrderBy(s => s.Price); break;
                case "Date (Newest)":
                default: query = query.OrderByDescending(s => s.StartTime); break;
            }

            var filteredList = query.ToList();
            var pagedData = filteredList.Skip((_screeningPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            PagedScreenings.Clear();
            foreach (var screening in pagedData) PagedScreenings.Add(screening);

            UpdateScreeningPagination(filteredList.Count);
        }

        // ================= MOVIE LIST & CASHIER ESEMÉNYEK =================

        private void MovieCatalog_MovieDetailRequested(object sender, Movie selectedMovie) => MovieDetailPopup.OpenModal(selectedMovie);
        private void MovieCatalog_DateChanged(object sender, DateTime newDate) => SelectedDate = newDate;
        private void CalendarPopup_DateSelected(object sender, DateTime newDate) => SelectedDate = newDate;

        private void CashierPanel_VerifyTicketRequested(object sender, string ticketId)
        {
            if (ticketId == "12345" || ticketId == "0000")
                CashierPanel.ShowValidationResult(true, "Dune: Part Two - 19:30\nRoom 1 | Seat: Row 4, Seat 12");
            else
                CashierPanel.ShowValidationResult(false, $"No ticket found in database with ID: {ticketId}");
        }

        private void CashierPanel_IssueAllTicketsRequested(object sender, ObservableCollection<Views.OrderItem> orderItems)
        {
            int totalTickets = orderItems.Count;
            AppToast.ShowToast($"Transaction successful! {totalTickets} tickets issued.", true);
        }

        // ================= USER & SYSTEM =================

        public void ApplyTestRole(string role)
        {
            ProfileTab.Visibility = CashierTab.Visibility = AdminTab.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;
            LoggedInPanel.Visibility = Visibility.Collapsed;

            if (role != "Guest" && !string.IsNullOrEmpty(role))
            {
                LoginBtn.Visibility = Visibility.Collapsed;
                LoggedInPanel.Visibility = Visibility.Visible;
            }

            switch (role)
            {
                case "User": ProfileTab.Visibility = Visibility.Visible; TopUserNameTxt.Text = "John Doe"; break;
                case "Cashier": CashierTab.Visibility = ProfileTab.Visibility = Visibility.Visible; TopUserNameTxt.Text = "Cashier"; break;
                case "Admin": AdminTab.Visibility = ProfileTab.Visibility = Visibility.Visible; TopUserNameTxt.Text = "Admin"; break;
            }
        }

        public void WelcomeUser(string username)
        {
            this.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(500);
                AppToast.ShowToast($"Welcome back, {username}!", true);
            });
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ApplyTestRole("Guest");
            HomeTab.IsChecked = true;
            AppToast.ShowToast("Successfully logged out!", true);
        }

        private void Login_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new LoginPage());

        private void UpdateMoviePagination(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

            // --- CSÚSZÓABLAK: Max 3 oldalszám ---
            int maxPagesToShow = 3;
            int startPage = Math.Max(1, _moviePage - 1);
            int endPage = startPage + maxPagesToShow - 1;

            if (endPage > totalPages)
            {
                endPage = totalPages;
                startPage = Math.Max(1, endPage - maxPagesToShow + 1);
            }

            MoviePageNumbers.Clear();
            for (int i = startPage; i <= endPage; i++)
                MoviePageNumbers.Add(new PageItem { Number = i, IsActive = (i == _moviePage) });

            int startItem = totalItems == 0 ? 0 : ((_moviePage - 1) * itemsPerPage) + 1;
            int endItem = Math.Min(_moviePage * itemsPerPage, totalItems);
            MoviePaginationStatus = $"Showing movies {startItem} to {endItem} of {totalItems} entries";
        }

        private void UpdateScreeningPagination(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

            // --- CSÚSZÓABLAK: Max 3 oldalszám ---
            int maxPagesToShow = 3;
            int startPage = Math.Max(1, _screeningPage - 1);
            int endPage = startPage + maxPagesToShow - 1;

            if (endPage > totalPages)
            {
                endPage = totalPages;
                startPage = Math.Max(1, endPage - maxPagesToShow + 1);
            }

            ScreeningPageNumbers.Clear();
            for (int i = startPage; i <= endPage; i++)
                ScreeningPageNumbers.Add(new PageItem { Number = i, IsActive = (i == _screeningPage) });

            int startItem = totalItems == 0 ? 0 : ((_screeningPage - 1) * itemsPerPage) + 1;
            int endItem = Math.Min(_screeningPage * itemsPerPage, totalItems);
            ScreeningPaginationStatus = $"Showing screenings {startItem} to {endItem} of {totalItems} entries";
        }

        // ================= PROPERTY CHANGED =================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}