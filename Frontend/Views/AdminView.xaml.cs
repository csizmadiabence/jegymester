using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;
using static ticketmasterwpf.HomePage;

namespace ticketmasterwpf.Views
{
    public partial class AdminView : UserControl
    {
        public event EventHandler<string> SearchChanged;
        public event EventHandler<string> SortChanged;
        public event EventHandler<string> ScreeningSearchChanged;
        public event EventHandler<string> ScreeningSortChanged;
        public event EventHandler<string> TicketSearchChanged;  
        public event EventHandler AddMovieRequested;
        public event EventHandler<Movie> EditMovieRequested;
        public event EventHandler<Screening> EditScreeningRequested;
        public event EventHandler<Movie> DeleteMovieRequested;
        public event EventHandler AddScreeningRequested;
        public event EventHandler<Screening> DeleteScreeningRequested;
        public event EventHandler AddCustomerRequested;
        public event EventHandler<User> EditCustomerRequested;
        public event EventHandler<User> DeleteCustomerRequested;
        public event EventHandler<GroupedTicket> RefundTicketRequested;

        // Lapozáshoz tartozó események
        public event EventHandler PrevPageRequested;
        public event EventHandler NextPageRequested;
        public event EventHandler<int> PageNumberRequested;
        public static readonly DependencyProperty PageNumbersProperty = DependencyProperty.Register("PageNumbers", typeof(ObservableCollection<int>), typeof(AdminView));

        public ObservableCollection<int> PageNumbers
        {
            get => (ObservableCollection<int>)GetValue(PageNumbersProperty);
            set => SetValue(PageNumbersProperty, value);
        }
        public static readonly DependencyProperty MoviePaginationStatusProperty = DependencyProperty.Register("MoviePaginationStatus", typeof(string), typeof(AdminView), new PropertyMetadata("Showing 0 entries"));
        public string MoviePaginationStatus { get => (string)GetValue(MoviePaginationStatusProperty); set => SetValue(MoviePaginationStatusProperty, value); }

        public static readonly DependencyProperty ScreeningPaginationStatusProperty = DependencyProperty.Register("ScreeningPaginationStatus", typeof(string), typeof(AdminView), new PropertyMetadata("Showing 0 entries"));
        public string ScreeningPaginationStatus { get => (string)GetValue(ScreeningPaginationStatusProperty); set => SetValue(ScreeningPaginationStatusProperty, value); }

        public static readonly DependencyProperty MoviePageNumbersProperty = DependencyProperty.Register("MoviePageNumbers", typeof(ObservableCollection<PageItem>), typeof(AdminView));
        public ObservableCollection<PageItem> MoviePageNumbers { get => (ObservableCollection<PageItem>)GetValue(MoviePageNumbersProperty); set => SetValue(MoviePageNumbersProperty, value); }

        public static readonly DependencyProperty ScreeningPageNumbersProperty = DependencyProperty.Register("ScreeningPageNumbers", typeof(ObservableCollection<PageItem>), typeof(AdminView));
        public ObservableCollection<PageItem> ScreeningPageNumbers { get => (ObservableCollection<PageItem>)GetValue(ScreeningPageNumbersProperty); set => SetValue(ScreeningPageNumbersProperty, value); }

        public static readonly DependencyProperty TicketsProperty = DependencyProperty.Register("Tickets", typeof(ObservableCollection<GroupedTicket>), typeof(AdminView));
        public ObservableCollection<GroupedTicket> Tickets { get => (ObservableCollection<GroupedTicket>)GetValue(TicketsProperty); set => SetValue(TicketsProperty, value); }

        public static readonly DependencyProperty TicketPaginationStatusProperty = DependencyProperty.Register("TicketPaginationStatus", typeof(string), typeof(AdminView));
        public string TicketPaginationStatus { get => (string)GetValue(TicketPaginationStatusProperty); set => SetValue(TicketPaginationStatusProperty, value); }

        public static readonly DependencyProperty TicketPageNumbersProperty = DependencyProperty.Register("TicketPageNumbers", typeof(ObservableCollection<PageItem>), typeof(AdminView));
        public ObservableCollection<PageItem> TicketPageNumbers { get => (ObservableCollection<PageItem>)GetValue(TicketPageNumbersProperty); set => SetValue(TicketPageNumbersProperty, value); }

        public static readonly DependencyProperty CustomerPaginationStatusProperty = DependencyProperty.Register("CustomerPaginationStatus", typeof(string), typeof(AdminView));

        public string CustomerPaginationStatus { get => (string)GetValue(CustomerPaginationStatusProperty); set => SetValue(CustomerPaginationStatusProperty, value); }

        public static readonly DependencyProperty CustomerPageNumbersProperty = DependencyProperty.Register("CustomerPageNumbers", typeof(ObservableCollection<PageItem>), typeof(AdminView));
        public ObservableCollection<PageItem> CustomerPageNumbers { get => (ObservableCollection<PageItem>)GetValue(CustomerPageNumbersProperty); set => SetValue(CustomerPageNumbersProperty, value); }

