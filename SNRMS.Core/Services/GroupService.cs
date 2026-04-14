using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;
namespace SNRMS.Core.Services
{
    public class GroupService
    {
        private readonly AppDbContext _dbContext;

        public GroupService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Group?> CreateGroupAsync(string groupName, int sectionId)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("Group name is required.");
            var hasGroup = await _dbContext.Groups.AnyAsync(g => g.GroupName == groupName && g.SectionId == sectionId);
            if (hasGroup)
                throw new InvalidOperationException("Group name already exists in this section.");
            var section = await _dbContext.Sections.FindAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found.");
            var group = new Group
            {
                GroupName = groupName,
                Section = section
            };
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            return group;
        }

        public async Task<List<Group>> GetGroupBySectionAsync(int sectionId)
        {
            var section = await _dbContext.Sections.FindAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found.");
            return await _dbContext.Groups.AsNoTracking().Include(g => g.Students).Where(g => g.SectionId == sectionId).ToListAsync();
        }
        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            var group = await _dbContext.Groups.AsNoTracking().Include(g => g.Students).FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null)
                throw new InvalidOperationException("Group not found.");
            return group;
        }
        public async Task<Group?> DeleteGroupAsync(int groupId)
        {
            var group = await _dbContext.Groups.Include(g => g.Students).Include(g => g.RotationAssignments).FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null)
                throw new InvalidOperationException("Group not found.");
            bool hasAssignment = group.RotationAssignments.Any(ra=>!ra.IsArchived);
            if (hasAssignment)
                throw new InvalidOperationException("Cannot delete group with existing rotation assignments.");
            group.Students.ToList().ForEach(s => s.GroupId = null); 
            _dbContext.Groups.Remove(group);
            await _dbContext.SaveChangesAsync();
            return group;
        }

        
    }
}
