using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;
using ticketmasterwpf.Controls;

namespace ticketmasterwpf.Views
{
    public partial class ProfileView : UserControl
    {
        public event Action<string, bool> ShowToastRequested;
        public event EventHandler ProfileUpdated;
        public ObservableCollection<TicketDisplayItem> UserTickets { get; set; } = new ObservableCollection<TicketDisplayItem>();

        public ProfileView()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += ProfileView_Loaded;
        }

        private void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
            FetchUserTickets();
        }

        private void LoadUserData()
        {
            if (DataService.CurrentUser != null)
            {
                ProfileUsernameInput.Text = DataService.CurrentUser.Username;
                ProfileEmailInput.Text = DataService.CurrentUser.Email;
                ProfilePhoneInput.Text = DataService.CurrentUser.PhoneNumber;
            }
        }

        private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (DataService.CurrentUser == null) return;

            var updatedUser = new User
            {
                Id = DataService.CurrentUser.Id,
                Username = ProfileUsernameInput.Text,
                Email = ProfileEmailInput.Text,
                PhoneNumber = ProfilePhoneInput.Text,
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
            ShowToastRequested?.Invoke("Password update feature coming soon!", false);
        }

        private async void FetchUserTickets()
        {
            if (DataService.CurrentUser == null) return;

            try
            {
                UserTickets.Clear();
            }
            catch (Exception ex) { ShowToastRequested?.Invoke("Error: " + ex.Message, false); }
        }
    }

    // Segédosztály a jegyek megjelenítéséhez a listában
    public class TicketDisplayItem
    {
        public string MovieTitle { get; set; }
        public string Time { get; set; }
        public string Hall { get; set; }
    }
}