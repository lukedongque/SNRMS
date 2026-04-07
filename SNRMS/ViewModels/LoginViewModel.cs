using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNRMS.Core.Services;
using SNRMS.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SNRMS.ViewModels
{
    public partial class LoginViewModel: ObservableObject
    {
        [ObservableProperty]
        public partial string Username { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string Password { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [RelayCommand]
        public async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var authService = new AuthService(App.Database);
                var user = await authService.LoginAsync(Username, Password);


                if (user != null)
                {
                    if (SessionManager.CurrentUser!.Role == "Admin")
                    {
                        App.RootFrame?.Navigate(typeof(View.AdminPanelPage));
                    }
                    else if (SessionManager.CurrentUser.Role == "Instructor")
                    {
                        App.RootFrame?.Navigate(typeof(View.InstructorDashboardPage));
                    }
                    else if (SessionManager.CurrentUser.Role == "Student")
                    {
                        App.RootFrame?.Navigate(typeof(View.StudentDashboardPage));
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";

                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }
    }
}
