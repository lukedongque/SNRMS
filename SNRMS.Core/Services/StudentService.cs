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
            if (user != null)
                _dbContext.Users.Remove(user);
            _dbContext.Students.Remove(student);
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
                    .Where(s => s.GroupId == groupId)
                    .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId) //get student by id
        {
            var student = await _dbContext.Students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
                throw new ArgumentException("Student not found");
            return student;
        }

        public async Task<Student?> TransferStudentAsync(int studentId, int newGroupId) //transfer student to different group
        {
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student == null)
                throw new InvalidOperationException("Student not found.");
            var newGroup = await _dbContext.Groups.FindAsync(newGroupId);
            if (newGroup == null)
                throw new InvalidOperationException("New group not found.");
            if (student.GroupId == newGroupId)
                throw new InvalidOperationException("Student is already in this group.");
            student.GroupId = newGroupId;
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
            return await _dbContext.Students.AsNoTracking().Include(s => s.Group).ToListAsync();
        }

        public async Task<List<Student>> SearchStudentsAsync(string searchTerm) //search student by name or student number
        {
            if (string.IsNullOrEmpty(searchTerm))
                throw new ArgumentException("Search term is required.");
            return await _dbContext.Students.AsNoTracking()
                .Where(s => s.FirstName.Contains(searchTerm) ||
                            s.LastName.Contains(searchTerm) ||
                            s.StudentNumber.Contains(searchTerm))
                .Include(s => s.Group)
                .ToListAsync();
        }
    }
}

    

