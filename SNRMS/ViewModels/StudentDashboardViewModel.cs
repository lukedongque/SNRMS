using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNRMS.Core.Models;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace SNRMS.ViewModels
{
    public partial class StudentDashboardViewModel : ObservableObject
    {
        private readonly RotationService _rotationService;
        public StudentDashboardViewModel()
        {
            _rotationService = new RotationService(App.Database);
        }
        [ObservableProperty]
        public partial RotationAssignment? CurrentRotation { get; set; }
        [ObservableProperty]
        public partial RotationAssignment? NextRotation { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<RotationAssignment> RotationHistory { get; set; } = new();
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var student = SessionManager.CurrentUser!.StudentId!.Value;
                var currentRotation = await _rotationService.GetCurrentRotationAssignmentAsync(student);
                var nextRotation = await _rotationService.GetNextRotationAssignment(student);
                var rotationHistory = await _rotationService.GetStudentRotationHistoryAsync(student);   
                CurrentRotation = currentRotation;
                NextRotation = nextRotation;
                RotationHistory = new ObservableCollection<RotationAssignment>(rotationHistory);

            }
            catch (Exception ex)
            {
                ErrorMessage = $"{ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }

        }
       
    }
}
