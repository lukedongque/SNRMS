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
            var existingUser = await _dbContext.Users.AnyAsync(u => u.Username == employeeId);
            if (existingUser)
                throw new InvalidOperationException("Username already exists.");
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

        public async Task<User?> ChangePasswordAsync(int userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("New password cannot be empty.");
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _dbContext.SaveChangesAsync();
            return user;
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
        }
}
