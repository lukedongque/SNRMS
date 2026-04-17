using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

            var tag = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString();
            switch (tag)
            {
                case "Groups":
                    GroupsPanel.Visibility = Visibility.Visible;
                    break;
                case "Students":
                    StudentsPanel.Visibility = Visibility.Visible;
                    break;
                case "Rotations":
                    RotationsPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void Logout_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ContentDialog logoutDialog = new ContentDialog
            {
                Title = "Logout Confirmation",
                Content = "Are you sure you want to log out of the system?",
                PrimaryButtonText = "Logout",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot 
            };

            logoutDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];

            ContentDialogResult result = await logoutDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (DataContext is InstructorDashboardViewModel vm)
                {
                    vm.LogoutCommand.Execute(null);
                }
            }
        }
    }
}