using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SNRMS.Core.Services
{
    public class InstructorService
    {
        private readonly AppDbContext _dbContext;
        public InstructorService(AppDbContext dbContex)
        {
            _dbContext = dbContex;
        }

        public async Task<Instructor?> GetInstructorbyIdAsync(int instructorId)
        {
            var instructor = await _dbContext.Instructors.FindAsync(instructorId);
            if (instructor == null)
                throw new InvalidOperationException("Instructor not found.");
            return instructor;
        }
        public async Task<List<Instructor>> GetInstructorbyNameAsync(string firstname, string lastname)
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname))
                throw new ArgumentException("Both first name and last name are required.");
            var instructorsList = await _dbContext.Instructors.Where(i => i.FirstName == firstname && i.LastName == lastname).ToListAsync();
            if(instructorsList.Count == 0)
                throw new InvalidOperationException("No instructors found with the provided name.");
            return instructorsList;
        }

        public async Task<List<Instructor>> GetAllInstructorsAsync()
        {
            var instructorsList = await _dbContext.Instructors
                .Include(i => i.User)
                .ToListAsync();
            if (instructorsList.Count == 0)
                throw new InvalidOperationException("No instructors found in the system.");
            return instructorsList;
        }
        public async Task<Section?> GetInstructorSectionAsync(int instructorId)
        {
            var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(i => i.InstructorId == instructorId);
            if (instructor == null)
                throw new InvalidOperationException("Instructor not found.");
            var section = await _dbContext.Sections.FirstOrDefaultAsync(s => s.InstructorId == instructorId);
            if(section == null)
                throw new InvalidOperationException("Instructor does not have an assigned section.");
            return section;
        }
        public async Task<Instructor?> UpdateInstructorDetailsasync(int instructorid, string firstname, string lastname, string email)
        {
            var instructor = await _dbContext.Instructors.FindAsync(instructorid);
            if (instructor == null)
                throw new InvalidOperationException("Instructor not found.");
            if (!string.IsNullOrEmpty(firstname))
                instructor.FirstName = firstname;
            if (!string.IsNullOrEmpty(lastname))
                instructor.LastName = lastname;
            if (!string.IsNullOrEmpty(email))
                instructor.Email = email;
            await _dbContext.SaveChangesAsync();
            return instructor;
        }


    }
}
