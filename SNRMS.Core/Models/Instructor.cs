using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class Instructor
    {
        public int InstructorId{ get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int? SectionId { get; set; }
        public Section? Section { get; set; }
    }
}
