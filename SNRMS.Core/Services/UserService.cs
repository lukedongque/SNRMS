using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Data;
using SNRMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
namespace SNRMS.Core.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Instructor?> CreateInstructorAsync(string firstname, string lastname, string email, string employeeId)
        {
            if (string.IsNullOrEmpty(firstname) ||
                string.IsNullOrEmpty(lastname) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(employeeId))
                throw new ArgumentException("All fields are required.");
            var existingEmployeeId = await _dbContext.Instructors.AnyAsync(i => i.EmployeeId == employeeId);
            if (existingEmployeeId)
                throw new InvalidOperationException("Employee ID already exists");
            var instructor = new Instructor
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                EmployeeId = employeeId
            };
            _dbContext.Instructors.Add(instructor);
            _dbContext.Users.Add(new User
            {
                Username = employeeId,
                Role = "Instructor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Instructor = instructor
            });
            await _dbContext.SaveChangesAsync();
            return instructor;
        }

        public async Task<User?> DeactivateUserAsync(int userId)
        {
            var section = await _dbContext.Sections
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(s => s.Instructor!.User!.UserId==userId);
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");
            if(!user.IsActive)
                throw new InvalidOperationException("User is already inactive.");
            if(user.IsActive && section != null)
                throw new InvalidOperationException("Cannot deactivate user assigned to a section.");
            user.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> ReactivateUserAsync(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");
            if (user.IsActive)
                throw new InvalidOperationException("User is already active.");
            user.IsActive = true;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        // ── CHANGE PASSWORD (requires current password verification) ──────────────
        public async Task<User?> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("All password fields are required.");
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.HasChangedPassword = true;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        // ── ADMIN RESET: INSTRUCTOR password → "user123" ──────────────────────────
        public async Task<Instructor?> ResetInstructorPasswordAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                throw new ArgumentException("Employee ID is required.");
            var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(i => i.EmployeeId == employeeId);
            if (instructor == null)
                throw new InvalidOperationException("No instructor found with that Employee ID.");
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.InstructorId == instructor.InstructorId);
            if (user == null)
                throw new InvalidOperationException("No linked user account found.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123");
            user.HasChangedPassword = false;
            await _dbContext.SaveChangesAsync();
            return instructor;
        }

        // ── ADMIN RESET: STUDENT password → [studentnumber] ──────────────────────
        public async Task<Student?> ResetStudentPasswordAsync(string studentNumber)
        {
            if (string.IsNullOrEmpty(studentNumber))
                throw new ArgumentException("Student Number is required.");
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
            if (student == null)
                throw new InvalidOperationException("No student found with that Student Number.");
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.StudentId == student.StudentId);
            if (user == null)
                throw new InvalidOperationException("No linked user account found.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(studentNumber);
            user.HasChangedPassword = false;
            await _dbContext.SaveChangesAsync();
            return student;
        }

        public async Task<Instructor?> UpdateInstructorAsync(int instructorId, string firstName, string lastName, string email, string employeeId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(employeeId))
                throw new ArgumentException("All fields are required.");

            var instructor = await _dbContext.Instructors.FindAsync(instructorId);
            if (instructor == null)
                throw new InvalidOperationException("Instructor not found.");

            var duplicate = await _dbContext.Instructors
                .AnyAsync(i => i.EmployeeId == employeeId && i.InstructorId != instructorId);
            if (duplicate)
                throw new InvalidOperationException("An instructor with this Employee ID already exists.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.InstructorId == instructorId);

            instructor.FirstName = firstName;
            instructor.LastName = lastName;
            instructor.Email = email;
            instructor.EmployeeId = employeeId;

            if (user != null)
                user.Username = employeeId;

            await _dbContext.SaveChangesAsync();
            return instructor;
        }
    }
}
