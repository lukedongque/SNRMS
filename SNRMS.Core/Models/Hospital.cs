using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Hospital
    {
        public int HospitalID { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ICollection<Station> Stations { get; set; } = new List<Station>();
    }
}
