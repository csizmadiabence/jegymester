using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Runtime.CompilerServices;
using ticketmasterwpf.Models;

namespace ticketmasterwpf
{
    public partial class HomePage : Page, INotifyPropertyChanged
    {
        // Adatgyűjtemények a felülethez
        public ObservableCollection<Movie> AllMoviesForAdmin { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> PagedAdminMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public ObservableCollection<Movie> ComingSoonMovies { get; set; }
        public ObservableCollection<DateItem> AvailableDates { get; set; }
        private string _currentSearch = "";
        private string _currentSort = "ID_DESC";

        private Movie _featuredMovie;
        public Movie FeaturedMovie
        {
            get => _featuredMovie;
            set { _featuredMovie = value; OnPropertyChanged(); }
        }

        private Movie _selectedMovie;
        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set { _selectedMovie = value; OnPropertyChanged(); }
        }

        private int currentPage = 1;
        private const int itemsPerPage = 8;

        private string _paginationStatus;
        public string PaginationStatus
        {
            get => _paginationStatus;
            set { _paginationStatus = value; OnPropertyChanged(); }
        }

        public HomePage()
        {
            InitializeComponent();

            AvailableDates = new ObservableCollection<DateItem>();
            Movies = new ObservableCollection<Movie>();
            ComingSoonMovies = new ObservableCollection<Movie>();
            PageNumbers = new ObservableCollection<int>();

            MovieDetailPopup.ShowToastRequested += (s, message) =>
            {
                AppToast.ShowToast(message, true);
            };

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

            ApplyTestRole("Guest");
            this.DataContext = this;

            LoadMoviesFromApiAsync();
        }

        // ================= API HÍVÁSOK (CRUD) =================

