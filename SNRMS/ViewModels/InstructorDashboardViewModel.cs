using CommunityToolkit.Mvvm.ComponentModel;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Threading.Tasks;
using System.Linq;
namespace SNRMS.ViewModels
{
    public partial class InstructorDashboardViewModel : ObservableObject
    {
        private readonly StudentService _studentService;
        private readonly RotationService _rotationService;
        private readonly GroupService _groupService;
        private readonly SectionService _sectionService;
        private readonly HospitalService _hospitalService;
        private readonly AttendanceService _attendanceService;
        private readonly UserService _userService;

        public InstructorDashboardViewModel()
        {
            _studentService = new StudentService(App.Database);
            _rotationService = new RotationService(App.Database);
            _groupService = new GroupService(App.Database);
            _sectionService = new SectionService(App.Database);
            _hospitalService = new HospitalService(App.Database);
            _attendanceService = new AttendanceService(App.Database);
            _userService = new UserService(App.Database);
            HasNoAttendanceRecords = true;
        }

        //GROUPS ---------------------

        [ObservableProperty] public partial Group? SelectedGroup { get; set; }
        [ObservableProperty] public partial string GroupName { get; set; } = string.Empty;
        [ObservableProperty] public partial Group? FilterGroup { get; set; }
        [ObservableProperty] public partial Group? StudentGroupFilter { get; set; }
        [ObservableProperty] public partial Group? GroupToTransfer { get; set; }
        [ObservableProperty] public partial Student? SelectedGroupMember { get; set; }

        //EDIT STUDENT ---------------------
        [ObservableProperty] public partial string EditStudentFirstName { get; set; } = string.Empty;
        [ObservableProperty] public partial string EditStudentLastName { get; set; } = string.Empty;
        [ObservableProperty]  public partial string EditStudentEmail { get; set; } = string.Empty;
        [ObservableProperty] public partial string EditStudentNumber { get; set; } = string.Empty;

        public async Task SaveStudentEditAsync()
        {
            if (SelectedStudent == null) return;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = true;
            try
            {
                var updated = await _studentService.UpdateStudentAsync(
                    SelectedStudent.StudentId,
                    EditStudentFirstName,
                    EditStudentLastName,
                    EditStudentEmail,
                    EditStudentNumber);
                await LoadDataAsync();
                SuccessMessage = "Student information updated successfully.";
            }
            finally { IsLoading = false; }
        }

        //STUDENTS ---------------------
        [ObservableProperty] public partial Student? SelectedStudent { get; set; }
        [ObservableProperty] public partial string StudentFirstName { get; set; } = string.Empty;
        [ObservableProperty] public partial string StudentLastName { get; set; } = string.Empty;
        [ObservableProperty] public partial string StudentEmail { get; set; } = string.Empty;
        [ObservableProperty] public partial string StudentNumber { get; set; } = string.Empty;
        [ObservableProperty]  public partial string StudentSearchQuery { get; set; } = string.Empty;

        //ROTATIONS ---------------------
        [ObservableProperty] public partial RotationAssignment? SelectedRotationAssignment { get; set; }
        [ObservableProperty] public partial string DaySlot { get; set; } = string.Empty;
        [ObservableProperty] public partial DateTimeOffset RotationStartDate { get; set; } = DateTimeOffset.Now;
        [ObservableProperty]  public partial DateTimeOffset RotationEndDate { get; set; } = DateTimeOffset.Now;
        [ObservableProperty]  public partial TimeSpan? RotationStartTime { get; set; }
        [ObservableProperty] public partial TimeSpan? RotationEndTime { get; set; }
        [ObservableProperty] public partial Station? SelectedStation { get; set; }
        [ObservableProperty] public partial Hospital? SelectedRotationHospital { get; set; }
        [ObservableProperty] public partial Group? FilterRotationGroup { get; set; }


        //COLLECTIONS ---------------------
        [ObservableProperty] public partial ObservableCollection<Hospital> Hospitals { get; set; } = new();

        [ObservableProperty] public partial ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();
        [ObservableProperty]  public partial ObservableCollection<Student> GroupMembers { get; set; } = new ObservableCollection<Student>();

