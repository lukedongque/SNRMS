using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;


        public int? SectionId { get; set; }
        public Section? Section { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public ICollection<StudentRotationHistory> RotationHistory { get; set; } = new List<StudentRotationHistory>();
        public bool IsArchived { get; set; } = false;
    }
}
