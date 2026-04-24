using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;

namespace ticketmasterwpf.Modals
{
    public partial class AddUserModal : UserControl
    {
        public event EventHandler UserSaved;
        public event Action<string, bool> ShowToastRequested;

        private User _editingUser = null;

        public AddUserModal()
        {
            InitializeComponent();
        }

        public void OpenModal(User user = null)
        {
            _editingUser = user;

            // 1. Alapból minden CheckBox-ról levesszük a pipát
            foreach (CheckBox cb in RoleCheckboxes.Children)
            {
                cb.IsChecked = false;
            }

            if (_editingUser == null)
            {
                UserModalTitle.Text = "Add New User";
                SaveUserBtn.Content = "Save User";
                ClearInputs();
                EmailInput.IsReadOnly = false;
                EmailInput.Opacity = 1.0;

                if (RoleCheckboxes.Children.Count > 0)
                    ((CheckBox)RoleCheckboxes.Children[0]).IsChecked = true;
            }
            else
            {
                UserModalTitle.Text = "Edit User Details";
                SaveUserBtn.Content = "Update User";

                UsernameInput.Text = _editingUser.Username;
                EmailInput.Text = _editingUser.Email;
                PhoneInput.Text = _editingUser.PhoneNumber;
                PasswordInput.Password = "";
                ConfirmPasswordInput.Password = "";

                if (_editingUser.Roles != null)
                {
                    foreach (var role in _editingUser.Roles)
                    {
                        foreach (CheckBox cb in RoleCheckboxes.Children)
                        {
                            if (cb.Content.ToString() == role.Name)
                            {
                                cb.IsChecked = true;
                            }
                        }
                    }
                }

                EmailInput.IsReadOnly = true;
                EmailInput.Opacity = 0.6;
            }

            this.Visibility = Visibility.Visible;
        }
        private void UserRole_Click(object sender, RoutedEventArgs e)
        {
            if (UserRoleCb.IsChecked == true)
            {
                CashierRoleCb.IsChecked = false;
                AdminRoleCb.IsChecked = false;
            }
        }

        private void HigherRole_Click(object sender, RoutedEventArgs e)
        {
            if (CashierRoleCb.IsChecked == true || AdminRoleCb.IsChecked == true)
            {
                UserRoleCb.IsChecked = false;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameInput.Text) || string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                ShowToastRequested?.Invoke("Username and Email are required!", false);
                return;
            }

            if (_editingUser == null && string.IsNullOrWhiteSpace(PasswordInput.Password))
            {
                ShowToastRequested?.Invoke("Password is required for new users!", false);
                return;
            }

            if (PasswordInput.Password != ConfirmPasswordInput.Password)
            {
                ShowToastRequested?.Invoke("Passwords do not match!", false);
                return;
            }

            var userData = new User
            {
                Id = _editingUser?.Id ?? 0,
                Username = UsernameInput.Text,
                Email = EmailInput.Text,
                PhoneNumber = PhoneInput.Text,
                PasswordHash = PasswordInput.Password,
                Roles = new()
            };

            foreach (CheckBox cb in RoleCheckboxes.Children)
            {
                if (cb.IsChecked == true)
                {
                    userData.Roles.Add(new Role { Name = cb.Content.ToString() });
                }
            }

            if (userData.Roles.Count == 0)
            {
                userData.Roles.Add(new Role { Name = "User" });
            }

            SaveUserBtn.IsEnabled = false;
            SaveUserBtn.Content = "Saving...";

            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = "http://localhost:5035/api/Users";
                    var json = JsonSerializer.Serialize(userData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response;

                    if (_editingUser == null)
                        response = await client.PostAsync(apiUrl, content);
                    else
                        response = await client.PutAsync($"{apiUrl}/{userData.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ShowToastRequested?.Invoke($"{userData.Username} saved successfully!", true);
                        UserSaved?.Invoke(this, EventArgs.Empty);
                        this.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        ShowToastRequested?.Invoke($"Server error: {errorMsg}", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowToastRequested?.Invoke($"Network error: {ex.Message}", false);
            }
            finally
            {
                SaveUserBtn.IsEnabled = true;
                SaveUserBtn.Content = _editingUser == null ? "Save User" : "Update User";
            }
        }

        private void ClearInputs()
        {
            UsernameInput.Text = "";
            EmailInput.Text = "";
            PhoneInput.Text = "";
            PasswordInput.Password = "";
            ConfirmPasswordInput.Password = "";

            if (RoleCheckboxes != null)
            {
                foreach (CheckBox cb in RoleCheckboxes.Children)
                {
                    cb.IsChecked = false;
                }
            }
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}