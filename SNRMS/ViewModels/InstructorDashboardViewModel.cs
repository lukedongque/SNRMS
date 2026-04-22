using CommunityToolkit.Mvvm.ComponentModel;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
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

        public InstructorDashboardViewModel()
        {
            _studentService = new StudentService(App.Database);
            _rotationService = new RotationService(App.Database);
            _groupService = new GroupService(App.Database);
            _sectionService = new SectionService(App.Database);
            _hospitalService = new HospitalService(App.Database);
            _attendanceService = new AttendanceService(App.Database);
            HasNoAttendanceRecords = true;
        }

        //GROUPS ---------------------

        [ObservableProperty]
        public partial Group? SelectedGroup { get; set; }
        [ObservableProperty]
        public partial string GroupName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial Group? FilterGroup { get; set; }
        [ObservableProperty]
        public partial Group? StudentGroupFilter { get; set; }
        [ObservableProperty]
        public partial Group? GroupToTransfer { get; set; }
        [ObservableProperty] public partial Student? SelectedGroupMember { get; set; }

        //STUDENTS ---------------------
        [ObservableProperty]
        public partial Student? SelectedStudent { get; set; }
        [ObservableProperty]
        public partial string StudentFirstName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentLastName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentEmail { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentNumber { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StudentSearchQuery { get; set; } = string.Empty;

        //ROTATIONS ---------------------
        [ObservableProperty]
        public partial RotationAssignment? SelectedRotationAssignment { get; set; }
        [ObservableProperty]
        public partial string DaySlot { get; set; } = string.Empty;
        [ObservableProperty]
        public partial DateTimeOffset RotationStartDate { get; set; } = DateTimeOffset.Now;
        [ObservableProperty]
        public partial DateTimeOffset RotationEndDate { get; set; } = DateTimeOffset.Now;
        [ObservableProperty]
        public partial TimeSpan? RotationStartTime { get; set; }
        [ObservableProperty]
        public partial TimeSpan? RotationEndTime { get; set; }
        [ObservableProperty]
        public partial Station? SelectedStation { get; set; }
        [ObservableProperty]
        public partial Hospital? SelectedRotationHospital { get; set; }
        [ObservableProperty]
        public partial Group? FilterRotationGroup { get; set; }





        //COLLECTIONS ---------------------
        [ObservableProperty]
        public partial ObservableCollection<Hospital> Hospitals { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();
        [ObservableProperty]
        public partial ObservableCollection<Student> GroupMembers { get; set; } = new ObservableCollection<Student>();

        [ObservableProperty]
        public partial ObservableCollection<Student> StudentsToCreate { get; set; } = new ObservableCollection<Student>();
        [ObservableProperty]
        public partial ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();
        [ObservableProperty]
        public partial ObservableCollection<RotationAssignment> RotationAssignments { get; set; } = new ObservableCollection<RotationAssignment>();
        [ObservableProperty]
        public partial ObservableCollection<Station> FilteredStations { get; set; } = new();
        [ObservableProperty]
        public partial ObservableCollection<Station> Stations { get; set; } = new ObservableCollection<Station>();

        public List<string> DaySlotsList { get; } = new List<string> { "Mon-Tue", "Wed-Thu", "Fri-Sat" };

        // ATTENDANCE ---------------------
        [ObservableProperty]
        public partial ObservableCollection<AttendanceRecord> AttendanceRecords { get; set; } = new();
        [ObservableProperty]
        public partial RotationAssignment? AttendanceFilterRotation { get; set; }
        [ObservableProperty]
        public partial DateTimeOffset AttendanceFilterDate { get; set; } = DateTimeOffset.Now;
        [ObservableProperty]
        public partial bool HasNoAttendanceRecords { get; set; }

        //OTHERS ---------------------
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string SuccessMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorDisplayName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string InstructorEmployeeId { get; set; } = string.Empty;

        [RelayCommand]
        public void Logout()
        {
            SNRMS.Core.Services.SessionManager.Logout();
            if (App.RootFrame != null)
            {
                App.RootFrame.Navigate(typeof(SNRMS.View.LoginPage));

                App.RootFrame.BackStack.Clear();
            }
        }
        [RelayCommand]
        public async Task LoadDataAsync()
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
                }
                else
                {
                    InstructorDisplayName = "Instructor Loaded (No Profile)";
                    InstructorEmployeeId = $"ID: {user.InstructorId}";
                }


                // Always load hospitals and stations first — independent of section assignment
                var allhospitals = await _hospitalService.GetAllHospitalsAsync();
                Hospitals.Clear();
                Stations.Clear();
                foreach (var hospital in allhospitals)
                {
                    Hospitals.Add(hospital);
                    foreach (var station in hospital.Stations)
                        Stations.Add(station);
                }

                // Load section-dependent data separately so a missing section
                // does not prevent hospitals/stations from loading
                var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                Section? section = null;
                try
                {
                    section = await _sectionService.GetInstructorSectionAsync(instructorId);
                }
                catch
                {
                    // Instructor has no assigned section yet — leave groups/rotations empty
                    Groups.Clear();
                    Students.Clear();
                    RotationAssignments.Clear();
                    return;
                }

                // Load groups for the instructor's section
                var groups = await _groupService.GetGroupBySectionAsync(section.SectionId);
                Groups.Clear();
                foreach (var group in groups)
                    Groups.Add(group);
                if (_isGroupsSorted)
                    SortGroupsByName();

                // Load all students
                var students = await _studentService.GetAllStudentsAsync();
                Students.Clear();
                foreach (var student in students)
                    Students.Add(student);

                // Load rotation assignments — safely handle empty result
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

        [RelayCommand]
        public async Task LoadGroupsAsync()
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

        [RelayCommand]
        public async Task CreateGroupAsync()
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
        [RelayCommand]
        public async Task DeleteGroupAsync()
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
        [RelayCommand]
        public async Task CreateStudentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var student = await _studentService.CreateStudentAsync(StudentFirstName, StudentLastName, StudentEmail, StudentNumber);
                if (student != null)
                {
                    Students.Add(student);
                    StudentFirstName = string.Empty;
                    StudentLastName = string.Empty;
                    StudentEmail = string.Empty;
                    StudentNumber = string.Empty;
                }
                await LoadDataAsync();
                SuccessMessage = "Student created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating the student: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand]
        public async Task BulkCreateStudent()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = true;
            try
            {
                var bulkstudents = await _studentService.CreateBulkAccountStudentAsync(StudentsToCreate.ToList());
                foreach (var student in bulkstudents)
                {
                    Students.Add(student);
                }
                SuccessMessage = $"{bulkstudents.Count} students created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while bulk creating students: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
        [RelayCommand]
        public async Task DeleteStudentAsync()
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
        [RelayCommand]
        public async Task TransferStudentAsync()
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
        [RelayCommand]
        public async Task CreateRotationAssignmentAsync()
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
                    RotationStartDate = DateTimeOffset.Now;
                    RotationEndDate = DateTimeOffset.Now;
                    SelectedStation = null;
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

        [RelayCommand]
        public async Task DeleteRotationAssignmentAsync()
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
        [RelayCommand]
        public async Task GetGroupByIdAsync()
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
        private bool _isGroupsSorted = false;

        [RelayCommand]
        public void SortGroupsByName()
        {
            _isGroupsSorted = true;
            var sorted = Groups.OrderBy(g => g.GroupName).ToList();
            Groups.Clear();
            foreach (var group in sorted)
                Groups.Add(group);
        }

        [RelayCommand]
        public async Task GetGroupBySectionAsync()
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

        partial void OnSelectedRotationHospitalChanged(Hospital? value)
        {
            FilteredStations.Clear();
            if (value != null)
                foreach (var station in Stations.Where(s => s.HospitalId == value.HospitalId))
                    FilteredStations.Add(station);
        }

        [RelayCommand]
        public async Task SearchStudentAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var students = await _studentService.SearchStudentsAsync(StudentSearchQuery);
                Students.Clear();
                foreach (var student in students)
                    Students.Add(student);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while searching for students: {ex.Message}";
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task GetRotationAssignmentByGroup()
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

        [RelayCommand]
        public async Task GetAllRotationAssignmentsAsync()
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

        [RelayCommand]
        public async Task FilterStudentsByGroupAsync()
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


        partial void OnSelectedGroupChanged(Group? value)
        {
            GroupMembers.Clear();
            if (value != null)
            {
                foreach (var member in Students.Where(s => s.GroupId == value.GroupId))
                    GroupMembers.Add(member);
            }
        }

        [RelayCommand]
        public async Task LoadAttendanceAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                List<AttendanceRecord> records;
                if (AttendanceFilterRotation != null)
                {
                    records = await _attendanceService.GetAttendanceByRotationAsync(AttendanceFilterRotation.RotationAssignmentId, DateOnly.FromDateTime(AttendanceFilterDate.Date));
                }
                else
                {
                    var instructorId = SessionManager.CurrentUser!.InstructorId!.Value;
                    var section = await _sectionService.GetInstructorSectionAsync(instructorId);
                    records = await _attendanceService.GetAttendanceBySectionAndDateAsync(
                        section.SectionId,
                        DateOnly.FromDateTime(AttendanceFilterDate.Date));
                }
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
    }
}
