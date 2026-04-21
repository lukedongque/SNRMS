using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class AttendanceRecord
    {
        public int AttendanceRecordId { get; set; }
        public int StudentId { get; set; }
        public int RotationAssignmentId { get; set; }

        public DateOnly DateToday { get; set; }
        public DateTime TimeIn { get; set; }      
        public DateTime? TimeOut { get; set; }   

        public Student Student { get; set; } = null!;
        public RotationAssignment RotationAssignment { get; set; } = null!;
    }
}
