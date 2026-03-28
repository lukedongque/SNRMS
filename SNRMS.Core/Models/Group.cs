using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public Section Section { get; set; } = null!;
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<RotationAssignment> RotationAssignments { get; set; } = new List<RotationAssignment>();
    }
}
