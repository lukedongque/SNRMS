using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Station
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public int HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;
        public ICollection<RotationAssignment> RotationAssignments { get; set; } = new List<RotationAssignment>();

    }
}
