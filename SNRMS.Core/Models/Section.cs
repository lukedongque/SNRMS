using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Section
    {
        public int SectionID { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int YearLevel { get; set; }

        public int? InstructorID {  get; set; }
        public Instructor? Instructor { get; set; }

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
