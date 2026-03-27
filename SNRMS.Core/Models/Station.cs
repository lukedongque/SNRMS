using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Station
    {
        public int StationID { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public int HospitalID { get; set; }
        public Hospital Hospital { get; set; } = null!;

    }
}
