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
    public partial class AdminPanelViewModel : ObservableObject
    {
        private readonly SectionService _sectionService;
        private readonly InstructorService _instructorService;
        private readonly HospitalService _hospitalService;
        private readonly UserService _userService;

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
        public partial string InstructorName { get; set; } = string.Empty;
        [ObservableProperty]
        //HOSPITAL ------------------
        public partial Hospital SelectedHospital { get; set; }
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
        ObservableCollection<Section> Section { get; set; } = new ObservableCollection<Section>();
        ObservableCollection<Instructor> Instructors { get; set; } = new ObservableCollection<Instructor>();
        ObservableCollection<Hospital> Hospitals { get; set; } = new ObservableCollection<Hospital>();

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var sections = await _sectionService.GetAllSectionsAsync();
                Section.Clear();
                foreach (var section in sections)
                    Section.Add(section);
                var instructors = await _instructorService.GetAllInstructorsAsync();
                Instructors.Clear();
                foreach (var instructor in instructors)
                    Instructors.Add(instructor);
                var hospitals = await _hospitalService.GetAllHospital();
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
        public async Task CreateSectionAsync()
        {
            IsLoading = true;
            try
            {
                var createsection = await _sectionService.CreateSectionAsync(SectionName, YearLevel);
                if (createsection != null)
                {
                    Section.Add(createsection);
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
                    var index = Section.IndexOf(SelectedSection);
                    if (index >= 0)
                    {
                        Section[index] = updatedSection;
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
    }
}