        public static readonly DependencyProperty CustomersProperty = DependencyProperty.Register("Customers", typeof(ObservableCollection<User>), typeof(AdminView));

        public static readonly DependencyProperty TotalRevenueProperty = DependencyProperty.Register("TotalRevenue", typeof(string), typeof(AdminView), new PropertyMetadata("0 Ft"));
        public string TotalRevenue { get => (string)GetValue(TotalRevenueProperty); set => SetValue(TotalRevenueProperty, value); }

        public static readonly DependencyProperty ActiveUserCountProperty = DependencyProperty.Register("ActiveUserCount", typeof(string), typeof(AdminView), new PropertyMetadata("0"));
        public string ActiveUserCount { get => (string)GetValue(ActiveUserCountProperty); set => SetValue(ActiveUserCountProperty, value); }

        public static readonly DependencyProperty RevenueChartDataProperty = DependencyProperty.Register("RevenueChartData", typeof(ObservableCollection<ChartBar>), typeof(AdminView));
        public ObservableCollection<ChartBar> RevenueChartData { get => (ObservableCollection<ChartBar>)GetValue(RevenueChartDataProperty); set => SetValue(RevenueChartDataProperty, value); }

        public static readonly DependencyProperty TopMoviesListProperty = DependencyProperty.Register("TopMoviesList", typeof(ObservableCollection<TopMovieStat>), typeof(AdminView));
        public ObservableCollection<TopMovieStat> TopMoviesList { get => (ObservableCollection<TopMovieStat>)GetValue(TopMoviesListProperty); set => SetValue(TopMoviesListProperty, value); }
        public ObservableCollection<User> Customers
        {
            get => (ObservableCollection<User>)GetValue(CustomersProperty);
            set => SetValue(CustomersProperty, value);
        }

        public static readonly DependencyProperty MoviesProperty = DependencyProperty.Register("Movies", typeof(ObservableCollection<Movie>), typeof(AdminView));
        public ObservableCollection<Movie> Movies
        {
            get => (ObservableCollection<Movie>)GetValue(MoviesProperty);
            set => SetValue(MoviesProperty, value);
        }

        public static readonly DependencyProperty ScreeningsProperty = DependencyProperty.Register("Screenings", typeof(ObservableCollection<Screening>), typeof(AdminView));

        public ObservableCollection<Screening> Screenings
        {
            get => (ObservableCollection<Screening>)GetValue(ScreeningsProperty);
            set => SetValue(ScreeningsProperty, value);
        }

        public AdminView()
        {
            InitializeComponent();
        }
        // SZORTIROZAS

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchChanged?.Invoke(this, (sender as TextBox).Text);
        }

        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem is ComboBoxItem item)
            {
                SortChanged?.Invoke(this, item.Tag.ToString());
            }
        }

        private void SearchScreeningInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScreeningSearchChanged?.Invoke(this, (sender as TextBox).Text);
        }

        private void SortScreeningCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item)
            {
                ScreeningSortChanged?.Invoke(this, item.Content.ToString());
            }
        }
        private void SearchTicketInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TicketSearchChanged?.Invoke(this, (sender as TextBox).Text);
        }

        // --- GOMBOK KATTINTÁSA (Csak továbbküldjük a kérést) ---

        private void OpenAddMovie_Click(object sender, RoutedEventArgs e) => AddMovieRequested?.Invoke(this, EventArgs.Empty);

        private void EditMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Movie movie)
                EditMovieRequested?.Invoke(this, movie);
        }

        private void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Movie movie)
                DeleteMovieRequested?.Invoke(this, movie);
        }

        private void OpenAddScreening_Click(object sender, RoutedEventArgs e) => AddScreeningRequested?.Invoke(this, EventArgs.Empty);
        private void EditScreening_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Screening screening)
            {
                EditScreeningRequested?.Invoke(this, screening);
            }
        }
        private void DeleteScreening_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Screening screening)
            {
                DeleteScreeningRequested?.Invoke(this, screening);
            }
        }
        private void OpenAddCustomer_Click(object sender, RoutedEventArgs e) => AddCustomerRequested?.Invoke(this, EventArgs.Empty);

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                EditCustomerRequested?.Invoke(this, user);
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                DeleteCustomerRequested?.Invoke(this, user);
            }
        }
        private void RefundTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GroupedTicket groupedTicket)
            {
                RefundTicketRequested?.Invoke(this, groupedTicket);
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e) => PrevPageRequested?.Invoke(this, EventArgs.Empty);

        private void NextPage_Click(object sender, RoutedEventArgs e) => NextPageRequested?.Invoke(this, EventArgs.Empty);

        private void PageNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Content != null)
            {
                if (int.TryParse(rb.Content.ToString(), out int page))
                    PageNumberRequested?.Invoke(this, page);
            }
        }
    }
}