        [ObservableProperty] public partial ObservableCollection<Student> StudentsToCreate { get; set; } = new ObservableCollection<Student>();
        [ObservableProperty] public partial ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();
        [ObservableProperty] public partial ObservableCollection<RotationAssignment> RotationAssignments { get; set; } = new ObservableCollection<RotationAssignment>();
        [ObservableProperty ] public partial ObservableCollection<Station> FilteredStations { get; set; } = new();
        [ObservableProperty] public partial ObservableCollection<Station> Stations { get; set; } = new ObservableCollection<Station>();

        public List<string> DaySlotsList { get; } = new List<string> { "Mon-Tue", "Wed-Thu", "Fri-Sat" };

        // ATTENDANCE ---------------------
        [ObservableProperty] public partial ObservableCollection<AttendanceRecord> AttendanceRecords { get; set; } = new();
        [ObservableProperty] public partial Group? AttendanceFilterGroup { get; set; }
        [ObservableProperty] public partial DateTimeOffset AttendanceFilterDate { get; set; } = DateTimeOffset.Now;
        [ObservableProperty] public partial bool HasNoAttendanceRecords { get; set; }

        //ANALYTICS ---------------------
        [ObservableProperty] public partial bool HasInstructorAnalytics { get; set; }
        [ObservableProperty] public partial string InstructorAnalyticsName { get; set; } = "No instructor selected";
        [ObservableProperty] public partial string InstructorAnalyticsSection { get; set; } = "No assigned section";
        [ObservableProperty] public partial int InstructorAnalyticsStudents { get; set; }
        [ObservableProperty] public partial int InstructorAnalyticsGroups { get; set; }
        [ObservableProperty] public partial int InstructorAnalyticsRotations { get; set; }
        [ObservableProperty] public partial string InstructorAnalyticsAttendanceRate { get; set; } = "0%";
        [ObservableProperty] public partial string NoRotationMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string SelectedOverallAttendanceRange { get; set; } = "This Month";
        [ObservableProperty] public partial string OverallAttendanceScopeDescription { get; set; } = "Attendance for the current month only";
        public List<string> OverallAttendanceRangeOptions { get; } = new() { "This Month", "All Time" };
        [ObservableProperty] public partial ISeries[] RotationsPerStationSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] RotationsPerStationXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] RotationsPerStationYAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial ISeries[] StudentsPerGroupSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] StudentsPerGroupXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] StudentsPerGroupYAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial ISeries[] AttendanceRateSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] AttendanceRateXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] AttendanceRateYAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial ISeries[] OverallAttendanceRateSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] OverallAttendanceRateXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] OverallAttendanceRateYAxes { get; set; } = Array.Empty<Axis>();

        //OTHERS ---------------------
        [ObservableProperty]  public partial bool IsLoading { get; set; }
        [ObservableProperty] public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string SuccessMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorDisplayName { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorSection { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorEmail { get; set; } = string.Empty;

        [ObservableProperty] public partial string InstructorEmployeeId { get; set; } = string.Empty;
        private bool _isAutofilled = false;
        private int? _foundStudentId = null;

        // ======================================= COMMANDS =======================================

        //LOAD DATA --------------------------------------------------
        [RelayCommand] public async Task LoadDataAsync()
        {
            var user = SessionManager.CurrentUser;
            if (user == null || user.InstructorId == null)
            {
                ErrorMessage = "No active session found. Please log in.";
                return;
            }
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (user.Instructor != null)
                {

                    InstructorDisplayName = $"{user.Instructor.FirstName} {user.Instructor.LastName}";
                    InstructorEmployeeId = $"ID: {user.Instructor.EmployeeId}";
                    InstructorEmail = $"{user.Instructor.Email}";
                    

                }
                else
                {
                    InstructorDisplayName = "Instructor Loaded (No Profile)";
                    InstructorEmployeeId = $"ID: {user.InstructorId}";
                }


                var allhospitals = await _hospitalService.GetAllHospitalsAsync();
                Hospitals.Clear();
                Stations.Clear();
                foreach (var hospital in allhospitals)
                {
                    Hospitals.Add(hospital);
                    foreach (var station in hospital.Stations)
                        Stations.Add(station);
                }

                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                Section? section = null;
                try
                {
                    section = await _sectionService.GetInstructorSectionAsync(instructorId);
                    InstructorSection = $"{section.SectionName} (Year {section.YearLevel})";
                }
                catch
                {
                    // if instructor has no assigned section yet, leave groups/rotations empty
                    Groups.Clear();
                    Students.Clear();
                    RotationAssignments.Clear();
                    ClearInstructorAnalyticsSummary();
                    return;
                }

                // Load groups for the instructor's section
                var groups = await _groupService.GetGroupBySectionAsync(section.SectionId);
                Groups.Clear();
                foreach (var group in groups)
                    Groups.Add(group);
                if (_isGroupsSorted)
                    SortGroupsByName();

                var students = await _studentService.GetStudentsBySectionAsync(section.SectionId);
                Students.Clear();
                foreach (var student in students)
                    Students.Add(student);

                // Load rotation assignments
                try
                {
                    var rotationassignments = await _rotationService.GetAssignmentBySection(section.SectionId);
                    RotationAssignments.Clear();
                    foreach (var assignment in rotationassignments)
                        RotationAssignments.Add(assignment);
                }
                catch
                {
                    RotationAssignments.Clear();
                }

                await LoadInstructorAnalyticsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task LoadGroupsAsync()
        {
            if (SelectedGroup == null)
            {
                ErrorMessage = "Please select a group to load.";
                return;
            }
            IsLoading = true;
            try
            {
                var students = await _studentService.GetStudentsByGroupAsync(SelectedGroup.GroupId);
                Students.Clear();
                foreach (var student in students)
                {
                    Students.Add(student);
                }
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                var rotationAssignments = await _rotationService.GetAssignmentBySection(SelectedGroup.GroupId);
                RotationAssignments.Clear();
                foreach (var assignment in rotationAssignments)
                {
                    RotationAssignments.Add(assignment);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while loading group data: {ex.Message}";
            }
            finally { IsLoading = false; }

        }
        [RelayCommand] public async Task LoadFilteredAttendanceAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                if (AttendanceFilterGroup == null)
                {
                    ErrorMessage = "Please select a group.";
                    return;
                }

                var date = DateOnly.FromDateTime(AttendanceFilterDate.Date);
                var records = await _attendanceService.GetAttendanceByGroupAndDateAsync(
                    AttendanceFilterGroup.GroupId, date);

                AttendanceRecords.Clear();
                foreach (var record in records)
                    AttendanceRecords.Add(record);

                HasNoAttendanceRecords = AttendanceRecords.Count == 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading attendance: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task LoadAllAttendanceAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                List<AttendanceRecord> records;

                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                records = await _attendanceService.GetAttendanceBySectionAndDateAsync(
                    section.SectionId,
                    DateOnly.FromDateTime(AttendanceFilterDate.Date));
                AttendanceRecords.Clear();
                foreach (var record in records)
                    AttendanceRecords.Add(record);
                if (AttendanceRecords.Count == 0)
                {
                    HasNoAttendanceRecords = true;
                }
                else HasNoAttendanceRecords = false;


            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading attendance: {ex.Message}";
            }
            finally { IsLoading = false; }

        }
        //GROUP MANAGEMENT --------------------------------------------------
        [RelayCommand] public async Task CreateGroupAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (string.IsNullOrEmpty(GroupName))
            {
                ErrorMessage = "Group name is required.";
                return;
            }
            try
            {
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                var group = await _groupService.CreateGroupAsync(GroupName, section.SectionId);
                if (group != null)
                {
                    Groups.Add(group);
                    GroupName = string.Empty;
                }
                SuccessMessage = "Group created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating the group: {ex.Message}";

            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task DeleteGroupAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = true;
            if (SelectedGroup == null)
            {
                ErrorMessage = "Please select a group to delete.";
                return;
            }
            try
            {
                var deletedGroup = await _groupService.DeleteGroupAsync(SelectedGroup.GroupId);
                if (deletedGroup != null)
                {
                    Groups.Remove(SelectedGroup);
                    SelectedGroup = null;
                }
                SuccessMessage = "Group deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting the group: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task GetGroupByIdAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var group = await _groupService.GetGroupByIdAsync(SelectedGroup!.GroupId);
                if (group != null)
                {
                    SelectedGroup = group;
                    var students = await _studentService.GetStudentsByGroupAsync(group.GroupId);
                    Students.Clear();
                    foreach (var student in students)
                    {
                        Students.Add(student);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while retrieving the group: {ex.Message}";
            }
            finally { IsLoading = false; }

        }
        [RelayCommand] public void SortGroupsByName()
        {
            _isGroupsSorted = true;
            var sorted = Groups.OrderBy(g => g.GroupName).ToList();
            Groups.Clear();
            foreach (var group in sorted)
                Groups.Add(group);
        }
        [RelayCommand]  public async Task GetGroupBySectionAsync()
        {
            IsLoading = true;
            try
            {
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                var groups = await _groupService.GetGroupBySectionAsync(section.SectionId);
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while retrieving groups by section: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        partial void OnSelectedGroupChanged(Group? value)
        {
            GroupMembers.Clear();
            if (value != null)
            {
                foreach (var member in Students.Where(s => s.GroupId == value.GroupId))
                    GroupMembers.Add(member);
            }
        }

        private bool _isGroupsSorted = false;

        //STUDENT MANAGEMENT --------------------------------------------------
        [RelayCommand] public async Task CreateStudentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                Section? section = null;
                try
                {
                    section = await _sectionService.GetInstructorSectionAsync(instructorId);
                }
                catch
                {
                    ErrorMessage = "Error: You cannot add students until an Admin assigns you to a Section.";
                    return;
                }

                if (section == null)
                {
                    ErrorMessage = "Error: No assigned section found for your account.";
                    return;
                }
                var existingStudent = await _studentService.GetStudentByNumberAsync(StudentNumber);

                if (existingStudent != null)
                {
                    // Scenario A: Student is Active AND already belongs to a section
                    if (!existingStudent.IsArchived && existingStudent.SectionId != null)
                    {
                        // If they already belong to THIS instructor's section, just show them
                        if (existingStudent.SectionId == section.SectionId)
                        {
                            if (!Students.Any(s => s.StudentId == existingStudent.StudentId))
                                Students.Add(existingStudent);
                            ErrorMessage = $"Student {existingStudent.FirstName} {existingStudent.LastName} is already in your section.";
                            return;
                        }
                        // Belongs to a different section — block
                        ErrorMessage = "Error: Student belongs to another section and is currently active.";
                        return;
                    }

                    // Scenario B: Student is Archived OR active with no section — Reactivate
                    var reactivated = await _studentService.ReactivateStudentAsync(existingStudent.StudentId, section.SectionId);
                    if (!Students.Any(s => s.StudentId == reactivated.StudentId))
                        Students.Add(reactivated);
                    SuccessMessage = $"Student {reactivated.FirstName} {reactivated.LastName} has been re-enrolled. Assign them to a group.";
                    ClearAutofill();
                    IsLoading = false;
                    return;
                }
                else
                {
                    var student = await _studentService.CreateStudentAsync(StudentFirstName, StudentLastName, StudentEmail, StudentNumber, section.SectionId);
                    if (student != null)
                    {
                        Students.Add(student);
                        StudentFirstName = string.Empty;
                        StudentLastName = string.Empty;
                        StudentEmail = string.Empty;
                        StudentNumber = string.Empty;
                    }
                    SuccessMessage = "Student created successfully.";

                }

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating the student: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        
        [RelayCommand] public async Task DeleteStudentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedStudent == null)
            {
                ErrorMessage = "Please select a student to delete.";
                return;
            }
            try
            {
                await _studentService.DeleteStudentAsync(SelectedStudent.StudentId);
                
                Students.Remove(SelectedStudent);
                SelectedStudent = null;

                await LoadDataAsync();
                SuccessMessage = "Student deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting the student: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task TransferStudentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedStudent == null || GroupToTransfer == null)
            {
                ErrorMessage = "Please select a student and a group to transfer.";
                IsLoading = false;
                return;
            }
            try
            {
                await _studentService.TransferStudentAsync(SelectedStudent.StudentId, GroupToTransfer.GroupId);

                var updatedStudent = await _studentService.GetStudentByIdAsync(SelectedStudent.StudentId);

                var index = Students.IndexOf(SelectedStudent);
                if (index != -1 && updatedStudent != null)
                {
                    Students[index] = updatedStudent;
                }

                

                SuccessMessage = $"Transferred {updatedStudent.FirstName} to {GroupToTransfer.GroupName}.";

                GroupToTransfer = null;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while transferring the student: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task SearchStudentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                var students = await _studentService.SearchStudentsAsync(StudentSearchQuery);
                Students.Clear();
                foreach (var student in students.Where(s => s.Group == null || s.Group.SectionId == section.SectionId))
                    Students.Add(student);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while searching for students: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task FilterStudentsByGroupAsync()
        {
            if (StudentGroupFilter == null) return;
            IsLoading = true;
            try
            {
                var students = await _studentService.GetStudentsByGroupAsync(StudentGroupFilter.GroupId);
                Students.Clear();
                foreach (var student in students)
                {
                    Students.Add(student);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error filtering students: {ex.Message}";
            }
            finally { IsLoading = false; }
        }

        //ROTATION MANAGEMENT --------------------------------------------------
        [RelayCommand] public async Task CreateRotationAssignmentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedGroup == null || SelectedStation == null || string.IsNullOrEmpty(DaySlot))
            {
                ErrorMessage = "Please select a group, station, and day slot to create a rotation assignment.";
                return;
            }
            if (RotationStartTime == null || RotationEndTime == null)
            {
                ErrorMessage = "Please select start and end time.";
                return;
            }

            try
            {
                var rotationAssignment = await _rotationService
                    .CreateRotationAssignmentAsync(SelectedGroup.GroupId, SelectedStation.StationId, DaySlot,
                                                    DateOnly.FromDateTime(RotationStartDate.Date),
                                                    DateOnly.FromDateTime(RotationEndDate.Date),
                                                    TimeOnly.FromTimeSpan(RotationStartTime ?? TimeSpan.Zero),
                                                    TimeOnly.FromTimeSpan(RotationEndTime ?? TimeSpan.Zero));
                if (rotationAssignment != null)
                {
                    RotationAssignments.Add(rotationAssignment);
                    DaySlot = string.Empty;
                    SelectedGroup = null;
                    SelectedRotationHospital = null;
                    SelectedStation = null;
                    FilteredStations.Clear();
                    RotationStartDate = DateTimeOffset.Now;
                    RotationEndDate = DateTimeOffset.Now;
                    RotationStartTime = null;
                    RotationEndTime = null;
                }
                await LoadDataAsync();
                SuccessMessage = "Rotation assignment created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating the rotation assignment: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task DeleteRotationAssignmentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedRotationAssignment == null)
            {
                ErrorMessage = "Please select a rotation assignment to delete.";
                return;
            }
            try
            {
                var deletedAssignment = await _rotationService.DeleteRotationAssignmentAsync(SelectedRotationAssignment.RotationAssignmentId);
                if (deletedAssignment != null)
                {
                    RotationAssignments.Remove(SelectedRotationAssignment);
                    SelectedRotationAssignment = null;
                }
                SuccessMessage = "Rotation assignment deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting the rotation assignment: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task GetRotationAssignmentByGroup()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                List<RotationAssignment> assignments;

                if (FilterRotationGroup == null)
                {
                    var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                    var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                    try { assignments = await _rotationService.GetAssignmentBySection(section.SectionId); }
                    catch { assignments = new List<RotationAssignment>(); }
                }
                else
                {
                    assignments = await _rotationService.GetAssignmentByGroupAsync(FilterRotationGroup.GroupId);
                }

                RotationAssignments.Clear();
                foreach (var a in assignments)
                    RotationAssignments.Add(a);
            }
            catch (Exception ex)
            {
                RotationAssignments.Clear();
                ErrorMessage = ex.Message;
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] public async Task GetAllRotationAssignmentsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                List<RotationAssignment> assignments;
                try { assignments = await _rotationService.GetAssignmentBySection(section.SectionId); }
                catch { assignments = new List<RotationAssignment>(); }
                RotationAssignments.Clear();
                foreach (var a in assignments)
                    RotationAssignments.Add(a);
            }
            catch (Exception ex)
            {
                RotationAssignments.Clear();
                ErrorMessage = $"An error occurred while retrieving rotation assignments: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        partial void OnSelectedRotationHospitalChanged(Hospital? value)
        {
            FilteredStations.Clear();
            if (value != null)
                foreach (var station in Stations.Where(s => s.HospitalId == value.HospitalId && !s.IsArchived))
                    FilteredStations.Add(station);
        }

        //ACCOUNT MANAGEMENT --------------------------------------------------
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
        partial void OnStudentNumberChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ClearAutofill();
                return;
            }

            _ = LookupStudentAsync(value);
        }
        private async Task LookupStudentAsync(string number)
        {
            var student = await _studentService.GetStudentByNumberAsync(number);

            if (student != null)
            {
                StudentFirstName = student.FirstName;
                StudentLastName = student.LastName;
                StudentEmail = student.Email;
                _foundStudentId = student.StudentId;
                _isAutofilled = true;

                if (!student.IsArchived)
                {
                    ErrorMessage = $"Note: This student is already active in {student.Group?.Section?.SectionName ?? "another section"}.";
                }
            }
            else
            {
                // Only clear names if we were previously in an autofilled state
                if (_isAutofilled) ClearAutofill();
            }
        }
        private void ClearAutofill()
        {
            _isAutofilled = false;
            _foundStudentId = null;
            StudentFirstName = string.Empty;
            StudentLastName = string.Empty;
            StudentEmail = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand] public async Task LoadInstructorAnalyticsAsync()
        {
            var user = SessionManager.CurrentUser;
            if (user == null || user.InstructorId == null)
            {
                ErrorMessage = "No active session found. Please log in.";
                ClearInstructorAnalyticsSummary();
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var instructorId = user.InstructorId.Value;
                var section = await App.Database.Sections
                    .Include(s => s.Groups).ThenInclude(g => g.Students.Where(st => !st.IsArchived))
                    .Include(s => s.Groups).ThenInclude(g => g.RotationAssignments.Where(r => !r.IsArchived))
                        .ThenInclude(ra => ra.Station).ThenInclude(s => s.Hospital)
                    .FirstOrDefaultAsync(s => s.InstructorId == instructorId);

                if (section == null)
                {
                    ClearInstructorAnalyticsSummary();
                    ErrorMessage = "You do not have an assigned section yet.";
                    return;
                }

                var activeGroups = section.Groups.Where(g => !g.IsArchived).OrderBy(g => g.GroupName).ToList();
                InstructorAnalyticsName = string.IsNullOrWhiteSpace(InstructorDisplayName)
                    ? "Instructor"
                    : InstructorDisplayName;
                InstructorAnalyticsSection = $"{section.SectionName} | Year {section.YearLevel}";
                InstructorAnalyticsGroups = activeGroups.Count;
                InstructorAnalyticsStudents = activeGroups.Sum(g => g.Students.Count);
                InstructorAnalyticsRotations = activeGroups.Sum(g => g.RotationAssignments.Count);

                var rotationsByStation = activeGroups
                    .SelectMany(g => g.RotationAssignments)
                    .GroupBy(r => new
                    {
                        Station = r.Station?.StationName ?? "Unknown",
                        Hospital = r.Station?.Hospital?.HospitalName ?? string.Empty
                    })
                    .OrderByDescending(g => g.Count())
                    .ToList();

                var stationLabels = rotationsByStation
                    .Select(g =>
                    {
                        if (string.IsNullOrEmpty(g.Key.Hospital)) return g.Key.Station;
                        var abbrev = string.Concat(g.Key.Hospital
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(w => w[0]))
                            .ToUpper();
                        return $"{g.Key.Station} ({abbrev})";
                    })
                    .ToArray();

                RotationsPerStationSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Rotations",
                        Values = rotationsByStation.Select(g => (double)g.Count()).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(27, 58, 107)),
                        MaxBarWidth = 40
                    }
                };
                RotationsPerStationXAxes = new Axis[]
                {
                    new Axis { Labels = stationLabels, LabelsRotation = -25, TextSize = 10, MinStep = 1, ForceStepToMin = true }
                };
                RotationsPerStationYAxes = new Axis[]
                {
                    new Axis { Name = "Rotations", MinLimit = 0, TextSize = 11 }
                };

                var groupLabels = activeGroups.Select(g => g.GroupName).ToArray();
                StudentsPerGroupSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Students",
                        Values = activeGroups.Select(g => (double)g.Students.Count).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(56, 161, 105)),
                        MaxBarWidth = 40
                    }
                };
                StudentsPerGroupXAxes = new Axis[] { new Axis { Labels = groupLabels, TextSize = 11 } };
                StudentsPerGroupYAxes = new Axis[] { new Axis { Name = "Students", MinLimit = 0, TextSize = 11 } };

                var today = DateOnly.FromDateTime(DateTime.Today);
                var attendanceLabels = new List<string>();
                var attendanceValues = new List<double>();
                var noRotationGroups = new List<string>();

                int GetScheduledDaysElapsed(RotationAssignment rotation, DateOnly currentDate)
                {
                    var scheduledDays = rotation.DaySlot switch
                    {
                        "Mon-Tue" => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                        "Wed-Thu" => new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday },
                        "Fri-Sat" => new[] { DayOfWeek.Friday, DayOfWeek.Saturday },
                        _ => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
                    };

                    var count = 0;
                    var d = rotation.StartDate;
                    while (d <= currentDate && d <= rotation.EndDate)
                    {
                        if (scheduledDays.Contains(d.DayOfWeek))
                            count++;
                        d = d.AddDays(1);
                    }
                    return count;
                }

                int GetScheduledDaysInRange(RotationAssignment rotation, DateOnly rangeStart, DateOnly rangeEnd)
                {
                    var scheduledDays = rotation.DaySlot switch
                    {
                        "Mon-Tue" => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                        "Wed-Thu" => new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday },
                        "Fri-Sat" => new[] { DayOfWeek.Friday, DayOfWeek.Saturday },
                        _ => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
                    };

                    var start = rotation.StartDate > rangeStart ? rotation.StartDate : rangeStart;
                    var end = rotation.EndDate < rangeEnd ? rotation.EndDate : rangeEnd;
                    if (start > end) return 0;

                    var count = 0;
                    var d = start;
                    while (d <= end)
                    {
                        if (scheduledDays.Contains(d.DayOfWeek))
                            count++;
                        d = d.AddDays(1);
                    }
                    return count;
                }

                foreach (var group in activeGroups)
                {
                    var relevantRotation = group.RotationAssignments
                        .Where(r => r.StartDate <= today)
                        .OrderByDescending(r => r.StartDate)
                        .FirstOrDefault();

                    if (relevantRotation == null)
                    {
                        noRotationGroups.Add(group.GroupName);
                        continue;
                    }

                    var scheduledDaysElapsed = GetScheduledDaysElapsed(relevantRotation, today);
                    var totalExpected = group.Students.Count * Math.Max(scheduledDaysElapsed, 1);

                    var totalAttended = await App.Database.AttendanceRecords
                        .Where(a => a.RotationAssignmentId == relevantRotation.RotationAssignmentId)
                        .Select(a => new { a.StudentId, a.DateToday })
                        .Distinct()
                        .CountAsync();

                    var rate = totalExpected > 0
                        ? Math.Round((totalAttended / (double)totalExpected) * 100, 1)
                        : 0;

                    attendanceLabels.Add(group.GroupName);
                    attendanceValues.Add(rate);
                }

                AttendanceRateSeries = new ISeries[]
                {
                    new ColumnSeries<double?>
                    {
                        Name = "80%+",
                        Values = attendanceValues.Select(v => v >= 80 ? v : (double?)null).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(56, 161, 105)),
                        MaxBarWidth = 40
                    },
                    new ColumnSeries<double?>
                    {
                        Name = "Below 80%",
                        Values = attendanceValues.Select(v => v < 80 ? v : (double?)null).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(229, 62, 62)),
                        MaxBarWidth = 40
                    },
                    new LineSeries<double?>
                    {
                        Name = "80% Target",
                        Values = attendanceLabels.Select(_ => (double?)80).ToArray(),
                        Stroke = new SolidColorPaint(new SKColor(237, 137, 54)) { StrokeThickness = 2 },
                        Fill = null,
                        GeometrySize = 0,
                        LineSmoothness = 0
                    }
                };
                AttendanceRateXAxes = new Axis[] { new Axis { Labels = attendanceLabels.ToArray(), TextSize = 11 } };
                AttendanceRateYAxes = new Axis[] { new Axis { Name = "Attendance %", MinLimit = 0, MaxLimit = 100, TextSize = 11 } };
                NoRotationMessage = noRotationGroups.Count > 0
                    ? $"No started rotation: {string.Join(", ", noRotationGroups)}"
                    : string.Empty;

                var overallLabels = new List<string>();
                var overallValues = new List<double>();
                var overallTrends = new List<string>();
                var overallRangeStart = SelectedOverallAttendanceRange == "This Month"
                    ? new DateOnly(today.Year, today.Month, 1)
                    : DateOnly.MinValue;
                var overallRangeEnd = today;
                OverallAttendanceScopeDescription = SelectedOverallAttendanceRange == "This Month"
                    ? $"Month-to-date attendance, {overallRangeEnd:MMMM yyyy}"
                    : "Cumulative attendance across all started rotations";

                foreach (var group in activeGroups)
                {
                    var pastAndCurrentRotations = group.RotationAssignments
                        .Where(r => r.StartDate <= overallRangeEnd && r.EndDate >= overallRangeStart)
                        .ToList();

                    if (!pastAndCurrentRotations.Any())
                    {
                        overallLabels.Add(group.GroupName);
                        overallValues.Add(0);
                        overallTrends.Add("→");
                        continue;
                    }

                    var totalScheduledDays = pastAndCurrentRotations
                        .Sum(r => GetScheduledDaysInRange(r, overallRangeStart, overallRangeEnd));
                    var totalExpectedOverall = group.Students.Count * totalScheduledDays;
                    var rotationIds = pastAndCurrentRotations.Select(r => r.RotationAssignmentId).ToList();
                    var totalAttendedOverall = await App.Database.AttendanceRecords
                        .Where(a => rotationIds.Contains(a.RotationAssignmentId)
                                    && a.DateToday >= overallRangeStart
                                    && a.DateToday <= overallRangeEnd)
                        .Select(a => new { a.StudentId, a.DateToday })
                        .Distinct()
                        .CountAsync();

                    var overallRate = totalExpectedOverall > 0
                        ? Math.Round((totalAttendedOverall / (double)totalExpectedOverall) * 100, 1)
                        : 0;

                    overallLabels.Add(group.GroupName);
                    overallValues.Add(overallRate);

                    var attendanceIndex = attendanceLabels.IndexOf(group.GroupName);
                    var currentRate = attendanceIndex >= 0 ? attendanceValues[attendanceIndex] : -1;
                    var trend = currentRate < 0 ? "→"
                        : currentRate > overallRate + 2 ? "▲"
                        : currentRate < overallRate - 2 ? "▼"
                        : "→";
                    overallTrends.Add(trend);
                }

                OverallAttendanceRateSeries = new ISeries[]
                {
                    new ColumnSeries<double?>
                    {
                        Name = "80%+",
                        Values = overallValues.Select(v => v >= 80 ? v : (double?)null).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(56, 161, 105)),
                        MaxBarWidth = 40
                    },
                    new ColumnSeries<double?>
                    {
                        Name = "Below 80%",
                        Values = overallValues.Select(v => v < 80 ? v : (double?)null).ToArray(),
                        Fill = new SolidColorPaint(new SKColor(229, 62, 62)),
                        MaxBarWidth = 40
                    },
                    new LineSeries<double?>
                    {
                        Name = "80% Target",
                        Values = overallLabels.Select(_ => (double?)80).ToArray(),
                        Stroke = new SolidColorPaint(new SKColor(27, 58, 107)) { StrokeThickness = 2 },
                        Fill = null,
                        GeometrySize = 0,
                        LineSmoothness = 0
                    }
                };
                OverallAttendanceRateXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = overallLabels.Select((label, index) => $"{label} {overallTrends[index]}").ToArray(),
                        TextSize = 11,
                        LabelsRotation = -15,
                        MinStep = 1,
                        ForceStepToMin = true
                    }
                };
                OverallAttendanceRateYAxes = new Axis[] { new Axis { Name = "Attendance %", MinLimit = 0, MaxLimit = 100, TextSize = 11 } };
                InstructorAnalyticsAttendanceRate = $"{(overallValues.Count > 0 ? overallValues.Average() : 0):0.#}%";
                HasInstructorAnalytics = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load analytics: {ex.Message}";
                ClearInstructorAnalyticsSummary();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearInstructorAnalyticsSummary()
        {
            HasInstructorAnalytics = false;
            InstructorAnalyticsName = string.IsNullOrWhiteSpace(InstructorDisplayName) ? "Instructor" : InstructorDisplayName;
            InstructorAnalyticsSection = "No assigned section";
            InstructorAnalyticsStudents = 0;
            InstructorAnalyticsGroups = 0;
            InstructorAnalyticsRotations = 0;
            InstructorAnalyticsAttendanceRate = "0%";
            NoRotationMessage = string.Empty;
            RotationsPerStationSeries = Array.Empty<ISeries>();
            StudentsPerGroupSeries = Array.Empty<ISeries>();
            AttendanceRateSeries = Array.Empty<ISeries>();
            OverallAttendanceRateSeries = Array.Empty<ISeries>();
        }

        partial void OnSelectedOverallAttendanceRangeChanged(string value)
        {
            if (HasInstructorAnalytics)
                _ = LoadInstructorAnalyticsAsync();
        }

        //SESSION MANAGEMENT --------------------------------------------------
        [RelayCommand] public void Logout()
        {
            SNRMS.Core.Services.SessionManager.Logout();
            if (App.RootFrame != null)
            {
                App.RootFrame.Navigate(typeof(SNRMS.View.LoginPage));

                App.RootFrame.BackStack.Clear();
            }
        }

    }
}
