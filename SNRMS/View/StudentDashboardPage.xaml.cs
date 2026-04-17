using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SNRMS.Core.Services;
using SNRMS.ViewModels;
using System;

namespace SNRMS.View
{
    public sealed partial class StudentDashboardPage : Page
    {
        public StudentDashboardPage()
        {
            this.InitializeComponent();
            // The DataContext is already set in XAML, so we just cast it
            if (DataContext is StudentDashboardViewModel vm)
            {
                _ = vm.LoadDataAsync();
                RotationPanel.Visibility = Visibility.Visible;


            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // We handle panel visibility using the 'Tag' and manual visibility 
            // OR you can eventually move this to the VM too. 
            // For now, let's keep the panel switching logic here but fix the cast error:

            var vm = (StudentDashboardViewModel)DataContext; // Fix: Use the correct VM class
            vm.ErrorMessage = string.Empty;

            RotationPanel.Visibility = Visibility.Collapsed;
            SchedulePanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility = Visibility.Collapsed;

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