        private async void LoadMoviesFromApiAsync()
        {
            string apiUrl = "http://localhost:5035/api/Movies";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var apiMovies = JsonSerializer.Deserialize<List<Movie>>(jsonString, options);

                        if (apiMovies != null)
                        {
                            Movies.Clear();
                            ComingSoonMovies.Clear();
                            AllMoviesForAdmin.Clear();
                            foreach (var movie in apiMovies)
                            {
                                AllMoviesForAdmin.Add(movie);

                                movie.PlaceholderColor = (Color)ColorConverter.ConvertFromString("#2C3E50");
                                movie.Showtimes = new ObservableCollection<string> { "14:30", "17:45", "20:15" };

                                if (movie.Status == "Active")
                                {
                                    Movies.Add(movie);
                                }
                                else if (movie.Status == "Upcoming")
                                {
                                    ComingSoonMovies.Add(movie);
                                }
                            }

                            RefreshAdminPage();

                            if (Movies.Count > 0)
                            {
                                FeaturedMovie = Movies[0];
                                UpdatePagination(Movies.Count);

                                if (HomeTab.IsChecked == true)
                                {
                                    MovieCatalog.SetMode(true, Movies);
                                }
                                else if (SoonTab.IsChecked == true)
                                {
                                    MovieCatalog.SetMode(false, ComingSoonMovies);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    AppToast.ShowToast("Nem sikerült kapcsolódni az API-hoz!", false);
                }
            }
        }

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
                        AppToast.ShowToast("Mentés sikeres!", true);
                        LoadMoviesFromApiAsync();
                    }
                }
                catch { AppToast.ShowToast("Hiba a mentés során!", false); }
            }
        }

        private async void DeletePopup_OnDeleteConfirmed(object sender, Movie movieToDelete)
        {
            if (movieToDelete == null) return;

            // A törléshez az ID kell az URL végére: /api/Movies/5
            string apiUrl = $"http://localhost:5035/api/Movies/{movieToDelete.Id}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.DeleteAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        AppToast.ShowToast($"{movieToDelete.Title} sikeresen törölve!", true);

                        // Ez a legfontosabb: újra lekérjük a friss listát az API-ból
                        // Így az AdminView inventory-ja is azonnal frissül!
                        LoadMoviesFromApiAsync();
                    }
                    else
                    {
                        AppToast.ShowToast("Szerver hiba a törlés során!", false);
                    }
                }
                catch (Exception ex)
                {
                    AppToast.ShowToast($"Hálózati hiba: {ex.Message}", false);
                }
            }
        }

        private void AddScreeningPopup_OnScreeningSaved(object sender, string message)
        {
            AppToast.ShowToast("Vetítés sikeresen rögzítve!", true);
        }

        private void AddMoviePopup_RefreshRequested(object sender, EventArgs e)
        {
            // Egyszerűen meghívjuk a már létező betöltő metódust
            LoadMoviesFromApiAsync();
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            // Ezzel szólunk a Listának, hogy mit kell mutatnia!
            if (MovieCatalog == null) return;

            if (sender is RadioButton rb)
            {
                if (rb.Name == "HomeTab")
                {
                    MovieCatalog.Visibility = Visibility.Visible;
                    MovieCatalog.SetMode(true, Movies); // Rendes lista
                }
                else if (rb.Name == "SoonTab")
                {
                    MovieCatalog.Visibility = Visibility.Visible;
                    MovieCatalog.SetMode(false, ComingSoonMovies); // Coming soon
                }
                else
                {
                    MovieCatalog.Visibility = Visibility.Collapsed; // Más füleknél rejtve van
                }
            }
        }

        // ================= ADMIN PANEL ESEMÉNYEK =================

        private void AdminPanel_AddMovieRequested(object sender, EventArgs e) => AddMoviePopup.OpenModal(null);

        private void AdminPanel_EditMovieRequested(object sender, Movie movie) => AddMoviePopup.OpenModal(movie);

        private void AdminPanel_DeleteMovieRequested(object sender, Movie movie) => DeletePopup.OpenModal(movie);

        private void AdminPanel_AddScreeningRequested(object sender, EventArgs e) => AddScreeningPopup.OpenModal();

        private void AdminPanel_PageNumberRequested(object sender, int page)
        {
            currentPage = page;
            RefreshAdminPage();
        }

        private void AdminPanel_NextPageRequested(object sender, EventArgs e)
        {
            int maxPage = (int)Math.Ceiling((double)AllMoviesForAdmin.Count / itemsPerPage);
            if (currentPage < maxPage)
            {
                currentPage++;
                RefreshAdminPage();
            }
        }
        private void AdminPanel_PrevPageRequested(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshAdminPage();
            }
        }

        private void AdminPanel_SearchChanged(object sender, string searchText)
        {
            _currentSearch = searchText;
            currentPage = 1;
            RefreshAdminPage();
        }

        private void AdminPanel_SortChanged(object sender, string sortTag)
        {
            _currentSort = sortTag;
            RefreshAdminPage();
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
                case "ID_ASC":
                    query = query.OrderBy(m => m.Id);
                    break;
                case "STATUS":
                    query = query.OrderBy(m => m.Status);
                    break;
                case "TITLE_ASC":
                    query = query.OrderBy(m => m.Title);
                    break;
                case "NEWEST":
                default:
                    query = query.OrderByDescending(m => m.Id);
                    break;
            }

            var filteredList = query.ToList();

            // 3. LAPOZÁS
            var pagedData = filteredList
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            PagedAdminMovies.Clear();
            foreach (var movie in pagedData) PagedAdminMovies.Add(movie);

            UpdatePagination(filteredList.Count);
        }

        // ================= MOVIE LIST ESEMÉNYEK =================

        private void MovieCatalog_MovieDetailRequested(object sender, Movie selectedMovie)
        {
            // Megnyitjuk az új részletes modált az adott filmmel!
            MovieDetailPopup.OpenModal(selectedMovie);
        }

        // ================= CASHIER PANEL ESEMÉNYEK =================

        private void CashierPanel_VerifyTicketRequested(object sender, string ticketId)
        {
            // Amíg nincs API, szimuláljuk a találatot
            if (ticketId == "12345" || ticketId == "0000")
            {
                CashierPanel.ShowValidationResult(true, "Dune: Part Two - 19:30\nRoom 1 | Seat: Row 4, Seat 12");
            }
            else
            {
                CashierPanel.ShowValidationResult(false, $"No ticket found in database with ID: {ticketId}");
            }
        }

        private void CashierPanel_IssueAllTicketsRequested(object sender, ObservableCollection<Views.OrderItem> orderItems)
        {
            // Később itt küldjük el az API-nak a rendeléseket egyesével vagy tömbben
            int totalTickets = orderItems.Count;

            AppToast.ShowToast($"Sikeres tranzakció! {totalTickets} db tétel kiállítva.", true);
        }

        // ================= UI LOGIKA & NAVIGÁCIÓ =================

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
            AppToast.ShowToast("Sikeres kijelentkezés!", true);
        }

        private void Login_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new LoginPage());

        private void UpdatePagination(int totalMovies)
        {
            int totalPages = (int)Math.Ceiling((double)totalMovies / itemsPerPage);
            PageNumbers.Clear();
            for (int i = 1; i <= totalPages; i++) PageNumbers.Add(i);
        }

        // ================= PROPERTY CHANGED =================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}