using Microsoft.EntityFrameworkCore;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SNRMS.ViewModels;
using System;

namespace SNRMS.View
{
    public sealed partial class AdminPanelPage : Page
    {
        public AdminPanelPage()
        {
            InitializeComponent();
            var vm = (AdminPanelViewModel)DataContext;
            _ = vm.LoadDataAsync();
            AnalyticsPanel.Visibility = Visibility.Visible;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var vm = (AdminPanelViewModel)DataContext;
            vm.ErrorMessage = string.Empty;
            vm.SuccessMessage = string.Empty;
            SectionsPanel.Visibility = Visibility.Collapsed;
            InstructorsPanel.Visibility = Visibility.Collapsed;
            HospitalsPanel.Visibility = Visibility.Collapsed;
            AnalyticsPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;
            if (args.IsSettingsSelected)
            {
                SettingsPanel.Visibility = Visibility.Visible;
                return;
            }

            var tag = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString();
            switch (tag)
            {
                case "Sections": SectionsPanel.Visibility = Visibility.Visible; break;
                case "Instructors": InstructorsPanel.Visibility = Visibility.Visible; break;
                case "Hospitals": HospitalsPanel.Visibility = Visibility.Visible; break;
                case "Analytics":
                    AnalyticsPanel.Visibility = Visibility.Visible;
                    _ = vm.LoadAnalyticsAsync();
                    break;
            }
        }

        private async void EditInstructor_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            if (vm.SelectedInstructor == null)
            {
                vm.ErrorMessage = "Please select an instructor to edit.";
                return;
            }

