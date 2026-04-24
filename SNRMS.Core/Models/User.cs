using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool HasChangedPassword { get; set; } = false;

        public int? StudentId { get; set; }
        public int? InstructorId { get; set; }

        public Student? Student { get; set; }
        public Instructor? Instructor { get; set; }
    }
}
