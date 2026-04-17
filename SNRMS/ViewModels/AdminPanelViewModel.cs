using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [ObservableProperty]
        public partial Section? SelectedSection { get; set; }
        [ObservableProperty]
        public partial string SectionName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string YearLevel { get; set; } = string.Empty;
        //INSTRUCTOR ------------------
        [ObservableProperty]
        public partial Instructor? SelectedInstructor { get; set; }
        [ObservableProperty]
        public partial string InstructorFirstName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string InstructorLastName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorSearchQuery { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorEmail { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorEmployeeId { get; set; } = string.Empty;    

        //HOSPITAL ------------------
        [ObservableProperty]
        public partial Hospital? SelectedHospital { get; set; }
        [ObservableProperty]
        public partial string HospitalName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string HospitalAddress { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string HospitalSearchName { get; set; } = string.Empty;

        //STATION ------------------    
        [ObservableProperty]
        public partial Station? SelectedStation { get; set; }
        [ObservableProperty]
        public partial string StationName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string StationCapacity { get; set; }


        //GENERAL-------------------
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string SuccessMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string SelectedSort { get; set; } = string.Empty;


        //COLLECTIONS / LISTS -------------------
        [ObservableProperty]
        public partial ObservableCollection<Station> SelectedHospitalStations { get; set; } = new();
        [ObservableProperty]
        public partial ObservableCollection<Section> Sections { get; set; } = new ObservableCollection<Section>();
        [ObservableProperty]
        public partial ObservableCollection<Instructor> Instructors { get; set; } = new ObservableCollection<Instructor>();
        [ObservableProperty]
        public partial ObservableCollection<Hospital> Hospitals { get; set; } = new ObservableCollection<Hospital>();
        [ObservableProperty]
        public partial ObservableCollection<Instructor> ActiveInstructors { get; set; } = new ObservableCollection<Instructor>();
        public List<string> YearLevelsList { get; } = new List<string> { "1", "2", "3", "4" };
        public List<string> SectionsList { get; } = new List<string>
        {
                "Section A", "Section B", "Section C", "Section D", "Section E"
        };
        public List<string> SortChoicesList { get; } = new List<string> { "Name", "Year Level" };

        // ── ANALYTICS ──────────────────────────────────────────────────
        // Summary cards
        [ObservableProperty] public partial int TotalStudents    { get; set; }
        [ObservableProperty] public partial int TotalInstructors { get; set; }
        [ObservableProperty] public partial int TotalGroups      { get; set; }
        [ObservableProperty] public partial int TotalHospitals   { get; set; }
        [ObservableProperty] public partial int TotalStations    { get; set; }
        [ObservableProperty] public partial int TotalRotations   { get; set; }
        [ObservableProperty] public partial int ActiveRotations  { get; set; }
        [ObservableProperty] public partial int UnassignedSections { get; set; }

        // Bar chart: students per section (list of label+value)
        [ObservableProperty]
        public partial ObservableCollection<AnalyticsBarItem> StudentsPerSection { get; set; } = new();

        // Bar chart: rotations per hospital
        [ObservableProperty]
        public partial ObservableCollection<AnalyticsBarItem> RotationsPerHospital { get; set; } = new();

        // Day slot distribution
        [ObservableProperty]
        public partial ObservableCollection<AnalyticsBarItem> RotationsByDaySlot { get; set; } = new();
        [ObservableProperty] public partial int SlotMonTue { get; set; }
        [ObservableProperty] public partial int SlotWedThu { get; set; }
        [ObservableProperty] public partial int SlotFriSat { get; set; }



        // LOAD DATA COMMANDS ----------------------------------------------------------------------
        [RelayCommand]
        public async Task LoadDataAsync()
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
                foreach(var instructor in instructors)
                {
                    var instructorUser = await App.Database.Users.FirstOrDefaultAsync(u => u.InstructorId == instructor.InstructorId);
                    if (instructorUser != null && instructorUser.IsActive)
                        ActiveInstructors.Add(instructor);
                }
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
        [RelayCommand]
        public async Task CreateSectionAsync()
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
        [RelayCommand]
        public async Task DeleteSectionAsync()
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
        [RelayCommand]
        public void SortSections()
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
        [RelayCommand]
        public async Task CreateInstructorAsync()
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
        [RelayCommand]
        public async Task AssignInstructorToSectionAsync()
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
        [RelayCommand]
        public async Task UnassignInstructorAsync()
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
        [RelayCommand]
        public async Task DeactivateUserAsync()
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

        [RelayCommand]
        public async Task ReactivateUserAsync()
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
        [RelayCommand]
        public void SortInstructorsByLastName()
        {
            var sorted = Instructors.OrderBy(i => i.LastName).ToList();
            Instructors.Clear();
            foreach (var instructor in sorted)
                Instructors.Add(instructor);
        }

        [RelayCommand]
        public async Task SearchInstructorAsync()
        {
            IsLoading = true;
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(InstructorSearchQuery))
                {
                    await LoadDataAsync();
                    return;
                }

                var results = await _instructorService.SearchInstructorsAsync(InstructorSearchQuery);
                Instructors.Clear();
                foreach (var instructor in results)
                    Instructors.Add(instructor);
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


        //HOSPITAL COMMANDS ----------------------------------------------------------------------
        [RelayCommand]
        public async Task CreateHospitalAsync()
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
        [RelayCommand]
        public async Task DeleteHospitalAsync()
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
        [RelayCommand]
        public void SortHospitalsByName()
        {
            var sorted = Hospitals.OrderBy(h => h.HospitalName).ToList();
            Hospitals.Clear();
            foreach (var hospital in sorted)
                Hospitals.Add(hospital);
        }
        [RelayCommand]
        partial void OnSelectedHospitalChanged(Hospital? value)
        {
            SelectedHospitalStations.Clear();
            if (value != null)
                foreach (var station in value.Stations)
                    SelectedHospitalStations.Add(station);
        }
        [RelayCommand]
        public async Task GetHospitalsByNameAsync()
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

        //STATION COMMANDS ----------------------------------------------------------------------
        [RelayCommand]
        public async Task AddStationAsync()
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
                if(!int.TryParse(StationCapacity, out int capacity) || capacity < 0)
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
        [RelayCommand]
        public async Task RemoveStationAsync()
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

         
        // ── ANALYTICS LOAD COMMAND ────────────────────────────────────
        [RelayCommand]
        public async Task LoadAnalyticsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                // Summary counts
                TotalStudents    = await App.Database.Students.CountAsync(s => !s.IsArchived);
                TotalInstructors = await App.Database.Instructors.Where(i => i.User != null && i.User.IsActive == true).CountAsync();
                TotalGroups      = await App.Database.Groups.CountAsync(g => !g.IsArchived);
                TotalHospitals   = await App.Database.Hospitals.CountAsync();
                TotalStations    = await App.Database.Stations.CountAsync();
                TotalRotations   = await App.Database.RotationAssignments.CountAsync(r => !r.IsArchived);
                ActiveRotations  = await App.Database.RotationAssignments
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
                var hospitals = await App.Database.Hospitals
                    .Include(h => h.Stations).ThenInclude(st => st.RotationAssignments)
                    .ToListAsync();
                RotationsPerHospital.Clear();
                foreach (var hosp in hospitals.OrderBy(h => h.HospitalName))
                {
                    var count = hosp.Stations.Sum(st => st.RotationAssignments.Count(r => !r.IsArchived));
                    RotationsPerHospital.Add(new AnalyticsBarItem($"{hosp.HospitalName} | {hosp.Address}", count));
                }

                // Rotations by day slot
                var allRotations = await App.Database.RotationAssignments
                    .Where(r => !r.IsArchived).ToListAsync();
                RotationsByDaySlot.Clear();
                SlotMonTue = allRotations.Count(r => r.DaySlot == "Mon-Tue");
                SlotWedThu = allRotations.Count(r => r.DaySlot == "Wed-Thu");
                SlotFriSat = allRotations.Count(r => r.DaySlot == "Fri-Sat");

                RotationsByDaySlot.Clear();
                RotationsByDaySlot.Add(new AnalyticsBarItem("Mon-Tue", SlotMonTue));
                RotationsByDaySlot.Add(new AnalyticsBarItem("Wed-Thu", SlotWedThu));
                RotationsByDaySlot.Add(new AnalyticsBarItem("Fri-Sat", SlotFriSat));
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

        //SESSION MANAGEMENT COMMANDS ----------------------------------------------------------------------
        [RelayCommand]
        public void Logout()
        {
            SessionManager.Logout();
        }
    }
}


