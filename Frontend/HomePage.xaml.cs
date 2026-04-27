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
        public ObservableCollection<CinemaHall> CinemaHalls { get; set; } = new ObservableCollection<CinemaHall>();
        public ObservableCollection<Screening> AllScreeningsForAdmin { get; set; } = new ObservableCollection<Screening>();
        public ObservableCollection<Movie> AllMoviesForAdmin { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> PagedAdminMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Screening> PagedScreenings { get; set; } = new ObservableCollection<Screening>();
        public ObservableCollection<Movie> Movies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> SelectableMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<PageItem> MoviePageNumbers { get; set; } = new ObservableCollection<PageItem>();
        public ObservableCollection<PageItem> ScreeningPageNumbers { get; set; } = new ObservableCollection<PageItem>();
        public ObservableCollection<PageItem> CustomerPageNumbers { get; set; } = new ObservableCollection<PageItem>();
        public ObservableCollection<Movie> ComingSoonMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<DateItem> AvailableDates { get; set; } = new ObservableCollection<DateItem>();
        public ObservableCollection<User> Customers { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<GroupedTicket> PagedTickets { get; set; } = new ObservableCollection<GroupedTicket>();
        public ObservableCollection<PageItem> TicketPageNumbers { get; set; } = new ObservableCollection<PageItem>();
        public ObservableCollection<int> PageNumbers { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<ChartBar> RevenueChartData { get; set; } = new ObservableCollection<ChartBar>();
        public ObservableCollection<TopMovieStat> TopMoviesList { get; set; } = new ObservableCollection<TopMovieStat>();

        public string TotalRevenue { get; set; } = "0 Ft";
        public string ActiveUserCount { get; set; } = "0";

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
        private string _ticketSearchText = "";

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
        private int _customerPage = 1;
        private int _ticketPage = 1;

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

        private string _ticketPaginationStatus;
        public string TicketPaginationStatus
        {
            get => _ticketPaginationStatus;
            set { _ticketPaginationStatus = value; OnPropertyChanged(); }
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
            AddUserPopup.ShowToastRequested += (message, isSuccess) =>
            {
                AppToast.ShowToast(message, isSuccess);
            };
            ProfilePanel.ShowToastRequested += (message, isSuccess) => {
                AppToast.ShowToast(message, isSuccess);
            };
            ProfilePanel.ProfileUpdated += (s, e) => {
                ApplyTestRole(DataService.CurrentUser);
            };
            CashierPanel.ShowToastRequested += (message, isSuccess) =>
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

            _rotationTimer = new DispatcherTimer();
            _rotationTimer.Interval = TimeSpan.FromSeconds(5);
            _rotationTimer.Tick += RotationTimer_Tick;

            this.DataContext = this;

            AllMoviesForAdmin = DataService.AllMovies;
            AllScreeningsForAdmin = DataService.AllScreenings;

            InitializeSpecialLists();

            this.Loaded += HomePage_Loaded;
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            await DataService.FetchMovies();
            await DataService.FetchScreenings();
            await DataService.FetchUsers();
            await DataService.FetchCinemaHalls();
            await DataService.FetchAllTickets();

            if (DataService.CurrentUser != null)
            {
                ApplyTestRole(DataService.CurrentUser);
            }
            else
            {
                ApplyTestRole(null);
            }

            InitializeSpecialLists();
            RefreshAdminPage();
            RefreshScreeningsPage();
            UpdateHomeCatalogByDate(DateTime.Today);
            RefreshUsersPage();
            RefreshTicketsPage();
            UpdateDashboardStats();
            ProfilePanel.FetchUserTickets();
        }

        private void InitializeSpecialLists()
        {
            if (AllMoviesForAdmin == null || !AllMoviesForAdmin.Any()) return;

            SelectableMovies.Clear();
            ComingSoonMovies.Clear();
            CinemaHalls.Clear();

            var activeOrUpcomingMovies = AllMoviesForAdmin.Where(m => m.Status != "Not Active").ToList();

            foreach (var hall in DataService.AllCinemaHalls)
            {
                CinemaHalls.Add(hall);
            }

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

        private void GetTickets_Click(object sender, RoutedEventArgs e)
        {
            if (_featuredMoviesList != null && _featuredMoviesList.Count > 0 &&
                _currentFeaturedIndex >= 0 && _currentFeaturedIndex < _featuredMoviesList.Count)
            {
                var currentFeatured = _featuredMoviesList[_currentFeaturedIndex];

                MovieDetailPopup.OpenModal(currentFeatured, _selectedDate);
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
                        try
                        {
                            var ticketsToCancel = DataService.AllTickets
                                .Where(t => t.ScreeningId == screening.Id && !t.IsCancelled)
                                 .ToList();

                            if (ticketsToCancel.Any())
                            {
                                foreach (var ticket in ticketsToCancel)
                                {
                                     var cancelRequest = new HttpRequestMessage(new HttpMethod("PATCH"), $"http://localhost:5035/api/Tickets/{ticket.Id}/cancel");
                                     await client.SendAsync(cancelRequest);
                                }

                                AppToast.ShowToast($"{ticketsToCancel.Count} tickets have been automatically cancelled.", true);
                            }

                            var response = await client.DeleteAsync($"http://localhost:5035/api/Screenings/{screening.Id}");

                            if (response.IsSuccessStatusCode)
                            {
                               AppToast.ShowToast("Screening successfully removed!", true);

                               await DataService.FetchScreenings();
                               await DataService.FetchAllTickets();

                               RefreshScreeningsPage();
                               RefreshTicketsPage();
                               UpdateHomeCatalogByDate(_selectedDate);
                               UpdateDashboardStats();
                            }
                            else
                            {
                               string error = await response.Content.ReadAsStringAsync();
                               AppToast.ShowToast($"Error deleting screening: {error}", false);
                            }
                        }
                        catch (Exception ex)
                        {
                            AppToast.ShowToast("Error: " + ex.Message, false);
                        }
                    }
                    else if (itemToDelete is User user)
                    {
                        if (DataService.CurrentUser != null && user.Id == DataService.CurrentUser.Id)
                        {
                            AppToast.ShowToast("You cannot delete your own account!", false);
                            return;
                        }

                        var response = await client.DeleteAsync($"http://localhost:5035/api/Users/{user.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            AppToast.ShowToast($"{user.Username} successfully deleted!", true);

                            await DataService.FetchUsers();

                            RefreshUsersPage();
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            AppToast.ShowToast($"Delete failed: {error}", false);
                        }
                    }
                    else if (itemToDelete is GroupedTicket gt || itemToDelete is Views.ProfileView.GroupedTicket)
                    {
                        try
                        {
                            var ticketsToCancel = (itemToDelete is GroupedTicket adminGt)
                                ? adminGt.AllTicketsInGroup
                                : ((Views.ProfileView.GroupedTicket)itemToDelete).AllTicketsInGroup;

                            foreach (var ticket in ticketsToCancel)
                            {
                                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"http://localhost:5035/api/Tickets/{ticket.Id}/cancel");
                                var response = await client.SendAsync(request);

                                if (!response.IsSuccessStatusCode)
                                {
                                    string err = await response.Content.ReadAsStringAsync();
                                    throw new Exception($"API Error: {err}");
                                }
                                ticket.IsCancelled = true;
                            }

                            AppToast.ShowToast("Tickets successfully cancelled!", true);

                            await DataService.FetchAllTickets();
                            RefreshTicketsPage();
                            ProfilePanel.FetchUserTickets();
                            UpdateDashboardStats();
                        }
                        catch (Exception ex)
                        {
                            AppToast.ShowToast("Cancellation failed: " + ex.Message, false);
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
                        ? movie.Screenings.Where(s => s.StartTime.Date == targetDate.Date && s.StartTime > DateTime.Now).OrderBy(s => s.StartTime).ToList()
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

        // ================= DASHBOARD STATS FRISSÍTÉSE =================

        private void UpdateDashboardStats()
        {
            if (DataService.AllTickets == null) return;

            var validTickets = DataService.AllTickets.Where(t => !t.IsCancelled).ToList();
            var totalRev = validTickets.Sum(t => t.Price);

            TotalRevenue = $"{totalRev:N0} Ft";
            ActiveUserCount = DataService.AllUsers.Count.ToString();

            var topMovies = validTickets
                .GroupBy(t => t.MovieTitle)
                .Select(g => new TopMovieStat { Title = g.Key, Sales = g.Count() })
                .OrderByDescending(x => x.Sales).Take(3).ToList();

            TopMoviesList.Clear();
            for (int i = 0; i < topMovies.Count; i++)
            {
                topMovies[i].Rank = i + 1;
                TopMoviesList.Add(topMovies[i]);
            }

            RevenueChartData.Clear();
            DateTime today = DateTime.Today;
            double maxDaily = 0;
            var last7Days = new Dictionary<DateTime, double>();

            for (int i = 6; i >= 0; i--)
            {
                DateTime d = today.AddDays(-i);
                double dayRev = validTickets.Where(t => t.PurchaseDate.Date == d.Date).Sum(t => t.Price);
                last7Days[d] = dayRev;
                if (dayRev > maxDaily) maxDaily = dayRev;
            }

            foreach (var day in last7Days)
            {
                double height = maxDaily > 0 ? (day.Value / maxDaily) * 140 : 0;
                RevenueChartData.Add(new ChartBar
                {
                    Day = day.Key.ToString("ddd"),
                    Value = Math.Max(height, 5),
                    Label = day.Value >= 1000 ? $"{day.Value / 1000.0:N1}k" : day.Value.ToString()
                });
            }

            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(ActiveUserCount));
        }

        // USER

        private void ProfilePanel_CancelTicketRequested(object sender, Views.ProfileView.GroupedTicket groupedTicket)
        {
            if (groupedTicket.MainTicket.IsValidated)
            {
                AppToast.ShowToast("You cannot cancel a ticket that has already been validated!", false);
                return;
            }

            var screening = DataService.AllScreenings.FirstOrDefault(s => s.Id == groupedTicket.MainTicket.ScreeningId);

            if (screening != null)
            {
                double hoursUntilShow = (screening.StartTime - DateTime.Now).TotalHours;

                if (hoursUntilShow < 4)
                {
                    AppToast.ShowToast("Cancellations are only allowed up to 4 hours before the show!", false);
                    return;
                }
            }

            // Ha minden ellenőrzésen átment, megnyitjuk a törlési ablakot
            _itemToDelete = groupedTicket;
            DeletePopup.OpenModal(groupedTicket);
        }
        private void ProfilePanel_ViewDigitalPassRequested(object sender, Ticket ticket)
        {
            string formattedTicketId = $"TID-{ticket.PurchaseDate:yyyyMMddHHmm}-{ticket.Id}";

            var ticketModal = new ticketmasterwpf.Modals.DigitalTicketModal(
                ticket.MovieTitle,
                ticket.SessionTime,
                ticket.RoomName,
                ticket.SeatInfo,
                formattedTicketId,
                ticket.LinkedScreening?.Movie?.PosterUrl,
                $"{ticket.Price} Ft",
                false
            );

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowModal(ticketModal);
        }

        // ================= ADMIN PANEL ESEMÉNYEK =================

        private void AdminPanel_AddMovieRequested(object sender, EventArgs e) => AddMoviePopup.OpenModal(null);
        private void AdminPanel_EditMovieRequested(object sender, Movie movie) => AddMoviePopup.OpenModal(movie);
        private void AdminPanel_DeleteMovieRequested(object sender, Movie movie) { _itemToDelete = movie; DeletePopup.OpenModal(movie); }
        private void AdminPanel_AddScreeningRequested(object sender, EventArgs e) => AddScreeningPopup.OpenModal();
        private void AdminPanel_EditScreeningRequested(object sender, Screening screening) => AddScreeningPopup.OpenModal(screening);
        private void AdminPanel_DeleteScreeningRequested(object sender, Screening screening) { _itemToDelete = screening; DeletePopup.OpenModal(screening); }
        private void AdminPanel_AddCustomerRequested(object sender, EventArgs e) => AddUserPopup.OpenModal(null);
        private void AdminPanel_EditCustomerRequested(object sender, User user) => AddUserPopup.OpenModal(user);
        private void AdminPanel_DeleteCustomerRequested(object sender, User user) { _itemToDelete = user; DeletePopup.OpenModal(user); }
        private void AdminPanel_RefundTicketRequested(object sender, GroupedTicket groupedTicket)
        {
            if (groupedTicket.MainTicket.IsValidated)
            {
                AppToast.ShowToast("Cannot refund a ticket that has already been validated!", false);
                return;
            }

            _itemToDelete = groupedTicket;
            DeletePopup.OpenModal(groupedTicket);
        }

        private void AdminPanel_PageNumberRequested(object sender, int page)
        {
            if (AdminTab.IsChecked == true)
            {
                if (AdminPanel.SubMovies.IsChecked == true) { _moviePage = page; RefreshAdminPage(); }
                else if (AdminPanel.SubScreenings.IsChecked == true) { _screeningPage = page; RefreshScreeningsPage(); }
                else if (AdminPanel.SubCustomers.IsChecked == true) { _customerPage = page; RefreshUsersPage(); }
                else if (AdminPanel.SubTickets.IsChecked == true) { _ticketPage = page; RefreshTicketsPage(); }
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
            else if (AdminPanel.SubCustomers.IsChecked == true)
            {
                int maxPage = (int)Math.Ceiling((double)DataService.AllUsers.Count / itemsPerPage);
                if (_customerPage < maxPage) { _customerPage++; RefreshUsersPage(); }
            }
            else if (AdminPanel.SubTickets.IsChecked == true)
            {
                int maxPage = (int)Math.Ceiling((double)DataService.AllTickets.Count / itemsPerPage);
                if (_ticketPage < maxPage) { _ticketPage++; RefreshTicketsPage(); }
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
            else if (AdminPanel.SubCustomers.IsChecked == true && _customerPage > 1)
            {
                _customerPage--; RefreshUsersPage();
            }
            else if (AdminPanel.SubTickets.IsChecked == true && _ticketPage > 1)
            {
                _ticketPage--; RefreshTicketsPage();
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
        private void AdminPanel_TicketSearchChanged(object sender, string searchText)
        {
            _ticketSearchText = searchText == "Search ticket ID..." ? "" : searchText;
            _ticketPage = 1;
            RefreshTicketsPage();
        }

        private async void AddUserPopup_UserSaved(object sender, EventArgs e)
        {
            await DataService.FetchUsers();
            RefreshUsersPage();

            if (DataService.CurrentUser != null)
            {
                var updatedSelf = DataService.AllUsers.FirstOrDefault(u => u.Id == DataService.CurrentUser.Id);

                if (updatedSelf != null)
                {
                    DataService.CurrentUser = updatedSelf;
                    ApplyTestRole(DataService.CurrentUser);
                }
            }
        }

        // CASHIER
        private string GetFormattedSeatName(Ticket t)
        {
            string seatName = $"ID:{t.SeatId}";
            var screening = DataService.AllScreenings.FirstOrDefault(scr => scr.Id == t.ScreeningId);
            var hall = DataService.AllCinemaHalls.FirstOrDefault(h => h.Id == screening?.CinemaHallId);

            if (hall?.Rows != null)
            {
                foreach (var row in hall.Rows)
                {
                    var seat = row.Seats?.FirstOrDefault(s => s.Id == t.SeatId);
                    if (seat != null)
                    {
                        seatName = $"R{row.RowNumber} S{row.Seats.IndexOf(seat) + 1}";
                        break;
                    }
                }
            }
            return seatName;
        }

        private void CashierPanel_VerifyTicketRequested(object sender, string inputId)
        {
            if (string.IsNullOrWhiteSpace(inputId)) return;

            string rawId = inputId.Contains("-") ? inputId.Split('-').Last() : inputId;
            if (!int.TryParse(rawId, out int id))
            {
                CashierPanel.ShowValidationResult(false, "Invalid Ticket ID format.");
                AppToast.ShowToast("Validation failed: Invalid format.", false);
                return;
            }

            var mainTicket = DataService.AllTickets.FirstOrDefault(t => t.Id == id);

            if (mainTicket == null)
            {
                CashierPanel.ShowValidationResult(false, $"Order #{id} not found.");
                AppToast.ShowToast("Order not found in database.", false);
                return;
            }

            var orderGroup = DataService.AllTickets.Where(t =>
                t.PurchaseDate.ToString("yyyyMMddHHmmss") == mainTicket.PurchaseDate.ToString("yyyyMMddHHmmss") &&
                t.UserId == mainTicket.UserId &&
                t.GuestEmail == mainTicket.GuestEmail
            ).ToList();

            var uiItems = orderGroup.Select(t => new Views.CashierTicketItem
            {
                Id = t.Id,
                SeatDisplay = GetFormattedSeatName(t),
                StatusText = t.IsCancelled ? "CANCELLED" : (t.IsValidated ? "ALREADY USED" : "VALID"),
                CanValidate = !t.IsCancelled && !t.IsValidated
            }).ToList();

            CashierPanel.ShowOrderDetails(uiItems);
        }
        private async void CashierPanel_ValidateSingleTicketRequested(object sender, int ticketId)
        {
            var ticket = DataService.AllTickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null || ticket.IsCancelled || ticket.IsValidated) return;

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"http://localhost:5035/api/Tickets/{ticketId}/validate");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        ticket.IsValidated = true;
                        AppToast.ShowToast($"Seat {GetFormattedSeatName(ticket)} successfully validated!", true);

                        RefreshTicketsPage();
                        ProfilePanel.FetchUserTickets();

                        CashierPanel_VerifyTicketRequested(this, ticketId.ToString());
                    }
                    else
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        AppToast.ShowToast($"Validation failed: {errorDetail}", false);
                    }
                }
            }
            catch (Exception ex)
            {
                AppToast.ShowToast("Network error: " + ex.Message, false);
            }
        }

        private async void CashierPanel_IssueAllTicketsRequested(object sender, ObservableCollection<Views.OrderItem> orderItems)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    foreach (var item in orderItems)
                    {
                        var newTicket = new Ticket
                        {
                            ScreeningId = item.ScreeningId,
                            SeatId = item.SeatId,
                            Price = item.Price,
                            PurchaseDate = DateTime.Now,
                            GuestEmail = "walk-in@cinema.com",
                            GuestPhone = "+36300000000",
                            IsValidated = false
                        };

                        var json = JsonSerializer.Serialize(newTicket);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        var response = await client.PostAsync("http://localhost:5035/api/Tickets", content);

                        if (!response.IsSuccessStatusCode)
                        {
                            string errorMsg = await response.Content.ReadAsStringAsync();
                            AppToast.ShowToast($"Failed to save ticket for {item.MovieTitle}: {errorMsg}", false);
                            return;
                        }
                    }
                }

                AppToast.ShowToast($"{orderItems.Count} ticket(s) successfully issued and saved!", true);

                await DataService.FetchAllTickets();
                RefreshTicketsPage();
                UpdateDashboardStats();
            }
            catch (Exception ex)
            {
                AppToast.ShowToast("Connection error: " + ex.Message, false);
            }
        }

        // REFRESH LOGIKA (SZŰRÉS + LAPOZÁS)

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
        private void RefreshTicketsPage()
        {
            if (DataService.AllTickets == null) return;

            var flattenedTickets = new List<GroupedTicket>();

            foreach (var ticket in DataService.AllTickets)
            {
                string seatName = $"ID:{ticket.SeatId}";
                var screening = DataService.AllScreenings.FirstOrDefault(scr => scr.Id == ticket.ScreeningId);
                var hall = DataService.AllCinemaHalls.FirstOrDefault(h => h.Id == screening?.CinemaHallId);

                if (hall?.Rows != null && hall.Rows.Any())
                {
                    foreach (var row in hall.Rows)
                    {
                        var seatInRow = row.Seats?.FirstOrDefault(st => st.Id == ticket.SeatId);
                        if (seatInRow != null)
                        {
                            int seatNum = row.Seats.IndexOf(seatInRow) + 1;
                            seatName = $"R{row.RowNumber} S{seatNum}";
                            break;
                        }
                    }
                }

                flattenedTickets.Add(new GroupedTicket
                {
                    MainTicket = ticket,
                    TotalPrice = ticket.Price,
                    CombinedSeats = seatName,
                    AllTicketsInGroup = new List<Ticket> { ticket }
                });
            }

            var filteredList = flattenedTickets.Where(g =>
                string.IsNullOrEmpty(_ticketSearchText) ||
                _ticketSearchText == "Search ticket ID..." ||
                g.MainTicket.Id.ToString().Contains(_ticketSearchText) ||
                (g.MainTicket.CustomerEmail != null && g.MainTicket.CustomerEmail.ToLower().Contains(_ticketSearchText.ToLower()))
            ).OrderByDescending(g => g.MainTicket.PurchaseDate).ThenBy(g => g.MainTicket.Id).ToList();

            var pagedData = filteredList.Skip((_ticketPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            PagedTickets.Clear();
            foreach (var grp in pagedData) PagedTickets.Add(grp);

            UpdateTicketPagination(filteredList.Count);
            UpdateDashboardStats();
        }

        private void RefreshUsersPage()
        {
            var filteredList = DataService.AllUsers.ToList();
            var pagedData = filteredList.Skip((_customerPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            Customers.Clear();
            foreach (var user in pagedData)
            {
                Customers.Add(user);
            }
            UpdateCustomerPagination(filteredList.Count);
        }

        // ================= MOVIE LIST & CASHIER ESEMÉNYEK =================

        private void MovieCatalog_MovieDetailRequested(object sender, Movie selectedMovie) { MovieDetailPopup.OpenModal(selectedMovie, SelectedDate); }
        private void MovieCatalog_DateChanged(object sender, DateTime newDate) => SelectedDate = newDate;
        private void CalendarPopup_DateSelected(object sender, DateTime newDate) => SelectedDate = newDate;

        // ================= USER & SYSTEM =================
        public void ApplyTestRole(User user)
        {
            ProfileTab.Visibility = CashierTab.Visibility = AdminTab.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;
            LoggedInPanel.Visibility = Visibility.Collapsed;

            if (user != null)
            {
                LoginBtn.Visibility = Visibility.Collapsed;
                LoggedInPanel.Visibility = Visibility.Visible;

                TopUserNameTxt.Text = user.Username;
                UserNameText.Text = user.Username;

                if (user.Roles != null)
                {
                    ProfileTab.Visibility = Visibility.Visible;

                    bool isAdmin = user.Roles.Any(r => r.Name == "Admin");
                    bool isCashier = user.Roles.Any(r => r.Name == "Cashier");

                    if (isAdmin)
                        AdminTab.Visibility = Visibility.Visible;

                    if (isCashier)
                        CashierTab.Visibility = Visibility.Visible;

                    if (AdminTab.IsChecked == true && !isAdmin)
                    {
                        HomeTab.IsChecked = true;
                    }

                    if (CashierTab.IsChecked == true && !isCashier)
                    {
                        HomeTab.IsChecked = true;
                    }
                }
            }
            else
            {
                HomeTab.IsChecked = true;
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
            AppToast.ShowToast("Successfully logged out!", true);

            DataService.CurrentUser = null;

            NavigationService?.Navigate(new HomePage());
        }

        private void Login_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new LoginPage());

        private void UpdateMoviePagination(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

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

        private void UpdateTicketPagination(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

            int maxPagesToShow = 3;
            int startPage = Math.Max(1, _ticketPage - 1);
            int endPage = Math.Min(totalPages, startPage + maxPagesToShow - 1);
            if (endPage - startPage < maxPagesToShow - 1)
                startPage = Math.Max(1, endPage - maxPagesToShow + 1);

            TicketPageNumbers.Clear();
            for (int i = startPage; i <= endPage; i++)
                TicketPageNumbers.Add(new PageItem { Number = i, IsActive = (i == _ticketPage) });

            int startItem = totalItems == 0 ? 0 : ((_ticketPage - 1) * itemsPerPage) + 1;
            int endItem = Math.Min(_ticketPage * itemsPerPage, totalItems);

            TicketPaginationStatus = $"Showing tickets {startItem} to {endItem} of {totalItems} entries";
        }

        private void UpdateCustomerPagination(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

            int maxPagesToShow = 3;
            int startPage = Math.Max(1, _customerPage - 1);
            int endPage = Math.Min(totalPages, startPage + maxPagesToShow - 1);
            if (endPage - startPage < maxPagesToShow - 1)
                startPage = Math.Max(1, endPage - maxPagesToShow + 1);

            CustomerPageNumbers.Clear();
            for (int i = startPage; i <= endPage; i++)
                CustomerPageNumbers.Add(new PageItem { Number = i, IsActive = (i == _customerPage) });

            int startItem = totalItems == 0 ? 0 : ((_customerPage - 1) * itemsPerPage) + 1;
            int endItem = Math.Min(_customerPage * itemsPerPage, totalItems);

            AdminPanel.CustomerPaginationStatus = $"Showing users {startItem} to {endItem} of {totalItems} entries";
        }
        // ================= PROPERTY CHANGED =================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}