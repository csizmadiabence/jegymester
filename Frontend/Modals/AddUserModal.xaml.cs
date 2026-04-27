using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Added for ObservableCollection
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ticketmasterwpf.Models;
using ticketmasterwpf.Services;

namespace ticketmasterwpf.Modals
{
    public partial class AddUserModal : UserControl
    {
        public event EventHandler UserSaved;
        public event Action<string, bool> ShowToastRequested;

        private readonly UserService _userService = new UserService();
        private User _editingUser = null;

        public AddUserModal()
        {
            InitializeComponent();
        }

        #region Public Interface
        public void OpenModal(User user = null)
        {
            _editingUser = user;
            ResetForm();

            if (_editingUser == null)
                PrepareForCreate();
            else
                PrepareForEdit();

            this.Visibility = Visibility.Visible;
        }
        #endregion

        #region UI Preparation Logic
        private void ResetForm()
        {
            ClearInputs();
            if (RoleCheckboxes != null)
            {
                foreach (CheckBox cb in RoleCheckboxes.Children)
                {
                    cb.IsChecked = false;
                }
            }
        }

        private void PrepareForCreate()
        {
            UserModalTitle.Text = "Add New User";
            SaveUserBtn.Content = "Save User";
            SetEmailFieldState(readOnly: false);

            if (RoleCheckboxes.Children.Count > 0)
                ((CheckBox)RoleCheckboxes.Children[0]).IsChecked = true;
        }

        private void PrepareForEdit()
        {
            UserModalTitle.Text = "Edit User Details";
            SaveUserBtn.Content = "Update User";
            SetEmailFieldState(readOnly: true);

            UsernameInput.Text = _editingUser.Username;
            EmailInput.Text = _editingUser.Email;
            LoadPhoneNumberIntoUI(_editingUser.PhoneNumber);
            LoadRolesIntoUI(_editingUser.Roles);
        }

        private void SetEmailFieldState(bool readOnly)
        {
            EmailInput.IsReadOnly = readOnly;
            EmailInput.Opacity = readOnly ? 0.6 : 1.0;
        }

        private void LoadPhoneNumberIntoUI(string fullPhone)
        {
            fullPhone ??= "";
            foreach (ComboBoxItem item in PhonePrefixCombo.Items)
            {
                string prefix = item.Tag.ToString();
                if (fullPhone.StartsWith(prefix))
                {
                    PhonePrefixCombo.SelectedItem = item;
                    PhoneInput.Text = fullPhone.Substring(prefix.Length);
                    return;
                }
            }
            PhoneInput.Text = fullPhone;
        }

        private void LoadRolesIntoUI(IEnumerable<Role> roles)
        {
            if (roles == null) return;
            foreach (var role in roles)
            {
                foreach (CheckBox cb in RoleCheckboxes.Children)
                {
                    if (cb.Content.ToString() == role.Name)
                        cb.IsChecked = true;
                }
            }
        }
        #endregion

        #region Event Handlers
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            SetLoadingState(true);

            var userData = CaptureFormData();
            var result = await _userService.SaveUserAsync(userData, _editingUser == null);

            if (result.Success)
            {
                ShowToastRequested?.Invoke($"{userData.Username} saved successfully!", true);
                UserSaved?.Invoke(this, EventArgs.Empty);
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowToastRequested?.Invoke(result.Message, false);
            }

            SetLoadingState(false);
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
                UserRoleCb.IsChecked = false;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;
        #endregion

        #region Helper Methods
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(UsernameInput.Text) || string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                ShowToastRequested?.Invoke("Username and Email are required!", false);
                return false;
            }

            if (_editingUser == null && string.IsNullOrWhiteSpace(PasswordInput.Password))
            {
                ShowToastRequested?.Invoke("Password is required for new users!", false);
                return false;
            }

            if (PasswordInput.Password != ConfirmPasswordInput.Password)
            {
                ShowToastRequested?.Invoke("Passwords do not match!", false);
                return false;
            }

            return true;
        }

        private User CaptureFormData()
        {
            var user = new User
            {
                Id = _editingUser?.Id ?? 0,
                Username = UsernameInput.Text,
                Email = EmailInput.Text,
                PhoneNumber = GetFormattedPhoneNumber(),
                Roles = GetSelectedRoles(),

                PasswordHash = PasswordInput.Password
            };

            return user;
        }

        private string GetFormattedPhoneNumber()
        {
            string prefix = ((ComboBoxItem)PhonePrefixCombo.SelectedItem).Tag.ToString();
            string pureNumber = PhoneInput.Text.Trim()
                .Replace(" ", "").Replace("-", "")
                .Replace("06", "").Replace("+36", "");

            return prefix + pureNumber;
        }

        private ObservableCollection<Role> GetSelectedRoles()
        {
            var roles = new ObservableCollection<Role>();
            foreach (CheckBox cb in RoleCheckboxes.Children)
            {
                if (cb.IsChecked == true)
                    roles.Add(new Role { Name = cb.Content.ToString() });
            }
            if (roles.Count == 0) roles.Add(new Role { Name = "User" });
            return roles;
        }

        private void SetLoadingState(bool isLoading)
        {
            SaveUserBtn.IsEnabled = !isLoading;
            if (isLoading)
                SaveUserBtn.Content = "Saving...";
            else
                SaveUserBtn.Content = _editingUser == null ? "Save User" : "Update User";
        }

        private void ClearInputs()
        {
            UsernameInput.Text = "";
            EmailInput.Text = "";
            PhoneInput.Text = "";
            PasswordInput.Password = "";
            ConfirmPasswordInput.Password = "";
        }
        #endregion
    }
}