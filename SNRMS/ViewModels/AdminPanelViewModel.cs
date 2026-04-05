using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Models;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

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
        public partial string YearLevel { get; set; }
        //INSTRUCTOR ------------------
        [ObservableProperty]
        public partial Instructor? SelectedInstructor { get; set; }
        [ObservableProperty]
        public partial string InstructorFirstName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string InstructorLastName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorEmail { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorPassword { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string InstructorUsername { get; set; } = string.Empty;

        //HOSPITAL ------------------
        [ObservableProperty]
        public partial Hospital? SelectedHospital { get; set; }
        [ObservableProperty]
        public partial string HospitalName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string HospitalAddress { get; set; } = string.Empty;


        //STATION ------------------    
        [ObservableProperty]
        public partial string StationName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial int StationCapacity { get; set; }


        //GENERAL-------------------
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string SuccessMessage { get; set; } = string.Empty;


        //COLLECTIONS / LISTS -------------------
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
                var addstation = await _hospitalService.AddStationAsync(SelectedHospital.HospitalId, StationName, StationCapacity);
                if (addstation != null)
                {
                    var index = Hospitals.IndexOf(SelectedHospital);
                    if (index >= 0)
                    {
                        Hospitals[index].Stations.Add(addstation);
                        SelectedHospital = Hospitals[index];
                    }
                    StationName = string.Empty;
                    StationCapacity = 0;
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
        public async Task CreateInstructorAsync()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = true;
            try
            {
                var createinstructor = await _userService.CreateInstructorAsync(InstructorFirstName, InstructorLastName, InstructorEmail, InstructorUsername, InstructorPassword);
                if (createinstructor != null)
                {
                    Instructors.Add(createinstructor);
                    InstructorFirstName = string.Empty;
                    InstructorLastName = string.Empty;
                    InstructorEmail = string.Empty;
                    InstructorUsername = string.Empty;
                    InstructorPassword = string.Empty;
                }
                SuccessMessage = "Instructor created successfully.";
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
                    ErrorMessage = "Please select an instructor to delete.";
                    return;
                }
                var instructorUser = await App.Database.Users.FirstOrDefaultAsync(u => u.InstructorId == SelectedInstructor.InstructorId);
                if(instructorUser == null)
                {
                    ErrorMessage = "Associated user account not found.";
                    return;
                }
                var user = await _userService.DeactivateUserAsync(instructorUser.UserId);
                SelectedInstructor = null;

                SuccessMessage = "Instructor deactivated successfully.";

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
        
    }
}