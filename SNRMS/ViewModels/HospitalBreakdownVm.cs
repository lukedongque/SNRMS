using System.Collections.ObjectModel;

namespace SNRMS.ViewModels
{
    public class HospitalBreakdownVm
    {
        public string HospitalName { get; }
        public string Address { get; }
        public int TotalRotations { get; }
        public ObservableCollection<GroupBreakdownVm> Groups { get; } = new();

        public HospitalBreakdownVm(string hospitalName, string address, int totalRotations)
        {
            HospitalName = hospitalName;
            Address = address;
            TotalRotations = totalRotations;
        }
    }

    public class GroupBreakdownVm
    {
        public string GroupName { get; }
        public string SectionName { get; }
        public int YearLevel { get; }
        public int Count { get; }
        public int MaxValue { get; }

        // e.g. "Year 3  ·  Section A  ·  Group 1"
        public string Label => $"Year {YearLevel}  ·  {SectionName}  ·  {GroupName}";

        public GroupBreakdownVm(string groupName, string sectionName, int yearLevel, int count, int maxValue)
        {
            GroupName = groupName;
            SectionName = sectionName;
            YearLevel = yearLevel;
            Count = count;
            MaxValue = maxValue;
        }
    }
}
