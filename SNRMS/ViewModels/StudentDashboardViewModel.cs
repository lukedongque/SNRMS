using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SNRMS.Core.Models;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SNRMS.ViewModels
{
    public partial class StudentDashboardViewModel : ObservableObject
    {
        private readonly RotationService _rotationService;
        private readonly StudentService _studentService;
        private readonly AttendanceService _attendanceService;
        private readonly UserService _userService;

        public StudentDashboardViewModel()
        {
            _rotationService = new RotationService(App.Database);
            _studentService = new StudentService(App.Database);
            _attendanceService = new AttendanceService(App.Database);
            _userService = new UserService(App.Database);
        }

        //STUDENT -----------------------------------------------------------------------
        [ObservableProperty]
        public partial string StudentDisplayName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentNumber { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentGroupName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentSectionName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial int StudentYearLevel { get; set; }
        [ObservableProperty]
        public partial bool AlreadyMarkedAttendance { get; set; }

        [ObservableProperty]
        public partial string AttendanceStatusMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool AlreadyClockedOut { get; set; }

        [ObservableProperty]
        public partial string ClockOutStatusMessage { get; set; } = string.Empty;

        // ROTATION ASSIGNMENT ----------------------------------------------------------
        [ObservableProperty]
        public partial RotationAssignment? CurrentRotation { get; set; }
        [ObservableProperty]
        public partial RotationAssignment? NextRotation { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<RotationAssignment> RotationHistory { get; set; } = new();
        [ObservableProperty]
        public partial DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        // SCHEDULE ------------------------------------------------------------------
        [ObservableProperty]
        public partial ObservableCollection<RotationAssignment> ScheduleAssignments { get; set; } = new();

        // Status flags
        [ObservableProperty]
        public partial bool HasCurrentRotation { get; set; }
        [ObservableProperty]
        public partial bool HasNextRotation { get; set; }
        [ObservableProperty]
        public partial bool HasSchedule { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string SuccessMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        // Computed display strings for CurrentRotation card
        public string CurrentHospitalName => CurrentRotation?.Station?.Hospital?.HospitalName ?? "—";
        public string CurrentStationName => CurrentRotation?.Station?.StationName ?? "—";
        public string CurrentDaySlot => CurrentRotation?.DaySlot ?? "—";
        public string CurrentStartTime => CurrentRotation?.StartTime.ToString("hh:mm tt") ?? "—";
        public string CurrentEndTime => CurrentRotation?.EndTime.ToString("hh:mm tt") ?? "—";
        public string CurrentDateRange => CurrentRotation != null
            ? $"{CurrentRotation.StartDate:MMM dd, yyyy}  –  {CurrentRotation.EndDate:MMM dd, yyyy}"
            : "—";

        // Attendance button state
        public bool CanMarkAttendance => HasCurrentRotation && !AlreadyMarkedAttendance;
        public bool CanClockOut => HasCurrentRotation && AlreadyMarkedAttendance && !AlreadyClockedOut;

        partial void OnHasCurrentRotationChanged(bool value)
        {
            OnPropertyChanged(nameof(CanMarkAttendance));
            OnPropertyChanged(nameof(CanClockOut));
        }
        partial void OnAlreadyMarkedAttendanceChanged(bool value)
        {
            OnPropertyChanged(nameof(CanMarkAttendance));
            OnPropertyChanged(nameof(CanClockOut));
        }
        partial void OnAlreadyClockedOutChanged(bool value)
        {
            OnPropertyChanged(nameof(CanClockOut));
        }

        // Computed display strings for NextRotation card
        public string NextHospitalName => NextRotation?.Station?.Hospital?.HospitalName ?? "—";
        public string NextStationName => NextRotation?.Station?.StationName ?? "—";
        public string NextDaySlot => NextRotation?.DaySlot ?? "—";
        public string NextStartTime => NextRotation?.StartTime.ToString("hh:mm tt") ?? "—";
        public string NextEndTime => NextRotation?.EndTime.ToString("hh:mm tt") ?? "—";
        public string NextDateRange => NextRotation != null
            ? $"{NextRotation.StartDate:MMM dd, yyyy}  –  {NextRotation.EndDate:MMM dd, yyyy}"
            : "—";


        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var studentId = SessionManager.CurrentUser!.StudentId!.Value;

                // Load student info for navbar
                var student = await _studentService.GetStudentByIdAsync(studentId);
                if (student != null)
                {
                    StudentDisplayName = $"{student.FirstName} {student.LastName}";
                    StudentNumber = student.StudentNumber;
                    StudentGroupName = student.Group?.GroupName ?? "No Group";
                    StudentSectionName = student.Group?.Section?.SectionName ?? "No Section";
                    StudentYearLevel = student.Group?.Section?.YearLevel ?? 0;
                }

                // Load current rotation (safe — no throw)
                try
                {
                    CurrentRotation = await _rotationService.GetCurrentRotationAssignmentAsync(studentId);
                    HasCurrentRotation = CurrentRotation != null;
                }
                catch
                {
                    CurrentRotation = null;
                    HasCurrentRotation = false;
                }
                OnPropertyChanged(nameof(CurrentHospitalName));
                OnPropertyChanged(nameof(CurrentStationName));
                OnPropertyChanged(nameof(CurrentDaySlot));
                OnPropertyChanged(nameof(CurrentStartTime));
                OnPropertyChanged(nameof(CurrentEndTime));
                OnPropertyChanged(nameof(CurrentDateRange));

                // Load next rotation (safe)
                NextRotation = await _rotationService.GetNextRotationAssignment(studentId);
                // Skip if it's the same as current
                if (NextRotation != null && CurrentRotation != null &&
                    NextRotation.RotationAssignmentId == CurrentRotation.RotationAssignmentId)
                    NextRotation = null;
                HasNextRotation = NextRotation != null;
                OnPropertyChanged(nameof(NextHospitalName));
                OnPropertyChanged(nameof(NextStationName));
                OnPropertyChanged(nameof(NextDaySlot));
                OnPropertyChanged(nameof(NextDateRange));

                // Load rotation history
                try
                {
                    var history = await _rotationService.GetStudentRotationHistoryAsync(studentId);
                    RotationHistory = new ObservableCollection<RotationAssignment>(history);

                    var allAssignments = await _rotationService.GetStudentAllAssignmentsAsync(studentId); 
                    ScheduleAssignments = new ObservableCollection<RotationAssignment>(
                        allAssignments
                            .Where(r => r.EndDate >= Today)
                            .OrderBy(r => r.StartDate)
                            .ToList());
                    HasSchedule = ScheduleAssignments.Count > 0;

                    if (CurrentRotation != null)
                    {
                        AlreadyMarkedAttendance = await _attendanceService.HasAttendanceTodayAsync(
                            SessionManager.CurrentUser!.StudentId!.Value,
                            CurrentRotation.RotationAssignmentId);
                        AlreadyClockedOut = await _attendanceService.HasClockedOutTodayAsync(
                            SessionManager.CurrentUser!.StudentId!.Value,
                            CurrentRotation.RotationAssignmentId);

                        if (AlreadyMarkedAttendance)
                            AttendanceStatusMessage = "Attendance already marked for today.";
                        if (AlreadyClockedOut)
                            ClockOutStatusMessage = "Already clocked out for today.";

                        if (AlreadyMarkedAttendance)
                            AttendanceStatusMessage = "Attendance already marked for today.";
                    }
                }
                catch
                {
                    RotationHistory.Clear();
                    ScheduleAssignments.Clear();
                    HasSchedule = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load dashboard: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void Logout()
        {
            SessionManager.Logout();
        }

        [RelayCommand]
        public async Task MarkAttendanceAsync()
        {
            if (CurrentRotation == null) return;

            var studentId = SessionManager.CurrentUser!.StudentId!.Value;

            try
            {
                var record = await _attendanceService.MarkAttendanceAsync(studentId, CurrentRotation.RotationAssignmentId);
                AlreadyMarkedAttendance = true;
                AttendanceStatusMessage = $"Attendance marked at {record!.TimeIn:hh:mm tt}";
            }
            catch (InvalidOperationException ex)
            {
                AttendanceStatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        public async Task ClockOutAsync()
        {
            if (CurrentRotation == null) return;

            var studentId = SessionManager.CurrentUser!.StudentId!.Value;

            try
            {
                var record = await _attendanceService.ClockOutAsync(studentId, CurrentRotation.RotationAssignmentId);
                AlreadyClockedOut = true;
                ClockOutStatusMessage = $"Clocked out at {record!.TimeOut:hh:mm tt}";
            }
            catch (InvalidOperationException ex)
            {
                ClockOutStatusMessage = ex.Message;
            }
        }

        public async Task ChangePasswordAsync(string currentPwd, string newPwd)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                var user = SessionManager.CurrentUser;
                if (user == null)
                {
                    throw new Exception("Session expired. Please log in again.");
                }

                
                var updatedUser = await _userService.ChangePasswordAsync(user.UserId, currentPwd, newPwd);

                if (updatedUser != null)
                {
                    SuccessMessage = "Your password has been updated successfully.";

                    SessionManager.CurrentUser.HasChangedPassword = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                throw;
            }
        }

    }
}
