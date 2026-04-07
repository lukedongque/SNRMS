using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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
            var exists = await _dbContext.Sections.AnyAsync(s => s.SectionName == name && s.YearLevel == yearlevel);
            if (exists)
                throw new InvalidOperationException("A section with the same name and year level already exists.");

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

            var instructorUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.InstructorId == instructorId);
            if (!instructorUser!.IsActive)
                throw new InvalidOperationException("Instructor account is inactive.");

            var alreadyAssignedSection = await _dbContext.Sections.FirstOrDefaultAsync(s => s.InstructorId == instructorId);
            if (alreadyAssignedSection != null)
                throw new InvalidOperationException("Instructor is already assigned to another section. Unassign them first.");
            var hasInstructorAlready = await _dbContext.Sections.AnyAsync(s => s.SectionId == sectionId && s.InstructorId != null);
                if(hasInstructorAlready)
                    throw new InvalidOperationException("Section has an instructor already. Please unassign the assigned instructor first.");
            section.InstructorId = instructorId;
            await _dbContext.SaveChangesAsync();
            return section;
        }
        public async Task<Section?> UnassignInstructorAsync(int sectionId)
        {
            var section = await _dbContext.Sections.FindAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found.");
            if (section.InstructorId == null)
                throw new InvalidOperationException("No instructor assigned to this section.");
            section.InstructorId = null;
            await _dbContext.SaveChangesAsync();
            return section;
        }

        public async Task<List<Section>> GetAllSectionsAsync()
        {

            return await _dbContext.Sections.AsNoTracking().Include(s => s.Instructor).Include(s => s.Groups).ToListAsync();
        }

        public async Task<List<Section>> GetSectionsByYearLevelAsync(int yearLevel)
        {
            return await _dbContext.Sections.AsNoTracking().Include(s => s.Instructor).Include(s => s.Groups).Where(s => s.YearLevel == yearLevel).ToListAsync();
        }
        public async Task<Section> GetInstructorSectionAsync(int instructorId)
        {
            var section = await _dbContext.Sections.Include(s => s.Groups).FirstOrDefaultAsync(s => s.InstructorId == instructorId);
            if (section == null)
                throw new InvalidOperationException("Instructor does not have an assigned section.");
            return section;
        }
        public async Task<Section?> DeleteSectionAsync(int sectionId)
        {
            var section = await _dbContext.Sections.Include(s => s.Groups).ThenInclude(s => s.Students).FirstOrDefaultAsync(s => s.SectionId == sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found.");
            var hasInstructor = await _dbContext.Sections.AnyAsync(s => s.SectionId == sectionId && s.InstructorId != null);
            if (hasInstructor)
                throw new InvalidOperationException("Cannot delete section with an assigned instructor. Unassign the instructor first.");
            section.Groups.SelectMany(g => g.Students).ToList().ForEach(s => s.GroupId = null); 
            _dbContext.Groups.RemoveRange(section.Groups); 
            _dbContext.Sections.Remove(section);
            await _dbContext.SaveChangesAsync();
            return section;
        }
        public async Task<List<Section>> ImportSectionsFromExcelAsync(string filePath)
        {
            var createdSections = new List<Section>();
            var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // skip header

            foreach (var row in rows)
            {
                var sectionName = row.Cell(1).GetString().Trim();
                var yearLevelStr = row.Cell(2).GetString().Trim();

                if (string.IsNullOrEmpty(sectionName) || !int.TryParse(yearLevelStr, out int yearLevel))
                    continue; // skip invalid rows

                try
                {
                    var section = await CreateSectionAsync(sectionName, yearLevel);
                    if (section != null)
                        createdSections.Add(section);
                }
                catch
                {
                    continue; // skip duplicates
                }
            }
            return createdSections;
        }
    }
}
