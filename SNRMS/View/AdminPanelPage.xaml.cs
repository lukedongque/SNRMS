using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
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

            SectionsPanel.Visibility  = Visibility.Collapsed;
            InstructorsPanel.Visibility = Visibility.Collapsed;
            HospitalsPanel.Visibility  = Visibility.Collapsed;
            AnalyticsPanel.Visibility  = Visibility.Collapsed;

            var tag = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString();
            switch (tag)
            {
                case "Sections":
                    SectionsPanel.Visibility = Visibility.Visible;
                    break;
                case "Instructors":
                    InstructorsPanel.Visibility = Visibility.Visible;
                    break;
                case "Hospitals":
                    HospitalsPanel.Visibility = Visibility.Visible;
                    break;
                case "Analytics":
                    AnalyticsPanel.Visibility = Visibility.Visible;
                    _ = vm.LoadAnalyticsAsync();
                    break;
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
                ((AdminPanelViewModel)DataContext).LogoutCommand.Execute(null);
                App.RootFrame.Navigate(typeof(LoginPage));
                App.RootFrame.BackStack.Clear();
            }
        }
    }
}
