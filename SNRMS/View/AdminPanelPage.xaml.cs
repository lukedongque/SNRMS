using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
            SectionsPanel.Visibility = Visibility.Visible;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var vm = (AdminPanelViewModel)DataContext;
            vm.ErrorMessage = string.Empty;
            SectionsPanel.Visibility = Visibility.Collapsed;
            InstructorsPanel.Visibility = Visibility.Collapsed;
            HospitalsPanel.Visibility = Visibility.Collapsed;
            
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
            }
        }
    }
}