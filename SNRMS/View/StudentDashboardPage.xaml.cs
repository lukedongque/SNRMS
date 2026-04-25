using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SNRMS.Core.Services;
using SNRMS.ViewModels;
using System;
using Windows.UI.ApplicationSettings;

namespace SNRMS.View
{
    public sealed partial class StudentDashboardPage : Page
    {
        public StudentDashboardPage()
        {
            this.InitializeComponent();
            if (DataContext is StudentDashboardViewModel vm)
            {
                _ = vm.LoadDataAsync();
                RotationPanel.Visibility = Visibility.Visible;


            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
           
            var vm = (StudentDashboardViewModel)DataContext; 
            vm.ErrorMessage = string.Empty;

            RotationPanel.Visibility = Visibility.Collapsed;
            SchedulePanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;

            if (args.IsSettingsSelected)
            {
                SettingsPanel.Visibility = Visibility.Visible;
                return;
            }

            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Rotation": RotationPanel.Visibility = Visibility.Visible; break;
                    case "Schedule": SchedulePanel.Visibility = Visibility.Visible; break;
                    case "History": HistoryPanel.Visibility = Visibility.Visible; break;
                }
            }
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var vm = (StudentDashboardViewModel)DataContext;
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
                    string.IsNullOrWhiteSpace(newPasswordBox.Password) ||
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


        private async void Logout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentDialog logoutDialog = new ContentDialog
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
                ((StudentDashboardViewModel)DataContext).LogoutCommand.Execute(null);
                App.RootFrame.Navigate(typeof(LoginPage));
                App.RootFrame.BackStack.Clear();
            }
        }
    }
}
