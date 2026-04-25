using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using SNRMS.ViewModels;
using System;

namespace SNRMS.View
{
    public sealed partial class InstructorDashboardPage : Page
    {
        public InstructorDashboardPage()
        {
            InitializeComponent();
            var vm = (InstructorDashboardViewModel)DataContext;
            _ = vm.LoadDataAsync();
            GroupsPanel.Visibility = Visibility.Visible;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var vm = (InstructorDashboardViewModel)DataContext;
            vm.ErrorMessage = string.Empty;
            vm.SuccessMessage = string.Empty;
            GroupsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            RotationsPanel.Visibility = Visibility.Collapsed;
            AttendancePanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;

            if (args.IsSettingsSelected)
            {
                SettingsPanel.Visibility = Visibility.Visible;
                return;
            }
            var tag = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString();
            switch (tag)
            {
                case "Groups":    GroupsPanel.Visibility    = Visibility.Visible; break;
                case "Students":  StudentsPanel.Visibility  = Visibility.Visible; break;
                case "Rotations": RotationsPanel.Visibility = Visibility.Visible; break;
                case "Attendance": AttendancePanel.Visibility = Visibility.Visible; break;
            }
        }

        private async void EditStudent_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (InstructorDashboardViewModel)DataContext;
            if (vm.SelectedStudent == null)
            {
                vm.ErrorMessage = "Please select a student to edit.";
                return;
            }

            var firstNameBox  = new TextBox { Header = "First Name",     Text = vm.SelectedStudent.FirstName,     Margin = new Thickness(0, 0, 0, 8) };
            var lastNameBox   = new TextBox { Header = "Last Name",      Text = vm.SelectedStudent.LastName,      Margin = new Thickness(0, 0, 0, 8) };
            var emailBox      = new TextBox { Header = "Email",          Text = vm.SelectedStudent.Email,         Margin = new Thickness(0, 0, 0, 8) };
            var studentNumBox = new TextBox { Header = "Student Number", Text = vm.SelectedStudent.StudentNumber, Margin = new Thickness(0, 0, 0, 8) };
            var errorText     = new TextBlock { Foreground = new SolidColorBrush(Colors.Red), FontSize = 12, Visibility = Visibility.Collapsed, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap };

            var form = new StackPanel { Width = 340 };
            form.Children.Add(firstNameBox);
            form.Children.Add(lastNameBox);
            form.Children.Add(emailBox);
            form.Children.Add(studentNumBox);
            form.Children.Add(errorText);

            var dialog = new ContentDialog
            {
                Title = $"Edit Student — {vm.SelectedStudent.FirstName} {vm.SelectedStudent.LastName}",
                Content = form,
                PrimaryButtonText = "Save Changes",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                args.Cancel = true;
                errorText.Visibility = Visibility.Collapsed;

                if (string.IsNullOrWhiteSpace(firstNameBox.Text) ||
                    string.IsNullOrWhiteSpace(lastNameBox.Text)  ||
                    string.IsNullOrWhiteSpace(emailBox.Text)     ||
                    string.IsNullOrWhiteSpace(studentNumBox.Text))
                {
                    errorText.Text = "All fields are required.";
                    errorText.Visibility = Visibility.Visible;
                    return;
                }

                vm.EditStudentFirstName = firstNameBox.Text.Trim();
                vm.EditStudentLastName  = lastNameBox.Text.Trim();
                vm.EditStudentEmail     = emailBox.Text.Trim();
                vm.EditStudentNumber    = studentNumBox.Text.Trim();

                try
                {
                    await vm.SaveStudentEditAsync();
                    dialog.Hide();
                }
                catch (Exception ex)
                {
                    errorText.Text = ex.Message;
                    errorText.Visibility = Visibility.Visible;
                }
            };

