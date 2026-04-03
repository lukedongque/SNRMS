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
        public partial int YearLevel { get; set; }
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


        //COLLECTIONS-------------------
        [ObservableProperty]
        public partial ObservableCollection<Section> Sections { get; set; } = new ObservableCollection<Section>();
        [ObservableProperty]
        public partial ObservableCollection<Instructor> Instructors { get; set; } = new ObservableCollection<Instructor>();
        [ObservableProperty]
        public partial ObservableCollection<Hospital> Hospitals { get; set; } = new ObservableCollection<Hospital>();

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;
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
            IsLoading = true;
            try
            {
                var createsection = await _sectionService.CreateSectionAsync(SectionName, YearLevel);
                if (createsection != null)
                {
                    Sections.Add(createsection);
                    SectionName = string.Empty;
                    YearLevel = 0;
                }

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
        public async Task AssignInstructorToSectionAsync()
        {
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
        public async Task DeleteSectionAsync()
        {
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
       
    }
}