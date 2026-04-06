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

        public async Task<Instructor?> CreateInstructorAsync(string firstname, string lastname, string email, string username, string password)
        {
            if (string.IsNullOrEmpty(firstname) ||
                string.IsNullOrEmpty(lastname) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
                throw new ArgumentException("All fields are required.");
            var existingUser = await _dbContext.Users.AnyAsync(u => u.Username == username);
            if (existingUser)
                throw new InvalidOperationException("Username already exists.");
            var instructor = new Instructor
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email
            };
            _dbContext.Instructors.Add(instructor);
            _dbContext.Users.Add(new User
            {
                Username = username,
                Role = "Instructor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
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
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");
            if(!user.IsActive)
                throw new InvalidOperationException("User is already inactive.");
            user.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> ReActivateUserAsync(int userId)
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
        public async Task<List<Instructor>> ImportInstructorsFromExcelAsync(string filePath)
        {
            var createdInstructors = new List<Instructor>();
            var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var firstName = row.Cell(1).GetString().Trim();
                var lastName = row.Cell(2).GetString().Trim();
                var email = row.Cell(3).GetString().Trim();
                var employeeId = row.Cell(4).GetString().Trim();

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(employeeId))
                    continue;

                try
                {
                    var instructor = await CreateInstructorAsync(
                        firstName, lastName, email, employeeId, "user123");
                    if (instructor != null)
                        createdInstructors.Add(instructor);
                }
                catch
                {
                    continue;
                }
            }
            return createdInstructors;
        }
    }
}