            var firstNameBox = new TextBox { Header = "First Name", Text = vm.SelectedInstructor.FirstName, Margin = new Thickness(0, 0, 0, 8) };
            var lastNameBox = new TextBox { Header = "Last Name", Text = vm.SelectedInstructor.LastName, Margin = new Thickness(0, 0, 0, 8) };
            var emailBox = new TextBox { Header = "Email", Text = vm.SelectedInstructor.Email, Margin = new Thickness(0, 0, 0, 8) };
            var employeeIdBox = new TextBox { Header = "Employee ID", Text = vm.SelectedInstructor.EmployeeId, Margin = new Thickness(0, 0, 0, 8) };
            var noteText = new TextBlock
            {
                Text = "Note: Changing the Employee ID will also update the instructor's login username.",
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.Gray),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            var errorText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Red),
                FontSize = 12,
                Visibility = Visibility.Collapsed,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };

            var form = new StackPanel { Width = 340 };
            form.Children.Add(firstNameBox);
            form.Children.Add(lastNameBox);
            form.Children.Add(emailBox);
            form.Children.Add(employeeIdBox);
            form.Children.Add(noteText);
            form.Children.Add(errorText);

            var dialog = new ContentDialog
            {
                Title = $"Edit Instructor — {vm.SelectedInstructor.FirstName} {vm.SelectedInstructor.LastName}",
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
                    string.IsNullOrWhiteSpace(lastNameBox.Text) ||
                    string.IsNullOrWhiteSpace(emailBox.Text) ||
                    string.IsNullOrWhiteSpace(employeeIdBox.Text))
                {
                    errorText.Text = "All fields are required.";
                    errorText.Visibility = Visibility.Visible;
                    return;
                }

                vm.EditInstructorFirstName = firstNameBox.Text.Trim();
                vm.EditInstructorLastName = lastNameBox.Text.Trim();
                vm.EditInstructorEmail = emailBox.Text.Trim();
                vm.EditInstructorEmployeeId = employeeIdBox.Text.Trim();

                try
                {
                    await vm.SaveInstructorEditAsync();
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
        private async void AdminChangePassword_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            var currentPwd = AdminCurrentPwd.Password;
            var newPwd = AdminNewPwd.Password;
            var confirmPwd = AdminConfirmPwd.Password;

            if (string.IsNullOrWhiteSpace(currentPwd) || string.IsNullOrWhiteSpace(newPwd) || string.IsNullOrWhiteSpace(confirmPwd))
            {
                vm.ErrorMessage = "All password fields are required.";
                return;
            }
            if (newPwd != confirmPwd)
            {
                vm.ErrorMessage = "New password and confirmation do not match.";
                return;
            }
            if (newPwd.Length < 4)
            {
                vm.ErrorMessage = "New password must be at least 4 characters.";
                return;
            }

            var confirm = new ContentDialog
            {
                Title = "Confirm Password Change",
                Content = "Are you sure you want to change your password?",
                PrimaryButtonText = "Yes, Change It",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

            try
            {
                await vm.ChangePasswordAsync(currentPwd, newPwd);
                AdminCurrentPwd.Password = string.Empty;
                AdminNewPwd.Password = string.Empty;
                AdminConfirmPwd.Password = string.Empty;
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = ex.Message;
            }
        }
        private async void DeleteSection_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            if (vm.SelectedSection == null) { vm.ErrorMessage = "Please select a section to delete."; return; }
            var dialog = new ContentDialog
            {
                Title = "Delete Section",
                Content = $"Are you sure you want to delete \"{vm.SelectedSection.SectionName}\"? This cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                vm.DeleteSectionCommand.Execute(null);
        }

        private async void DeactivateInstructor_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            if (vm.SelectedInstructor == null) { vm.ErrorMessage = "Please select an instructor to deactivate."; return; }
            var dialog = new ContentDialog
            {
                Title = "Deactivate Instructor",
                Content = $"Deactivate {vm.SelectedInstructor.FirstName} {vm.SelectedInstructor.LastName}? Their account will be disabled and they will not be able to log in.",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await vm.DeactivateUserCommand.ExecuteAsync(null);
        }

        private async void DeleteHospital_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            if (vm.SelectedHospital == null) { vm.ErrorMessage = "Please select a hospital to delete."; return; }
            var dialog = new ContentDialog
            {
                Title = "Delete Hospital",
                Content = $"Are you sure you want to delete \"{vm.SelectedHospital.HospitalName}\"? All associated stations will also be removed.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await vm.DeleteHospitalCommand.ExecuteAsync(null);
        }

        private async void RemoveStation_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            if (vm.SelectedStation == null) { vm.ErrorMessage = "Please select a station to remove."; return; }
            var dialog = new ContentDialog
            {
                Title = "Remove Station",
                Content = $"Are you sure you want to remove \"{vm.SelectedStation.StationName}\"?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await vm.RemoveStationCommand.ExecuteAsync(null);
        }
        private async void ResetInstructorPassword_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            vm.ErrorMessage = string.Empty;
            vm.SuccessMessage = string.Empty;

            var employeeId = vm.ResetEmployeeId?.Trim();
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                vm.ErrorMessage = "Please enter an Employee ID.";
                return;
            }

            SNRMS.Core.Models.Instructor? instructor;
            try
            {
                instructor = await App.Database.Instructors
                    .FirstOrDefaultAsync(i => i.EmployeeId == employeeId);
                if (instructor == null)
                {
                    vm.ErrorMessage = "No instructor found with that Employee ID.";
                    return;
                }
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = ex.Message;
                return;
            }

            var confirm = new ContentDialog
            {
                Title = "Reset Instructor Password",
                Content = $"Reset password for {instructor.FirstName} {instructor.LastName} ({employeeId}) to default (user123)?",
                PrimaryButtonText = "Yes, Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

            try
            {
                await vm.ResetInstructorPasswordAsync(employeeId);
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = ex.Message;
            }
        }

        private async void ResetStudentPassword_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = (AdminPanelViewModel)DataContext;
            vm.ErrorMessage = string.Empty;
            vm.SuccessMessage = string.Empty;

            var studentNumber = vm.ResetStudentNumber?.Trim();
            if (string.IsNullOrWhiteSpace(studentNumber))
            {
                vm.ErrorMessage = "Please enter a Student Number.";
                return;
            }

            SNRMS.Core.Models.Student? student;
            try
            {
                student = await App.Database.Students
                    .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                if (student == null)
                {
                    vm.ErrorMessage = "No student found with that Student Number.";
                    return;
                }
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = ex.Message;
                return;
            }

            var confirm = new ContentDialog
            {
                Title = "Reset Student Password",
                Content = $"Reset password for {student.FirstName} {student.LastName} to their student number ({studentNumber})?",
                PrimaryButtonText = "Yes, Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

            try
            {
                await vm.ResetStudentPasswordAsync(studentNumber);
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = ex.Message;
            }
        }
        private async void Logout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var logoutDialog = new ContentDialog
            {
                Title = "Logout",
                Content = "Are you sure you want to log out?",
                PrimaryButtonText = "Logout",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            logoutDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
            if (await logoutDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                ((AdminPanelViewModel)DataContext).LogoutCommand.Execute(null);
                App.RootFrame.Navigate(typeof(LoginPage));
                App.RootFrame.BackStack.Clear();
            }
        }
    }
}