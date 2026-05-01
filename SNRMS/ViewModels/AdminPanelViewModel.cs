using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Windows.Storage.Pickers;
using SNRMS.Core.Models;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.Interop;

namespace SNRMS.ViewModels
{


    public partial class AdminPanelViewModel : ObservableObject
    {
        private readonly SectionService _sectionService;
        private readonly InstructorService _instructorService;
        private readonly HospitalService _hospitalService;
        private readonly UserService _userService;

        public AdminPanelViewModel()
        {
            _sectionService = new SectionService(App.Database);
            _instructorService = new InstructorService(App.Database);
            _hospitalService = new HospitalService(App.Database);
            _userService = new UserService(App.Database);
        }

        //SECTION ------------------
        [ObservableProperty] public partial Section? SelectedSection { get; set; }
        [ObservableProperty] public partial string SectionName { get; set; } = string.Empty;
        [ObservableProperty] public partial string YearLevel { get; set; } = string.Empty;
        //INSTRUCTOR ------------------
        [ObservableProperty] public partial Instructor? SelectedInstructor { get; set; }
        [ObservableProperty] public partial string InstructorFirstName { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorLastName { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorSearchQuery { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorEmail { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstructorEmployeeId { get; set; } = string.Empty;
        [ObservableProperty] public partial bool ShowInactiveInstructors { get; set; }

        //EDIT INSTRUCTOR ------------------
        [ObservableProperty] public partial string EditInstructorFirstName { get; set; } = string.Empty;
        [ObservableProperty] public partial string EditInstructorLastName { get; set; } = string.Empty;
        [ObservableProperty] public partial string EditInstructorEmail { get; set; } = string.Empty;
        [ObservableProperty] public partial string EditInstructorEmployeeId { get; set; } = string.Empty;

        //HOSPITAL ------------------
        [ObservableProperty] public partial Hospital? SelectedHospital { get; set; }
        [ObservableProperty] public partial string HospitalName { get; set; } = string.Empty;
        [ObservableProperty] public partial string HospitalAddress { get; set; } = string.Empty;
        [ObservableProperty] public partial string HospitalSearchName { get; set; } = string.Empty;

        //STATION ------------------    
        [ObservableProperty] public partial Station? SelectedStation { get; set; }
        [ObservableProperty] public partial string StationName { get; set; } = string.Empty;
        [ObservableProperty] public partial string StationCapacity { get; set; } = string.Empty ;


        //GENERAL-------------------
        [ObservableProperty] public partial bool IsLoading { get; set; }
        [ObservableProperty] public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string SuccessMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string SelectedSort { get; set; } = string.Empty;
        [ObservableProperty] public partial string ResetStudentNumber { get; set; } = string.Empty;
        [ObservableProperty] public partial string ResetEmployeeId { get; set; } = string.Empty;


        //COLLECTIONS / LISTS -------------------
        [ObservableProperty] public partial ObservableCollection<Station> SelectedHospitalStations { get; set; } = new();
        [ObservableProperty] public partial ObservableCollection<Section> Sections { get; set; } = new ObservableCollection<Section>();
        [ObservableProperty] public partial ObservableCollection<Instructor> Instructors { get; set; } = new ObservableCollection<Instructor>();
        [ObservableProperty] public partial ObservableCollection<Instructor> DisplayInstructors { get; set; } = new();
        [ObservableProperty] public partial ObservableCollection<Hospital> Hospitals { get; set; } = new ObservableCollection<Hospital>();
        [ObservableProperty] public partial ObservableCollection<Instructor> ActiveInstructors { get; set; } = new ObservableCollection<Instructor>();
        public List<string> YearLevelsList { get; } = new List<string> { "1", "2", "3", "4" };
        public List<string> SectionsList { get; } = new List<string>
        {
                "Section A", "Section B", "Section C", "Section D", "Section E"
        };
        public List<string> SortChoicesList { get; } = new List<string> { "Name", "Year Level" };

        // ── ANALYTICS ──────────────────────────────────────────────────
        // Summary cards
        [ObservableProperty] public partial int TotalStudents { get; set; }
        [ObservableProperty] public partial int TotalInstructors { get; set; }
        [ObservableProperty] public partial int TotalGroups { get; set; }
        [ObservableProperty] public partial int TotalHospitals { get; set; }
        [ObservableProperty] public partial int TotalStations { get; set; }
        [ObservableProperty] public partial int TotalRotations { get; set; }
        [ObservableProperty] public partial int ActiveRotations { get; set; }
        [ObservableProperty] public partial int UnassignedSections { get; set; }

        // Horizontal Bar chart
        [ObservableProperty] public partial ObservableCollection<AnalyticsBarItem> StudentsPerSection { get; set; } = new();
        [ObservableProperty] public partial ObservableCollection<AnalyticsBarItem> RotationsPerHospital { get; set; } = new();
        [ObservableProperty] public partial ObservableCollection<HospitalBreakdownVm> HospitalBreakdown { get; set; } = new();
      
        // Day slot distribution
        [ObservableProperty]
        public partial ObservableCollection<AnalyticsBarItem> RotationsByDaySlot { get; set; } = new();
        [ObservableProperty] public partial int SlotMonTue { get; set; }
        [ObservableProperty] public partial int SlotWedThu { get; set; }
        [ObservableProperty] public partial int SlotFriSat { get; set; }

        // Instructor Analytics
        [ObservableProperty] public partial Instructor? SelectedAnalyticsInstructor { get; set; }
        [ObservableProperty] public partial bool HasInstructorAnalytics { get; set; }
        [ObservableProperty] public partial string InstructorAnalyticsName { get; set; } = "No instructor selected";
        [ObservableProperty] public partial string InstructorAnalyticsSection { get; set; } = "Choose an instructor to load section metrics.";
        [ObservableProperty] public partial int InstructorAnalyticsStudents { get; set; }
        [ObservableProperty] public partial int InstructorAnalyticsGroups { get; set; }
        [ObservableProperty] public partial int InstructorAnalyticsRotations { get; set; }
        [ObservableProperty] public partial string InstructorAnalyticsAttendanceRate { get; set; } = "0%";
        [ObservableProperty] public partial string SelectedOverallAttendanceRange { get; set; } = "This Month";
        [ObservableProperty] public partial string OverallAttendanceScopeDescription { get; set; } = "Attendance for the current month only";
        public List<string> OverallAttendanceRangeOptions { get; } = new() { "This Month", "All Time" };

        // Bar Graph 1: Rotations per Station
        [ObservableProperty] public partial ISeries[] RotationsPerStationSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] RotationsPerStationXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] RotationsPerStationYAxes { get; set; } = Array.Empty<Axis>();

        // Bar Graph 2: Students per Group
        [ObservableProperty] public partial ISeries[] StudentsPerGroupSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] StudentsPerGroupXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] StudentsPerGroupYAxes { get; set; } = Array.Empty<Axis>();

