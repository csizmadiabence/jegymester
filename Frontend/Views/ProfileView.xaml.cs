using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Views
{
    public partial class ProfileView : UserControl
    {
        public event Action<string, bool> ShowToastRequested;
        public event EventHandler ProfileUpdated;
        public event EventHandler<Ticket> ViewDigitalPassRequested;
        public event EventHandler<GroupedTicket> CancelTicketRequested;

        public class GroupedTicket
        {
            public Ticket MainTicket { get; set; }
            public string CombinedSeats { get; set; }
            public int TotalPrice { get; set; }
            public List<Ticket> AllTicketsInGroup { get; set; }
        }

        private const int itemsPerPage = 5;
        private int _currentPage = 1;
        private int _totalGroups = 0;

        public static readonly DependencyProperty PagedUserTicketsProperty = DependencyProperty.Register("PagedUserTickets", typeof(ObservableCollection<GroupedTicket>), typeof(ProfileView));
        public ObservableCollection<GroupedTicket> PagedUserTickets { get => (ObservableCollection<GroupedTicket>)GetValue(PagedUserTicketsProperty); set => SetValue(PagedUserTicketsProperty, value); }

        public static readonly DependencyProperty TicketPaginationStatusProperty = DependencyProperty.Register("TicketPaginationStatus", typeof(string), typeof(ProfileView));
        public string TicketPaginationStatus { get => (string)GetValue(TicketPaginationStatusProperty); set => SetValue(TicketPaginationStatusProperty, value); }

        public static readonly DependencyProperty TicketPageNumbersProperty = DependencyProperty.Register("TicketPageNumbers", typeof(ObservableCollection<PageItem>), typeof(ProfileView));
        public ObservableCollection<PageItem> TicketPageNumbers { get => (ObservableCollection<PageItem>)GetValue(TicketPageNumbersProperty); set => SetValue(TicketPageNumbersProperty, value); }

        public ProfileView()
        {
            InitializeComponent();
            PagedUserTickets = new ObservableCollection<GroupedTicket>();
            TicketPageNumbers = new ObservableCollection<PageItem>();
            this.Loaded += ProfileView_Loaded;
        }

        private void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
            FetchUserTickets();
        }

        public void LoadUserData()
        {
            if (DataService.CurrentUser != null)
            {
                ProfileUsernameInput.Text = DataService.CurrentUser.Username;
                ProfileEmailInput.Text = DataService.CurrentUser.Email;

                string fullPhone = DataService.CurrentUser.PhoneNumber ?? "";
                bool prefixFound = false;

                foreach (ComboBoxItem item in PhonePrefixCombo.Items)
                {
                    string tag = item.Tag?.ToString() ?? "";
                    if (fullPhone.StartsWith(tag) && tag != "+")
                    {
                        PhonePrefixCombo.SelectedItem = item;
                        ProfilePhoneInput.Text = fullPhone.Substring(tag.Length);
                        prefixFound = true;
                        break;
                    }
                }

                if (!prefixFound)
                {
                    ProfilePhoneInput.Text = fullPhone;
                }
            }
        }

        public void FetchUserTickets()
        {
            _currentPage = 1;
            RefreshTicketsPage();
        }

        private void RefreshTicketsPage()
        {
            if (DataService.CurrentUser == null) return;

            var groupedData = DataService.AllTickets
                .Where(t => t.UserId == DataService.CurrentUser.Id && (t.StatusText != "Cancelled"))
                .GroupBy(t => new {
                    t.ScreeningId,
                    DateKey = t.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .OrderByDescending(g => g.First().PurchaseDate)
                .ToList();

            _totalGroups = groupedData.Count;

            var pagedData = groupedData.Skip((_currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            PagedUserTickets.Clear();
            foreach (var group in pagedData)
            {
                var mainTicket = group.First();
                var allTicketsInGroup = group.ToList();

                var seatNames = allTicketsInGroup.OrderBy(t => t.SeatId).Select(t => {
                    string seatName = t.SeatId.ToString();
                    var screening = DataService.AllScreenings.FirstOrDefault(scr => scr.Id == t.ScreeningId);
                    var hall = DataService.AllCinemaHalls.FirstOrDefault(h => h.Id == screening?.CinemaHallId);

                    if (hall?.Rows != null)
                    {
                        foreach (var row in hall.Rows)
                        {
                            bool seatIsInThisRow = row.Seats.Any(s => s.Id == t.SeatId);
                            if (seatIsInThisRow)
                            {
                                int displayCounter = 1;
                                foreach (var s in row.Seats.OrderBy(x => x.Id))
                                {
                                    if (!s.IsHidden)
                                    {
                                        if (s.Id == t.SeatId)
                                        {
                                            seatName = $"R{row.RowNumber} S{displayCounter}";
                                            break;
                                        }
                                        displayCounter++;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    return seatName;
                });

                PagedUserTickets.Add(new GroupedTicket
                {
                    MainTicket = mainTicket,
                    TotalPrice = allTicketsInGroup.Sum(t => t.Price),
                    CombinedSeats = "Seats: " + string.Join(", ", seatNames),
                    AllTicketsInGroup = allTicketsInGroup
                });
            }

            UpdatePagination(_totalGroups);
        }

        private void UpdatePagination(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

            TicketPageNumbers.Clear();
            for (int i = 1; i <= totalPages; i++)
                TicketPageNumbers.Add(new PageItem { Number = i, IsActive = (i == _currentPage) });

            int startItem = totalItems == 0 ? 0 : ((_currentPage - 1) * itemsPerPage) + 1;
            int endItem = Math.Min(_currentPage * itemsPerPage, totalItems);
            TicketPaginationStatus = $"Showing {startItem} to {endItem} of {totalItems} purchases";
        }

        // --- LAPOZÓ ESEMÉNYEK ---
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; RefreshTicketsPage(); }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalGroups / itemsPerPage);
            if (_currentPage < totalPages) { _currentPage++; RefreshTicketsPage(); }
        }

        private void PageNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && int.TryParse(rb.Content.ToString(), out int p))
            {
                _currentPage = p;
                RefreshTicketsPage();
            }
        }

        private void ViewPass_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GroupedTicket group)
            {
                var ticket = group.MainTicket;

                string formattedTicketId = $"TID-{ticket.PurchaseDate:yyyyMMddHHmm}-{ticket.Id}";

                var ticketModal = new ticketmasterwpf.Modals.DigitalTicketModal(
                    ticket.MovieTitle,
                    ticket.SessionTime,
                    ticket.LinkedScreening?.CinemaHall?.Name ?? "Unknown Hall",
                    group.CombinedSeats,
                    formattedTicketId,
                    ticket.LinkedScreening?.Movie?.PosterUrl,
                    $"{group.TotalPrice} Ft"
                );

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowModal(ticketModal);
            }
        }

        // --- MENTÉS ---
        private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (DataService.CurrentUser == null) return;

            string prefix = (PhonePrefixCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
            string finalPhone = prefix + ProfilePhoneInput.Text.Trim();

            var updatedUser = new User
            {
                Id = DataService.CurrentUser.Id,
                Username = ProfileUsernameInput.Text,
                Email = ProfileEmailInput.Text,
                PhoneNumber = finalPhone,
                Roles = new(DataService.CurrentUser.Roles)
            };

            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = $"http://localhost:5035/api/Users/{updatedUser.Id}";
                    var json = JsonSerializer.Serialize(updatedUser);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        DataService.CurrentUser.Username = updatedUser.Username;
                        DataService.CurrentUser.PhoneNumber = updatedUser.PhoneNumber;
                        ProfileUpdated?.Invoke(this, EventArgs.Empty);
                        ShowToastRequested?.Invoke("Profile updated successfully!", true);
                    }
                }
            }
            catch (Exception ex) { ShowToastRequested?.Invoke("Error: " + ex.Message, false); }
        }

        private async void UpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            string currentPwd = CurrentPasswordBox.Password;
            string newPwd = NewPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(currentPwd) || string.IsNullOrWhiteSpace(newPwd))
            {
                ShowToastRequested?.Invoke("Please fill out both password fields!", false);
                return;
            }

            if (DataService.CurrentUser == null) return;

            if (DataService.CurrentUser.PasswordHash != currentPwd)
            {
                ShowToastRequested?.Invoke("Hiba! Incorrect current password.", false);
                return;
            }

            var updatedUser = new User
            {
                Id = DataService.CurrentUser.Id,
                Username = DataService.CurrentUser.Username,
                Email = DataService.CurrentUser.Email,
                PhoneNumber = DataService.CurrentUser.PhoneNumber,
                PasswordHash = newPwd,
                Roles = new(DataService.CurrentUser.Roles)
            };

            var mainWin = Application.Current.MainWindow as MainWindow;
            mainWin?.ShowLoading();

            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = $"http://localhost:5035/api/Users/{updatedUser.Id}";
                    var json = JsonSerializer.Serialize(updatedUser);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        DataService.CurrentUser.PasswordHash = newPwd;

                        ShowToastRequested?.Invoke("Password successfully updated!", true);
                        CurrentPasswordBox.Clear();
                        NewPasswordBox.Clear();
                    }
                    else
                    {
                        ShowToastRequested?.Invoke("Error saving new password to server.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke("Network Error: " + ex.Message, false);
            }
            finally
            {
                mainWin?.HideLoading();
            }
        }

        // --- JEGY TÖRLÉSE ---
        private void CancelTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GroupedTicket groupedTicket)
            {
                CancelTicketRequested?.Invoke(this, groupedTicket);
            }
        }
    }
}