            await dialog.ShowAsync();
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var vm = (InstructorDashboardViewModel)DataContext;
            var currentPasswordBox = new PasswordBox { Header = "Current Password", Margin = new Thickness(0, 0, 0, 8) };
            var newPasswordBox = new PasswordBox { Header = "New Password", Margin = new Thickness(0, 0, 0, 8) };
            var confirmPasswordBox = new PasswordBox { Header = "Confirm New Password", Margin = new Thickness(0, 0, 0, 8) };
            var errorText = new TextBlock { Foreground = new SolidColorBrush(Colors.Red), FontSize = 12, Visibility = Visibility.Collapsed };
            var form = new StackPanel { Width = 340 };
            form.Children.Add(currentPasswordBox);
            form.Children.Add(newPasswordBox);
            form.Children.Add(confirmPasswordBox);
            form.Children.Add(errorText);
            var dialog = new ContentDialog
            {
                Title = "Change Password",
                Content = form,
                PrimaryButtonText = "Change Password",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };
            dialog.PrimaryButtonClick += async (s, args) =>
            {
                args.Cancel = true;
                errorText.Visibility = Visibility.Collapsed;
                if (string.IsNullOrWhiteSpace(currentPasswordBox.Password) ||
                    string.IsNullOrWhiteSpace(newPasswordBox.Password)     ||
                    string.IsNullOrWhiteSpace(confirmPasswordBox.Password))
                {
                    errorText.Text = "All fields are required.";
                    errorText.Visibility = Visibility.Visible;
                    return;
                }
                if (newPasswordBox.Password != confirmPasswordBox.Password)
                {
                    errorText.Text = "New password and confirmation do not match.";
                    errorText.Visibility = Visibility.Visible;
                    return;
                }
                try
                {
                    await vm.ChangePasswordAsync(currentPasswordBox.Password, newPasswordBox.Password);
                    dialog.Hide();
                }
                catch (Exception ex)
                {
                    errorText.Text = ex.Message;
                    errorText.Visibility = Visibility.Visible;
                }
            };
            await dialog.ShowAsync();
        }
        private async void DeleteGroup_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (InstructorDashboardViewModel)DataContext;
            if (vm.SelectedGroup == null) { vm.ErrorMessage = "Please select a group to delete."; return; }
            var dialog = new ContentDialog
            {
                Title = "Delete Group",
                Content = $"Are you sure you want to delete \"{vm.SelectedGroup.GroupName}\"? Students in this group will be unassigned.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await vm.DeleteGroupCommand.ExecuteAsync(null);
        }

        private async void DeleteStudent_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (InstructorDashboardViewModel)DataContext;
            if (vm.SelectedStudent == null) { vm.ErrorMessage = "Please select a student to delete."; return; }
            var dialog = new ContentDialog
            {
                Title = "Delete Student",
                Content = $"Are you sure you want to remove {vm.SelectedStudent.FirstName} {vm.SelectedStudent.LastName} ({vm.SelectedStudent.StudentNumber}) from this section? Their account will be deactivated.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await vm.DeleteStudentCommand.ExecuteAsync(null);
        }

        private async void DeleteRotation_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (InstructorDashboardViewModel)DataContext;
            if (vm.SelectedRotationAssignment == null) { vm.ErrorMessage = "Please select a rotation assignment to delete."; return; }
            var ra = vm.SelectedRotationAssignment;
            var dialog = new ContentDialog
            {
                Title = "Delete Rotation Assignment",
                Content = $"Are you sure you want to delete the rotation for {ra.Group?.GroupName} at {ra.Station?.StationName} ({ra.DaySlot})?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await vm.DeleteRotationAssignmentCommand.ExecuteAsync(null);
        }
        private async void Logout_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var logoutDialog = new ContentDialog
            {
                Title = "Logout Confirmation",
                Content = "Are you sure you want to log out of the system?",
                PrimaryButtonText = "Logout",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            logoutDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
            if (await logoutDialog.ShowAsync() == ContentDialogResult.Primary)
                if (DataContext is InstructorDashboardViewModel vm)
                    vm.LogoutCommand.Execute(null);
        }
    }
}