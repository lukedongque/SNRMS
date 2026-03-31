using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class StudentRotationHistory
    {
        public int StudentRotationHistoryId { get; set; }
        public int StudentId { get; set; }
        public int RotationAssignmentId { get; set; }

        public Student Student { get; set; } = null!;
        public RotationAssignment RotationAssignment { get; set; } = null!;
    }
}
