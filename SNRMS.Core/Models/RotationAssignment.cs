using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class RotationAssignment
    {
        public int RotationAssignmentId { get; set; }
        public int GroupId { get; set; }
        public int StationId {  get; set; }
        public string DaySlot { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public  Group Group { get; set; } = null!;
        public  Station Station { get; set; } = null!;
        public bool IsArchived { get; set; } = false;

        public ICollection<StudentRotationHistory> RotationHistories { get; set; } = new List<StudentRotationHistory>();

    }
}
