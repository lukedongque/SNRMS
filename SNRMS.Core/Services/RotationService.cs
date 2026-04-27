using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Data;
using SNRMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
namespace SNRMS.Core.Services
{
    public class RotationService
    {
        private readonly AppDbContext _dbContext;
        public RotationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RotationAssignment?> CreateRotationAssignmentAsync(int groupId, int stationId, string dayslot, DateOnly startDate, DateOnly endDate, TimeOnly startTime, TimeOnly endTime)
        {
            if (startDate >= endDate)
                throw new InvalidOperationException("Start date must be before end date.");

            var station = await _dbContext.Stations.FindAsync(stationId);
            if (station == null)
                throw new InvalidOperationException("Station not found.");
            var currentAssignments = await _dbContext.RotationAssignments
                .CountAsync(ca => !ca.IsArchived && 
                            ca.StationId == stationId &&
                            ca.DaySlot == dayslot &&
                            startDate <= ca.EndDate && endDate >= ca.StartDate &&
                            startTime < ca.EndTime && endTime > ca.StartTime);
            if (currentAssignments >= station.Capacity)
                throw new InvalidOperationException($"Station capacity is full for the given time slot. Number of assignment on the time slot chosen: {currentAssignments}");
            var groupConflict = await _dbContext.RotationAssignments
                    .AnyAsync(ra => ra.GroupId == groupId &&
                    ra.DaySlot == dayslot &&
                    startDate <= ra.EndDate &&
                    endDate >= ra.StartDate &&
                    startTime < ra.EndTime && endTime > ra.StartTime);
            if (groupConflict)
                throw new InvalidOperationException("Group has another assignment during the same time slot.");

            var rotationAssignment = new RotationAssignment
            {
                GroupId = groupId,
                StationId = stationId,
                StartDate = startDate,
                EndDate = endDate,
                DaySlot = dayslot,
                StartTime = startTime,
                EndTime = endTime
            };
            _dbContext.RotationAssignments.Add(rotationAssignment);
            await _dbContext.SaveChangesAsync();
            var students = await _dbContext.Students.Where(s => s.GroupId == groupId).ToListAsync();
            foreach (var s in students)
            {
                _dbContext.StudentRotationHistories.Add(new StudentRotationHistory
                {
                    StudentId = s.StudentId,
                    RotationAssignmentId = rotationAssignment.RotationAssignmentId,

                });
            }
            await _dbContext.SaveChangesAsync();
            return rotationAssignment;
        }

        public async Task<RotationAssignment?> GetNextRotationAssignment(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student?.GroupId == null) return null;

            return await _dbContext.StudentRotationHistories
                .Where(h => h.StudentId == studentId)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Station).ThenInclude(s => s.Hospital)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Group)
                .Select(h => h.RotationAssignment)
                .Where(r => !r.IsArchived && r.StartDate > today && r.GroupId == student.GroupId)
                .OrderBy(r => r.StartDate)
                .FirstOrDefaultAsync();
        }
        public async Task<RotationAssignment?> GetCurrentRotationAssignmentAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student?.GroupId == null) return null;

            return await _dbContext.StudentRotationHistories
                .Where(h => h.StudentId == studentId)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Station).ThenInclude(s => s.Hospital)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Group)
                .Select(h => h.RotationAssignment)
                .Where(r => !r.IsArchived && r.StartDate <= today && r.EndDate >= today && r.GroupId == student.GroupId)
                .FirstOrDefaultAsync();
        }
        public async Task<List<RotationAssignment>> GetAssignmentBySection(int sectionId)
        {
            var groupIds = await _dbContext.Groups.Where(g => g.SectionId == sectionId)
                .Select(g => g.GroupId)
                .ToListAsync();
            var assignments = await _dbContext.RotationAssignments
                .Where(ra => groupIds.Contains(ra.GroupId) && !ra.IsArchived) 
                .Include(ra => ra.Station)
                    .ThenInclude(s => s.Hospital)
                .Include(ra => ra.Group)
                .ToListAsync();
            if (assignments.Count == 0)
                throw new InvalidOperationException("No rotation assignments found for the section.");
            return assignments;

        }
        public async Task<List<RotationAssignment>> GetStudentAllAssignmentsAsync(int studentId)
        {
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student?.GroupId == null) return new List<RotationAssignment>();

            return await _dbContext.StudentRotationHistories
                .Where(h => h.StudentId == studentId)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Station).ThenInclude(s => s.Hospital)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Group)
                .Select(h => h.RotationAssignment)
                .Where(r => r.GroupId == student.GroupId) 
                .OrderBy(r => r.StartDate)
                .ToListAsync();
        }
        public async Task<List<RotationAssignment>> GetStudentRotationHistoryAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var history = await _dbContext.StudentRotationHistories
                .Where(h => h.StudentId == studentId)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Station)
                    .ThenInclude(s => s.Hospital)
                .Include(h => h.RotationAssignment)
                    .ThenInclude(r => r.Group)
                .Select(h => h.RotationAssignment)
                .Where(r => r.EndDate < today)   
                .OrderByDescending(r => r.EndDate)
                .ToListAsync();

            return history;
        }
        public async Task<RotationAssignment?> DeleteRotationAssignmentAsync(int rotationassignmentId)
        {
            var rotationAssignment = await _dbContext.RotationAssignments
            .FindAsync(rotationassignmentId);
            if (rotationAssignment == null)
                throw new ArgumentException("No rotation assignment found.");

            rotationAssignment.IsArchived = true;  
            await _dbContext.SaveChangesAsync();
            return rotationAssignment;

        }

        public async Task<List<RotationAssignment>> GetAllRotationAssignmentsAsync()
        {
            return await _dbContext.RotationAssignments
                .Include(ra => ra.Station)
                    .ThenInclude(s => s.Hospital)
                .Include(ra => ra.Group)
                .ToListAsync();
        }

        public async Task<List<RotationAssignment>> GetAssignmentByGroupAsync(int groupId)
        {
            var rotationassignments = await _dbContext.RotationAssignments.AsNoTracking().Where(ra => ra.GroupId == groupId && !ra.IsArchived)
                .Include(ra => ra.Group)
                .Include(ra => ra.Station)
                    .ThenInclude(s => s.Hospital)
                .ToListAsync();
            
            return rotationassignments;
        }


    }
}
