using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SNRMS.Core.Services
{
    public class SectionService
    {
        private readonly AppDbContext _dbContext;
        public SectionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Section?> CreateSectionAsync(string name,  int yearlevel)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Section name is required.");
            
            var section = new Section
            {
                SectionName = name,
                YearLevel = yearlevel,
            };
            _dbContext.Sections.Add(section);
            await _dbContext.SaveChangesAsync();
            return section;
        }

        public async Task<Section?> AssignInstructorAsync(int sectionId, int instructorId)
        {
            var section = await _dbContext.Sections.FindAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found.");
            var instructor = await _dbContext.Instructors.FindAsync(instructorId);
            if (instructor == null)
                throw new InvalidOperationException("Instructor not found.");
            section.Instructor = instructor;
            await _dbContext.SaveChangesAsync();
            return section;
        }

        public async Task<List<Section>> GetAllSectionAsync()
        {

            return await _dbContext.Sections.Include(s => s.Instructor).Include(s => s.Groups).ToListAsync();
        }

        public async Task<Section?> DeleteSectionAsync(int sectionId)
        {
            var section = await _dbContext.Sections.Include(s => s.Groups).ThenInclude(s => s.Students).FirstOrDefaultAsync(s => s.SectionId == sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found.");
            section.Groups.SelectMany(g => g.Students).ToList().ForEach(s => s.GroupId = null); 
            _dbContext.Groups.RemoveRange(section.Groups); 
            _dbContext.Sections.Remove(section);
            await _dbContext.SaveChangesAsync();
            return section;
        }
    }
}
