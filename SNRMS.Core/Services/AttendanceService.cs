using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Data;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;


namespace SNRMS.Core.Services
{
    public class AttendanceService
    {
        private readonly AppDbContext _dbContext;
        public AttendanceService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AttendanceRecord?> MarkAttendanceAsync(int studentId, int rotationAssignmentId)
        {
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student == null)
                throw new InvalidOperationException("Student not found.");
            var rotationAssignment = await _dbContext.RotationAssignments.FindAsync(rotationAssignmentId);
            if (rotationAssignment == null)
                throw new InvalidOperationException("Rotation assignment not found.");
            var alreadyMarked = await HasAttendanceTodayAsync(studentId, rotationAssignmentId);
            if (alreadyMarked)
                throw new InvalidOperationException("Attendance already marked for today.");
            var attendanceRecord = new AttendanceRecord
            {
                StudentId = studentId,
                RotationAssignmentId = rotationAssignmentId,
                DateToday = DateOnly.FromDateTime(DateTime.Now),
                TimeIn = DateTime.Now
            };
            _dbContext.AttendanceRecords.Add(attendanceRecord);
            await _dbContext.SaveChangesAsync();
            return attendanceRecord;
        }

        public async Task<AttendanceRecord> ClockOutAsync(int studentId, int rotationAssignmentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var record = await _dbContext.AttendanceRecords
                .FirstOrDefaultAsync(ar => ar.StudentId == studentId
                                        && ar.RotationAssignmentId == rotationAssignmentId
                                        && ar.DateToday == today);

            if (record == null)
                throw new InvalidOperationException("No attendance record found for today.");

            if (record.TimeOut != null)
                throw new InvalidOperationException("Already clocked out for today.");

            record.TimeOut = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return record;
        }
        public async Task<bool> HasAttendanceTodayAsync(int studentId, int rotationAssignmentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _dbContext.AttendanceRecords
                .AnyAsync(ar => ar.StudentId == studentId
                            && ar.RotationAssignmentId == rotationAssignmentId
                            && ar.DateToday == today);
        }

        public async Task<bool> HasClockedOutTodayAsync(int studentId, int rotationAssignmentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _dbContext.AttendanceRecords
                .AnyAsync(ar => ar.StudentId == studentId
                             && ar.RotationAssignmentId == rotationAssignmentId
                             && ar.DateToday == today
                             && ar.TimeOut != null);
        }

        public async Task<List<AttendanceRecord>> GetStudentAttendanceAsync(int studentId)
        {
            return await _dbContext.AttendanceRecords
                .AsNoTracking()
                .Where(ar => ar.StudentId == studentId)
                .Include(ar => ar.RotationAssignment)
                    .ThenInclude(ra => ra.Station)
                    .ThenInclude(s => s.Hospital)
                .OrderByDescending(ar => ar.DateToday)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetAttendanceByRotationAsync(int rotationAssignmentId)
        {
            return await _dbContext.AttendanceRecords
                .AsNoTracking()
                .Where(ar => ar.RotationAssignmentId == rotationAssignmentId)
                .Include(ar => ar.Student)
                .OrderBy(ar => ar.DateToday)
                    .ThenBy(ar => ar.Student.LastName)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetAttendanceBySectionAndDateAsync(int sectionId, DateOnly date)
        {
            var groupIds = await _dbContext.Groups
                .Where(g => g.SectionId == sectionId && !g.IsArchived)
                .Select(g => g.GroupId)
                .ToListAsync();

            var rotationIds = await _dbContext.RotationAssignments
                .Where(ra => groupIds.Contains(ra.GroupId) && !ra.IsArchived)
                .Select(ra => ra.RotationAssignmentId)
                .ToListAsync();

            return await _dbContext.AttendanceRecords
                .AsNoTracking()
                .Where(a => rotationIds.Contains(a.RotationAssignmentId) && a.DateToday == date)
                .Include(a => a.Student)
                .Include(a => a.RotationAssignment)
                    .ThenInclude(r => r.Station)
                    .ThenInclude(s => s.Hospital)
                .OrderBy(a => a.Student.LastName)
                .ToListAsync();
        }
    }
}