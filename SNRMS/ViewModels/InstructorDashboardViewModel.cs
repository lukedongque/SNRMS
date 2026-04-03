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

        public InstructorDashboardViewModel()
        {
            _studentService = new StudentService(App.Database);
            _rotationService = new RotationService(App.Database);
            _groupService = new GroupService(App.Database);
            _sectionService = new SectionService(App.Database);
            _hospitalService = new HospitalService(App.Database);
        }

        //GROUPS ---------------------

        [ObservableProperty]
        public partial Group? SelectedGroup { get; set; }
        [ObservableProperty]
        public partial string GroupName { get; set; } = string.Empty;

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

        //ROTATIONS ---------------------
        [ObservableProperty]
        public partial RotationAssignment? SelectedRotationAssignment { get; set; }
        [ObservableProperty]
        public partial string DaySlot { get; set; } = string.Empty;
        [ObservableProperty]
        public partial DateOnly RotationStartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        public partial DateOnly RotationEndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        public partial Station? SelectedStation { get; set; }


        //COLLECTIONS ---------------------
        [ObservableProperty]
        public partial ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();
        [ObservableProperty]
        public partial ObservableCollection<Student> StudentsToCreate { get; set; } = new ObservableCollection<Student>();
        [ObservableProperty]
        public partial ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();
        [ObservableProperty]
        public partial ObservableCollection<RotationAssignment> RotationAssignments { get; set; } = new ObservableCollection<RotationAssignment>();
        [ObservableProperty]
        public partial ObservableCollection<Station> Stations { get; set; } = new ObservableCollection<Station>();

        //OTHERS ---------------------
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;


        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
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
                var hospitals = await _hospitalService.GetAllHospitalsAsync();
                Stations.Clear();
                foreach (var hospital in hospitals)
                {
                    foreach (var station in hospital.Stations)
                    {
                        Stations.Add(station);
                    }
                }
                if (SelectedGroup != null)
                {
                    var students = await _studentService.GetStudentsByGroupAsync(SelectedGroup.GroupId);
                    Students.Clear();
                    foreach (var student in students)
                    {
                        Students.Add(student);
                    }

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
        public async Task LoadGroupAsync()
        {
            if(SelectedGroup == null)
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
            IsLoading = true;
            try
            {
                var bulkstudents = await _studentService.CreateBulkAccountStudentAsync(StudentsToCreate.ToList());
                foreach (var student in bulkstudents)
                {
                    Students.Add(student);
                }
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
            if (SelectedStudent == null)
            {
                ErrorMessage = "Please select a student to delete.";
                return;
            }
            try
            {
                var deletedStudent = await _studentService.DeleteStudentAsync(SelectedStudent.StudentId);
                if (deletedStudent != null)
                {
                    Students.Remove(SelectedStudent);
                    SelectedStudent = null;
                }
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
            if (SelectedStudent == null || SelectedGroup == null)
            {
                ErrorMessage = "Please select a student and a group to transfer.";
                return;
            }
            try
            {
                var student = await _studentService.GetStudentByIdAsync(SelectedStudent.StudentId);
                if (student != null)
                {
                    await _studentService.TransferStudentAsync(SelectedStudent.StudentId, SelectedGroup.GroupId);
                    Students.Remove(SelectedStudent);
                    Students.Add(student);
                    SelectedStudent = student;
                }
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
            if (SelectedGroup == null || SelectedStation == null || string.IsNullOrEmpty(DaySlot))
            {
                ErrorMessage = "Please select a group, station, and day slot to create a rotation assignment.";
                return;
            }
            try
            {
                var rotationAssignment = await _rotationService.CreateRotationAssignmentAsync(SelectedGroup.GroupId, SelectedStation.StationId, DaySlot, RotationStartDate, RotationEndDate);
                if (rotationAssignment != null)
                {
                    RotationAssignments.Add(rotationAssignment);
                    DaySlot = string.Empty;
                    RotationStartDate = DateOnly.FromDateTime(DateTime.Now);
                    RotationEndDate = DateOnly.FromDateTime(DateTime.Now);
                    SelectedStation = null;
                }
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
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting the rotation assignment: {ex.Message}";
            }
            finally { IsLoading = false; }
        }
    }
}
