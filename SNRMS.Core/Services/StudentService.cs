using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SNRMS.Core.Services
{
    public class StudentService
    {
        private readonly AppDbContext _dbContext;
        public StudentService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Student?> CreateStudentAsync(string firstname, string lastname, string email, string studentnumber) //create single student
        {
            if (string.IsNullOrEmpty(firstname) ||
                string.IsNullOrEmpty(lastname) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(studentnumber))
                throw new ArgumentException("All fields are required.");
            var existingUser = await _dbContext.Users.AnyAsync(u => u.Username == studentnumber);
            if (existingUser)
                throw new InvalidOperationException("A user with the same student number already exists.");
            var student = new Student
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                StudentNumber = studentnumber
            };

            var user = new User
            {
                Username = studentnumber,
                Role = "Student",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(studentnumber),
                Student = student
            };
            _dbContext.Students.Add(student);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return student;
        }
        public async Task<Student?> DeleteStudentAsync(int studentId) //delete student by id
        {
            var student = await _dbContext.Students.Include(s => s.Group!).ThenInclude(g => g.RotationAssignments).FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
                throw new InvalidOperationException("Student not found.");
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);

            var hasAssignment = student.Group != null && student.Group.RotationAssignments.Any(ra => !ra.IsArchived);
            if (hasAssignment)
                throw new InvalidOperationException("Cannot delete student with existing rotation assignments.");
            student.IsArchived = true;
            student.GroupId = null;
            if (user != null)
                user.IsActive = false;  // deactivate user account
            await _dbContext.SaveChangesAsync();
            return student;
        }
        public async Task<Student> ReactivateStudentAsync(int studentId)
        {
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student == null) throw new InvalidOperationException("Student not found.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);

            student.IsArchived = false;
            student.GroupId = null; // Ensure they start without a group as requested

            if (user != null)
            {
                user.IsActive = true; // Give them access back
            }

            await _dbContext.SaveChangesAsync();
            return student;
        }

        public async Task<List<Student>> GetStudentsByGroupAsync(int groupId) //get students in a group
        {
            var group = await _dbContext.Groups.FindAsync(groupId);
            if (group == null)
                throw new ArgumentException("Group not found");
            return await _dbContext.Students
                    .Include(s => s.Group) 
                    .AsNoTracking()
                    .Where(s => s.GroupId == groupId && !s.IsArchived)
                    .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId) //get student by id
        {
            var student = await _dbContext.Students
                .AsNoTracking()
                .Include(s => s.Group)
                    .ThenInclude(g => g.Section)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
                throw new ArgumentException("Student not found");
            return student;
        }
        public async Task<Student?> GetStudentByNumberAsync(string studentNumber)
        {
         
            return await _dbContext.Students
                .Include(s => s.Group).ThenInclude(g => g.Section)
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<Student?> TransferStudentAsync(int studentId, int newGroupId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var student = await _dbContext.Students.FindAsync(studentId);
            if (student == null) throw new InvalidOperationException("Student not found.");

            var newGroup = await _dbContext.Groups
                .Include(g => g.RotationAssignments)
                .FirstOrDefaultAsync(g => g.GroupId == newGroupId);

            if (newGroup == null) throw new InvalidOperationException("New group not found.");
            if (student.GroupId == newGroupId) throw new InvalidOperationException("Student is already in this group.");

            // 1. Remove future StudentRotationHistory entries from the OLD group
            //    (keep past/completed ones so rotation history is preserved)
            if (student.GroupId != null)
            {
                var oldGroupRotationIds = await _dbContext.RotationAssignments
                    .Where(ra => ra.GroupId == student.GroupId && ra.EndDate >= today)
                    .Select(ra => ra.RotationAssignmentId)
                    .ToListAsync();

                var staleHistories = await _dbContext.StudentRotationHistories
                    .Where(h => h.StudentId == studentId &&
                                oldGroupRotationIds.Contains(h.RotationAssignmentId))
                    .ToListAsync();

                _dbContext.StudentRotationHistories.RemoveRange(staleHistories);
            }

            // 2. Assign student to new group
            student.GroupId = newGroupId;

            // 3. Catch-up: add history entries for new group's current + future rotations
            var upcomingRotations = newGroup.RotationAssignments
                .Where(ra => !ra.IsArchived && ra.EndDate >= today)
                .ToList();

            foreach (var ra in upcomingRotations)
            {
                var alreadyLinked = await _dbContext.StudentRotationHistories
                    .AnyAsync(h => h.StudentId == studentId &&
                                   h.RotationAssignmentId == ra.RotationAssignmentId);

                if (!alreadyLinked)
                {
                    _dbContext.StudentRotationHistories.Add(new StudentRotationHistory
                    {
                        StudentId = studentId,
                        RotationAssignmentId = ra.RotationAssignmentId
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
            return student;
        }
        public async Task<List<Student>> CreateBulkAccountStudentAsync(List<Student> student)// create bulk student accounts
        {
            foreach (var s in student)
            {
                if (string.IsNullOrEmpty(s.FirstName) ||
                    string.IsNullOrEmpty(s.LastName) ||
                    string.IsNullOrEmpty(s.Email) ||
                    string.IsNullOrEmpty(s.StudentNumber))
                    throw new ArgumentException("All fields are required for student.");
            }


            var createdStudents = new List<Student>();
            foreach (var s in student)
            {
                var newstudent = await CreateStudentAsync(
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.StudentNumber);
                if (newstudent != null)
                    createdStudents.Add(newstudent);

            }
            return createdStudents;
        }

        public async Task<List<Student>> GetAllStudentsAsync() //get all students
        {
            return await _dbContext.Students.AsNoTracking().Include(s => s.Group).Where(s => !s.IsArchived).ToListAsync();
        }

        public async Task<List<Student>> SearchStudentsAsync(string searchTerm) 
        {
            if (string.IsNullOrEmpty(searchTerm))
                throw new ArgumentException("Search term is required.");
            return await _dbContext.Students.AsNoTracking()
                .Where(s => !s.IsArchived && (s.FirstName.Contains(searchTerm) ||
                            s.LastName.Contains(searchTerm) ||
                            s.StudentNumber.Contains(searchTerm)))
                .Include(s => s.Group)
                .ToListAsync();
        }

        public async Task<Student?> UpdateStudentAsync(int studentId, string firstName, string lastName, string email, string studentNumber)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(studentNumber))
                throw new ArgumentException("All fields are required.");

            var student = await _dbContext.Students.FindAsync(studentId);
            if (student == null)
                throw new InvalidOperationException("Student not found.");

            var duplicate = await _dbContext.Students
                .AnyAsync(s => s.StudentNumber == studentNumber && s.StudentId != studentId);
            if (duplicate)
                throw new InvalidOperationException("A student with this student number already exists.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);

            // Capture old student number BEFORE updating — needed for password check
            var oldStudentNumber = student.StudentNumber;

            student.FirstName = firstName;
            student.LastName = lastName;
            student.Email = email;
            student.StudentNumber = studentNumber;

            if (user != null)
            {
                user.Username = studentNumber;

                // Only update password if the student never changed it from the default
                if (!user.HasChangedPassword)
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(studentNumber);
                // else: student set a custom password — leave it untouched
            }

            await _dbContext.SaveChangesAsync();
            return student;
        }
    }
}

    