        // Bar Graph 3: Attendance Rate per Group (Current Rotation)
        [ObservableProperty] public partial ISeries[] AttendanceRateSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] AttendanceRateXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] AttendanceRateYAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial string NoRotationMessage { get; set; } = string.Empty;

        // Bar Graph 4: Overall Attendance Rate (All Rotations)
        [ObservableProperty] public partial ISeries[] OverallAttendanceRateSeries { get; set; } = Array.Empty<ISeries>();
        [ObservableProperty] public partial Axis[] OverallAttendanceRateXAxes { get; set; } = Array.Empty<Axis>();
        [ObservableProperty] public partial Axis[] OverallAttendanceRateYAxes { get; set; } = Array.Empty<Axis>();

        // LOAD DATA COMMANDS ----------------------------------------------------------------------
        [RelayCommand] public async Task LoadDataAsync()
        {
            IsLoading = true;
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            try
            {
                var sections = await _sectionService.GetAllSectionsAsync();
                Sections.Clear();
                foreach (var section in sections)
                    Sections.Add(section);
                var instructors = await _instructorService.GetAllInstructorsAsync();
                Instructors.Clear();
                ActiveInstructors.Clear();
                foreach (var instructor in instructors)
                    Instructors.Add(instructor);
                foreach (var instructor in instructors)
                {
                    var instructorUser = await App.Database.Users.FirstOrDefaultAsync(u => u.InstructorId == instructor.InstructorId);
                    if (instructorUser != null && instructorUser.IsActive)
                        ActiveInstructors.Add(instructor);
                }
                ApplyInstructorFilter();
                var hospitals = await _hospitalService.GetAllHospitalsAsync();
                Hospitals.Clear();
                foreach (var hospital in hospitals)
                    Hospitals.Add(hospital);
                await LoadAnalyticsAsync();

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


        //SECTION COMMANDS ----------------------------------------------------------------------
        [RelayCommand] public async Task CreateSectionAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (int.TryParse(YearLevel, out int yearlevel) == false || yearlevel > 4 || yearlevel < 1)
                {
                    ErrorMessage = "Please input a valid numerical value (1 - 4).";
                    return;
                }
                var createsection = await _sectionService.CreateSectionAsync(SectionName, int.Parse(YearLevel));
                if (createsection != null)
                {
                    Sections.Add(createsection);
                    SectionName = string.Empty;
                    YearLevel = "";
                }
                SuccessMessage = "Section created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating section: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task DeleteSectionAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedSection == null)
                {
                    ErrorMessage = "Please select a section to delete.";
                    return;
                }
                var deletedSection = await _sectionService.DeleteSectionAsync(SelectedSection.SectionId);
                if (deletedSection != null)
                {
                    Sections.Remove(SelectedSection);
                    SelectedSection = null;
                }
                SuccessMessage = "Section deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting section: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public void SortSections()
        {
            if (SelectedSort == "Name")
            {
                var sorted = Sections.OrderBy(s => s.SectionName).ToList();
                Sections.Clear();
                foreach (var section in sorted)
                    Sections.Add(section);
            }
            else if (SelectedSort == "Year Level")
            {
                var sorted = Sections.OrderBy(s => s.YearLevel).ToList();
                Sections.Clear();
                foreach (var section in sorted)
                    Sections.Add(section);
            }

        }

        //INSTRUCTOR COMMANDS ----------------------------------------------------------------------
        [RelayCommand] public async Task CreateInstructorAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                var createinstructor = await _userService.CreateInstructorAsync(InstructorFirstName, InstructorLastName, InstructorEmail, InstructorEmployeeId);
                if (createinstructor != null)
                {
                    Instructors.Add(createinstructor);
                    InstructorFirstName = string.Empty;
                    InstructorLastName = string.Empty;
                    InstructorEmail = string.Empty;
                    InstructorEmployeeId = string.Empty;
                }
                SuccessMessage = "Instructor created successfully.";
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating instructor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task AssignInstructorToSectionAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedSection == null || SelectedInstructor == null)
                {
                    ErrorMessage = "Please select both a section and an instructor.";
                    return;
                }
                var updatedSection = await _sectionService.AssignInstructorAsync(SelectedSection.SectionId, SelectedInstructor.InstructorId);
                if (updatedSection != null)
                {
                    var index = Sections.IndexOf(SelectedSection);
                    if (index >= 0)
                    {
                        Sections[index] = updatedSection;
                        SelectedSection = updatedSection;
                    }
                }
                SuccessMessage = "Instructor assigned successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while assigning instructor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }

        }
        [RelayCommand] public async Task UnassignInstructorAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedSection == null)
                {
                    ErrorMessage = "Please select a section to unassign.";
                    return;
                }
                var updatedSection = await _sectionService.UnassignInstructorAsync(SelectedSection.SectionId);
                if (updatedSection != null)
                {
                    var index = Sections.IndexOf(SelectedSection);
                    if (index >= 0)
                    {
                        Sections[index] = updatedSection;
                        SelectedSection = updatedSection;
                    }
                }
                SuccessMessage = "Instructor unassigned successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while unassigning instructor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task DeactivateUserAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedInstructor == null)
                {
                    ErrorMessage = "Please select an instructor to deactivate.";
                    return;
                }
                var instructorUser = await App.Database.Users.FirstOrDefaultAsync(u => u.InstructorId == SelectedInstructor.InstructorId);
                if (instructorUser == null)
                {
                    ErrorMessage = "Associated user account not found.";
                    return;
                }
                var user = await _userService.DeactivateUserAsync(instructorUser.UserId);
                SelectedInstructor = null;


                SuccessMessage = "Instructor deactivated successfully.";
                await LoadDataAsync();

            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting instructor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task ReactivateUserAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedInstructor == null)
                {
                    ErrorMessage = "Please select an instructor to reactivate.";
                    return;
                }
                var instructorUser = await App.Database.Users.FirstOrDefaultAsync(u => u.InstructorId == SelectedInstructor.InstructorId);
                if (instructorUser == null)
                {
                    ErrorMessage = "Associated user account not found.";
                    return;
                }
                var user = await _userService.ReactivateUserAsync(instructorUser.UserId);
                SelectedInstructor = null;

                SuccessMessage = "Instructor reactivated successfully.";
                await LoadDataAsync();

            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting instructor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public void SortInstructorsByLastName()
        {
            var sorted = Instructors.OrderBy(i => i.LastName).ToList();
            Instructors.Clear();
            foreach (var instructor in sorted)
                Instructors.Add(instructor);
            ApplyInstructorFilter();
        }
        [RelayCommand]public async Task SearchInstructorAsync()
        {
            IsLoading = true;
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(InstructorSearchQuery))
                {
                    await LoadDataAsync();
                    return;
                }

                var results = await _instructorService.SearchInstructorsAsync(InstructorSearchQuery);
                Instructors.Clear();
                foreach (var instructor in results)
                    Instructors.Add(instructor);
                ApplyInstructorFilter();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while searching for instructors: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        public async Task SaveInstructorEditAsync()
        {
            if (SelectedInstructor == null) return;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = true;
            try
            {
                await _userService.UpdateInstructorAsync(
                    SelectedInstructor.InstructorId,
                    EditInstructorFirstName,
                    EditInstructorLastName,
                    EditInstructorEmail,
                    EditInstructorEmployeeId);
                await LoadDataAsync();
                SuccessMessage = "Instructor information updated successfully.";
            }
            finally { IsLoading = false; }
        }
        private void ApplyInstructorFilter()
        {
            if (Instructors == null) return;

            var filtered = Instructors.Where(i =>
                (i.User?.IsActive == true && !ShowInactiveInstructors) ||
                (i.User?.IsActive == false && ShowInactiveInstructors)
            ).ToList();

            DisplayInstructors.Clear();
            foreach (var instructor in filtered)
            {
                DisplayInstructors.Add(instructor);
            }
        }
        partial void OnShowInactiveInstructorsChanged(bool value)
        {
            ApplyInstructorFilter();
        }

        //HOSPITAL COMMANDS ----------------------------------------------------------------------
        [RelayCommand] public async Task CreateHospitalAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                var createhospital = await _hospitalService.CreateHospitalAsync(HospitalName, HospitalAddress);
                if (createhospital != null)
                {
                    Hospitals.Add(createhospital);
                    HospitalName = string.Empty;
                    HospitalAddress = string.Empty;
                }
                SuccessMessage = "Hospital created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating hospital: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand]  public async Task DeleteHospitalAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedHospital == null)
                {
                    ErrorMessage = "Please select a hospital to delete.";
                    return;
                }
                var deletedHospital = await _hospitalService.DeleteHospitalAsync(SelectedHospital.HospitalId);
                if (deletedHospital != null)
                {
                    Hospitals.Remove(SelectedHospital);
                    SelectedHospital = null;
                }
                SuccessMessage = "Hospital deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting hospital: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public void SortHospitalsByName()
        {
            var sorted = Hospitals.OrderBy(h => h.HospitalName).ToList();
            Hospitals.Clear();
            foreach (var hospital in sorted)
                Hospitals.Add(hospital);
        }    
        [RelayCommand] public async Task GetHospitalsByNameAsync()
        {
            IsLoading = true;
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(HospitalSearchName))
                {
                    await LoadDataAsync();
                    return;
                }
                var hospitals = await _hospitalService.GetHospitalsByNameAsync(HospitalSearchName);
                Hospitals.Clear();
                foreach (var hospital in hospitals)
                    Hospitals.Add(hospital);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while searching for hospitals: {ex.Message}";
            }
            finally
            {
                IsLoading = false;

            }
        }
        partial void OnSelectedHospitalChanged(Hospital? value)
        {
            _ = LoadStationsForHospitalAsync(value);
        }
        private async Task LoadStationsForHospitalAsync(Hospital? hospital)
        {
            SelectedHospitalStations.Clear();
            if (hospital == null) return;
            var stations = await _hospitalService.GetStationsByHospitalIdAsync(hospital.HospitalId);
            foreach (var station in stations)
                SelectedHospitalStations.Add(station);
        }

        //STATION COMMANDS ----------------------------------------------------------------------
        [RelayCommand] public async Task AddStationAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedHospital == null)
                {
                    ErrorMessage = "Please select a hospital.";
                    return;
                }
                if (!int.TryParse(StationCapacity, out int capacity) || capacity < 0)
                {
                    ErrorMessage = "Please input a valid numerical value for station capacity.";
                    return;
                }
                var addstation = await _hospitalService.AddStationAsync(SelectedHospital.HospitalId, StationName, int.Parse(StationCapacity));
                if (addstation != null)
                {
                    SelectedHospital.Stations.Add(addstation);
                    OnSelectedHospitalChanged(SelectedHospital);
                    StationName = string.Empty;
                    StationCapacity = "";
                }
                SuccessMessage = "Station added successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while adding station: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task RemoveStationAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedStation == null)
                {
                    ErrorMessage = "Please select a station to remove.";
                    return;
                }
                await _hospitalService.RemoveStationAsync(SelectedStation.StationId);
                SelectedHospitalStations.Remove(SelectedStation);
                SelectedStation = null;
                SuccessMessage = "Station removed successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }


        //ANALYTICS LOAD COMMAND ----------------------------------------------------------------------
        [RelayCommand] public async Task LoadAnalyticsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                //  total count
                TotalStudents = await App.Database.Students.CountAsync(s => !s.IsArchived);
                TotalInstructors = await App.Database.Instructors.Where(i => i.User != null && i.User.IsActive == true).CountAsync();
                TotalGroups = await App.Database.Groups.CountAsync(g => !g.IsArchived);
                TotalHospitals = await App.Database.Hospitals.CountAsync();
                TotalStations = await App.Database.Stations.CountAsync(s => !s.IsArchived);
                TotalRotations = await App.Database.RotationAssignments.CountAsync(r => !r.IsArchived);
                ActiveRotations = await App.Database.RotationAssignments
                    .CountAsync(r => !r.IsArchived && r.StartDate <= today && r.EndDate >= today);
                UnassignedSections = await App.Database.Sections.CountAsync(s => s.InstructorId == null);

                // Students per section
                var sections = await App.Database.Sections
                    .Include(s => s.Groups).ThenInclude(g => g.Students)
                    .ToListAsync();
                StudentsPerSection.Clear();
                foreach (var sec in sections.OrderBy(s => s.SectionName))
                {
                    var count = sec.Groups.Sum(g => g.Students.Count(st => !st.IsArchived));
                    StudentsPerSection.Add(new AnalyticsBarItem($"{sec.SectionName} | Year: {sec.YearLevel}", count));
                }

                // Rotations per hospital
                var detailedRotations = await App.Database.RotationAssignments
                    .Where(r => !r.IsArchived)
                    .Include(r => r.Station).ThenInclude(s => s.Hospital)
                    .Include(r => r.Group).ThenInclude(g => g.Section)
                    .ToListAsync();

                HospitalBreakdown.Clear();
                RotationsPerHospital.Clear();

                // Load ALL hospitals 
                var allHospitals = await App.Database.Hospitals
                    .OrderBy(h => h.HospitalName)
                    .ToListAsync();

                int globalMax = Math.Max(detailedRotations.Count, 1);

                foreach (var hospital in allHospitals)
                {
                    var hospitalRotations = detailedRotations
                        .Where(r => r.Station?.Hospital?.HospitalId == hospital.HospitalId)
                        .ToList();

                    int totalForHospital = hospitalRotations.Count;

                    var vm = new HospitalBreakdownVm(
                        hospital.HospitalName,
                        hospital.Address,
                        totalForHospital);

                    // Only build group breakdown if there are rotations
                    if (totalForHospital > 0)
                    {
                        var byGroup = hospitalRotations
                            .GroupBy(r => new
                            {
                                GroupName = r.Group?.GroupName ?? "No Group",
                                SectionName = r.Group?.Section?.SectionName ?? "No Section",
                                YearLevel = r.Group?.Section?.YearLevel ?? 0
                            })
                            .OrderBy(g => g.Key.YearLevel)
                            .ThenBy(g => g.Key.SectionName)
                            .ThenBy(g => g.Key.GroupName);

                        foreach (var g in byGroup)
                        {
                            vm.Groups.Add(new GroupBreakdownVm(
                                g.Key.GroupName,
                                g.Key.SectionName,
                                g.Key.YearLevel,
                                g.Count(),
                                Math.Max(totalForHospital, 1)));
                        }
                    }

                    HospitalBreakdown.Add(vm);

                    // Keep flat list in sync
                    RotationsPerHospital.Add(new AnalyticsBarItem(
                        hospital.HospitalName, totalForHospital,
                        isHeader: true, maxValue: globalMax));
                }

               
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load analytics: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand] public async Task LoadInstructorAnalyticsAsync()
        {
            if (SelectedAnalyticsInstructor == null) return;
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var instructorId = SelectedAnalyticsInstructor.InstructorId;

                var section = await App.Database.Sections
                    .Include(s => s.Groups).ThenInclude(g => g.Students.Where(st => !st.IsArchived))
                    .Include(s => s.Groups).ThenInclude(g => g.RotationAssignments.Where(r => !r.IsArchived))
                        .ThenInclude(ra => ra.Station).ThenInclude(s => s.Hospital)
                    .FirstOrDefaultAsync(s => s.InstructorId == instructorId);

                if (section == null)
                {
                    ErrorMessage = "This instructor has no assigned section.";
                    HasInstructorAnalytics = false;
                    ClearInstructorAnalyticsSummary();
                    return;
                }

                var activeGroups = section.Groups.Where(g => !g.IsArchived).ToList();
                InstructorAnalyticsName = $"{SelectedAnalyticsInstructor.FirstName} {SelectedAnalyticsInstructor.LastName}".Trim();
                InstructorAnalyticsSection = $"{section.SectionName} | Year {section.YearLevel}";
                InstructorAnalyticsGroups = activeGroups.Count;
                InstructorAnalyticsStudents = activeGroups.Sum(g => g.Students.Count);
                InstructorAnalyticsRotations = activeGroups.Sum(g => g.RotationAssignments.Count);

                // ── GRAPH 1: Rotations per Station ────────────────────────────
                var rotationsByStation = activeGroups
                    .SelectMany(g => g.RotationAssignments)
                    .GroupBy(r => new {
                        Station  = r.Station?.StationName ?? "Unknown",
                        Hospital = r.Station?.Hospital?.HospitalName ?? ""
                    })
                    .OrderByDescending(g => g.Count())
                    .ToList();

                // Label = "StationName\nHospitalName" so hospital shows below station
                var stationLabels = rotationsByStation
                    .Select(g => {
                        if (string.IsNullOrEmpty(g.Key.Hospital)) return g.Key.Station;
                        // Abbreviate hospital name: take first letter of each word
                        var abbrev = string.Concat(g.Key.Hospital
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(w => w[0]))
                            .ToUpper();
                        return $"{g.Key.Station} ({abbrev})";
                    })
                    .ToArray();
                var stationValues = rotationsByStation.Select(g => (double)g.Count()).ToArray();

                // One series with abbreviated station labels on the X axis.
                RotationsPerStationSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Rotations",
                        Values = stationValues,
                        Fill = new SolidColorPaint(new SKColor(27, 58, 107)),
                        MaxBarWidth = 40
                    }
                };
                RotationsPerStationXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = stationLabels,
                        LabelsRotation = -25,
                        TextSize = 10,
                        MinStep = 1,
                        ForceStepToMin = true
                    }
                };
                RotationsPerStationYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Rotations",
                        MinLimit = 0,
                        TextSize = 11
                    }
                };

                // ── GRAPH 2: Students per Group ───────────────────────────────
                var groupLabels = activeGroups.OrderBy(g => g.GroupName)
                    .Select(g => g.GroupName).ToArray();
                var studentValues = activeGroups.OrderBy(g => g.GroupName)
                    .Select(g => (double)g.Students.Count).ToArray();

                StudentsPerGroupSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Students",
                        Values = studentValues,
                        Fill = new SolidColorPaint(new SKColor(56, 161, 105)),
                        MaxBarWidth = 40
                    }
                };
                StudentsPerGroupXAxes = new Axis[]
                {
                    new Axis { Labels = groupLabels, TextSize = 11 }
                };
                StudentsPerGroupYAxes = new Axis[]
                {
                    new Axis { Name = "Students", MinLimit = 0, TextSize = 11 }
                };

                // GRAPH 3: Attendance Rate per Group — current rotation only
                var today = DateOnly.FromDateTime(DateTime.Today);
                var attendanceLabels = new List<string>();
                var attendanceValues = new List<double>();
                var noRotationGroups = new List<string>();

                int GetScheduledDaysElapsed(RotationAssignment rotation, DateOnly currentDate)
                {
                    // Map DaySlot to which days of week count
                    var scheduledDays = rotation.DaySlot switch
                    {
                        "Mon-Tue" => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                        "Wed-Thu" => new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday },
                        "Fri-Sat" => new[] { DayOfWeek.Friday, DayOfWeek.Saturday },
                        _ => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday,
                                     DayOfWeek.Wednesday, DayOfWeek.Thursday,
                                     DayOfWeek.Friday }
                    };

                    int count = 0;
                    var d = rotation.StartDate;
                    while (d <= currentDate && d <= rotation.EndDate)
                    {
                        if (scheduledDays.Contains((DayOfWeek)((int)d.DayOfWeek)))
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
                        _ => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday,
                                     DayOfWeek.Wednesday, DayOfWeek.Thursday,
                                     DayOfWeek.Friday }
                    };

                    var start = rotation.StartDate > rangeStart ? rotation.StartDate : rangeStart;
                    var end = rotation.EndDate < rangeEnd ? rotation.EndDate : rangeEnd;
                    if (start > end) return 0;

                    int count = 0;
                    var d = start;
                    while (d <= end)
                    {
                        if (scheduledDays.Contains(d.DayOfWeek))
                            count++;
                        d = d.AddDays(1);
                    }
                    return count;
                }

                foreach (var group in activeGroups.OrderBy(g => g.GroupName))
                {
                    // Include any rotation that has started (past or active) — not just today's
                    var relevantRotation = group.RotationAssignments
                        .Where(r => r.StartDate <= today)
                        .OrderByDescending(r => r.StartDate)
                        .FirstOrDefault();

                    if (relevantRotation == null)
                    {
                        noRotationGroups.Add(group.GroupName);
                        continue;
                    }

                    int scheduledDaysElapsed = GetScheduledDaysElapsed(relevantRotation, today);
                    // Always count at least 1 expected day so a clock-in always registers
                    int totalExpected = group.Students.Count * Math.Max(scheduledDaysElapsed, 1);

                    int totalAttended = await App.Database.AttendanceRecords
                        .Where(a => a.RotationAssignmentId == relevantRotation.RotationAssignmentId)
                        .Select(a => new { a.StudentId, a.DateToday })
                        .Distinct()
                        .CountAsync();

                    double rate = totalExpected > 0
                        ? Math.Round((totalAttended / (double)totalExpected) * 100, 1)
                        : 0;

                    attendanceLabels.Add(group.GroupName);
                    attendanceValues.Add(rate);
                }

                var attendancePaints = attendanceValues
                    .Select(v => v >= 80
                        ? new SolidColorPaint(new SKColor(56, 161, 105))
                        : new SolidColorPaint(new SKColor(229, 62, 62)))
                    .ToArray();

                // Use two series: one green (≥80) one red (<80), null placeholders for gaps
                var greenValues = attendanceValues.Select((v, i) => v >= 80 ? v : (double?)null).ToArray();
                var redValues   = attendanceValues.Select((v, i) => v < 80  ? v : (double?)null).ToArray();

                var attendanceSeries = new ISeries[]
                {
                    new ColumnSeries<double?>
                    {
                        Name = "≥ 80%",
                        Values = greenValues,
                        Fill = new SolidColorPaint(new SKColor(56, 161, 105)),
                        MaxBarWidth = 40,
                        IgnoresBarPosition = false
                    },
                    new ColumnSeries<double?>
                    {
                        Name = "< 80%",
                        Values = redValues,
                        Fill = new SolidColorPaint(new SKColor(229, 62, 62)),
                        MaxBarWidth = 40,
                        IgnoresBarPosition = false
                    },
                    new LineSeries<double?>
                    {
                        Name = "80% Threshold",
                        Values = attendanceLabels.Select(_ => (double?)80.0).ToArray(),
                        Stroke = new SolidColorPaint(new SKColor(237, 137, 54)) { StrokeThickness = 2 },
                        Fill = null,
                        GeometrySize = 0,
                        LineSmoothness = 0
                    }
                };

                AttendanceRateSeries = attendanceSeries;
                AttendanceRateXAxes = new Axis[]
                {
                    new Axis { Labels = attendanceLabels.ToArray(), TextSize = 11 }
                };
                AttendanceRateYAxes = new Axis[]
                {
                    new Axis { Name = "Attendance %", MinLimit = 0, MaxLimit = 100, TextSize = 11 }
                };

                // Show message for groups with no active rotation
                NoRotationMessage = noRotationGroups.Count > 0
                    ? $"No active rotation today: {string.Join(", ", noRotationGroups)}"
                    : string.Empty;

                // ── GRAPH 4: Overall Attendance Rate (all rotations to date) ─────────
                var overallLabels = new List<string>();
                var overallValues = new List<double>();
                var overallTrends = new List<string>(); // ▲ ▼ →
                var overallRangeStart = SelectedOverallAttendanceRange == "This Month"
                    ? new DateOnly(today.Year, today.Month, 1)
                    : DateOnly.MinValue;
                var overallRangeEnd = today;
                OverallAttendanceScopeDescription = SelectedOverallAttendanceRange == "This Month"
                    ? $"Attendance from {overallRangeStart:MMM d} to {overallRangeEnd:MMM d, yyyy}"
                    : "Cumulative attendance across all started rotations";

                foreach (var group in activeGroups.OrderBy(g => g.GroupName))
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

                    int totalScheduledDays = pastAndCurrentRotations
                        .Sum(r => GetScheduledDaysInRange(r, overallRangeStart, overallRangeEnd));

                    int totalExpectedOverall = group.Students.Count * totalScheduledDays;

                    var rotationIds = pastAndCurrentRotations
                        .Select(r => r.RotationAssignmentId)
                        .ToList();

                    int totalAttendedOverall = await App.Database.AttendanceRecords
                        .Where(a => rotationIds.Contains(a.RotationAssignmentId)
                                    && a.DateToday >= overallRangeStart
                                    && a.DateToday <= overallRangeEnd)
                        .Select(a => new { a.StudentId, a.DateToday })
                        .Distinct()
                        .CountAsync();

                    double overallRate = totalExpectedOverall > 0
                        ? Math.Round((totalAttendedOverall / (double)totalExpectedOverall) * 100, 1)
                        : 0;

                    overallLabels.Add(group.GroupName);
                    overallValues.Add(overallRate);

                    // Trend arrow: compare current rotation rate vs overall rate
                    var attendanceIndex = attendanceLabels.IndexOf(group.GroupName);
                    double currentRate = attendanceIndex >= 0 ? attendanceValues[attendanceIndex] : -1;

                    string trend = currentRate < 0 ? "→"
                        : currentRate > overallRate + 2 ? "▲"
                        : currentRate < overallRate - 2 ? "▼"
                        : "→";

                    overallTrends.Add(trend);
                }

                var overallGreenValues = overallValues.Select(v => v >= 80 ? v : (double?)null).ToArray();
                var overallRedValues   = overallValues.Select(v => v < 80  ? v : (double?)null).ToArray();

                var overallSeries = new ISeries[]
                {
                    new ColumnSeries<double?>
                    {
                        Name = "≥ 80%",
                        Values = overallGreenValues,
                        Fill = new SolidColorPaint(new SKColor(56, 161, 105)),
                        MaxBarWidth = 40,
                        IgnoresBarPosition = false
                    },
                    new ColumnSeries<double?>
                    {
                        Name = "< 80%",
                        Values = overallRedValues,
                        Fill = new SolidColorPaint(new SKColor(229, 62, 62)),
                        MaxBarWidth = 40,
                        IgnoresBarPosition = false
                    },
                    new LineSeries<double?>
                    {
                        Name = "80% Threshold",
                        Values = overallLabels.Select(_ => (double?)80.0).ToArray(),
                        Stroke = new SolidColorPaint(new SKColor(237, 137, 54)) { StrokeThickness = 2 },
                        Fill = null,
                        GeometrySize = 0,
                        LineSmoothness = 0
                    }
                };

                OverallAttendanceRateSeries = overallSeries;
                OverallAttendanceRateXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = overallLabels.Select((l, i) => $"{l} {overallTrends[i]}").ToArray(),
                        TextSize = 11,
                        LabelsRotation = -15,
                        MinStep = 1,
                        ForceStepToMin = true
                    }
                };
                OverallAttendanceRateYAxes = new Axis[]
                {
                    new Axis { Name = "Attendance %", MinLimit = 0, MaxLimit = 100, TextSize = 11 }
                };
                var overallAverage = overallValues.Count > 0 ? overallValues.Average() : 0;
                InstructorAnalyticsAttendanceRate = $"{overallAverage:0.#}%";

                HasInstructorAnalytics = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load instructor analytics: {ex.Message}";
                HasInstructorAnalytics = false;
                ClearInstructorAnalyticsSummary();
            }
            finally { IsLoading = false; }
        }

        private void ClearInstructorAnalyticsSummary()
        {
            InstructorAnalyticsName = SelectedAnalyticsInstructor == null
                ? "No instructor selected"
                : $"{SelectedAnalyticsInstructor.FirstName} {SelectedAnalyticsInstructor.LastName}".Trim();
            InstructorAnalyticsSection = "No assigned section";
            InstructorAnalyticsStudents = 0;
            InstructorAnalyticsGroups = 0;
            InstructorAnalyticsRotations = 0;
            InstructorAnalyticsAttendanceRate = "0%";
        }

        partial void OnSelectedOverallAttendanceRangeChanged(string value)
        {
            if (HasInstructorAnalytics)
                _ = LoadInstructorAnalyticsAsync();
        }


        // RESET PASSWORD COMMANDS ----------------------------------------------------------------------
        [RelayCommand] public async Task ResetInstructorPasswordAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                if (SelectedInstructor == null)
                {
                    ErrorMessage = "Please select an instructor to reset password.";
                    return;
                }
                var resetInstructor = await _userService.ResetInstructorPasswordAsync(SelectedInstructor.EmployeeId);
                if (resetInstructor != null)
                {
                    SuccessMessage = $"Password for {resetInstructor.FirstName} {resetInstructor.LastName} has been reset to 'user123'.";
                }
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while resetting password: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand] public async Task ResetStudentPasswordAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            if (string.IsNullOrWhiteSpace(ResetStudentNumber))
            {
                ErrorMessage = "Please enter a Student Number.";
                return;
            }
            try
            {
                IsLoading = true;
                var student = await _userService.ResetStudentPasswordAsync(ResetStudentNumber);
                SuccessMessage = $"Password for {student!.FirstName} {student.LastName} has been reset to: {student.StudentNumber}";
                ResetStudentNumber = string.Empty;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally { IsLoading = false; }

        }

        // SETTINGS --------------------------------------------------------------------------------------
        
        public async Task ChangePasswordAsync(string currentPwd, string newPwd)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            var user = SessionManager.CurrentUser;
            if (user == null) throw new Exception("Session expired. Please log in again.");
            await _userService.ChangePasswordAsync(user.UserId, currentPwd, newPwd);
            SessionManager.CurrentUser.HasChangedPassword = true;
            SuccessMessage = "Password changed successfully.";
        }
        public async Task<Instructor?> ResetInstructorPasswordAsync(string employeeId)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            var instructor = await _userService.ResetInstructorPasswordAsync(employeeId);
            SuccessMessage = $"Password reset to 'user123' for {instructor?.FirstName} {instructor?.LastName}.";
            return instructor;
        }
        public async Task<Student?> ResetStudentPasswordAsync(string studentNumber)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            var student = await _userService.ResetStudentPasswordAsync(studentNumber);
            SuccessMessage = $"Password reset to student number for {student?.FirstName} {student?.LastName}.";
            return student;
        }

        //SESSION MANAGER-----------------------------------------------------------------------------
        [RelayCommand] public void Logout()
        {
            SessionManager.Logout();
        }

    }
}


