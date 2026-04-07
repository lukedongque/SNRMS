using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Data;
using SNRMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Services
{
    public class HospitalService
    {
        private readonly AppDbContext _dbContext;

        public HospitalService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<Hospital?> CreateHospitalAsync(string name, string address)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Hospital name is required.");
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Hospital address is required.");
            var hospital = new Hospital
            {
                HospitalName = name,
                Address = address
            };
            _dbContext.Hospitals.Add(hospital);
            await _dbContext.SaveChangesAsync();
            return hospital;
        }

        public async Task<Station?> AddStationAsync(int hospitalId, string stationName, int capacity)
        {
            if (string.IsNullOrEmpty(stationName))
                throw new ArgumentException("Station name is required.");
            var hospital = await _dbContext.Hospitals.FindAsync(hospitalId);
            if (hospital == null)
                throw new InvalidOperationException("Hospital not found.");
            var station = new Station
            {
                HospitalId = hospitalId,
                StationName = stationName,
                Capacity = capacity
            };
            _dbContext.Stations.Add(station);
            await _dbContext.SaveChangesAsync();
            return station;
        }

        public async Task<Station?> RemoveStationAsync(int stationId)
        {
            var station = await _dbContext.Stations.Include(s => s.RotationAssignments).FirstOrDefaultAsync(s => s.StationId == stationId);
            if (station == null)
                throw new InvalidOperationException("Station not found.");
            if (station.RotationAssignments.Any())
                throw new InvalidOperationException("Cannot remove station with assigned rotations.");
         
            _dbContext.Stations.Remove(station);
            await _dbContext.SaveChangesAsync();
            return station;
        }

        public async Task<List<Hospital>> GetAllHospitalsAsync()
        {
            return await _dbContext.Hospitals.AsNoTracking().Include(h => h.Stations).ThenInclude(h => h.RotationAssignments).ToListAsync();
        }
        public async Task<List<Station>> GetAllStationsByHospitalIdAsync(int hospitalId)
        {
            var hospital = await _dbContext.Hospitals.Include(h => h.Stations).ThenInclude(h => h.RotationAssignments).FirstOrDefaultAsync(h => h.HospitalId == hospitalId);
            if (hospital == null)
                throw new InvalidOperationException("Hospital not found.");
            return hospital.Stations.ToList();
        }
        public async Task<Hospital?> DeleteHospitalAsync(int hospitalId)
        {
            var hospital = await _dbContext.Hospitals.Include(h => h.Stations).ThenInclude(h => h.RotationAssignments).FirstOrDefaultAsync(h => h.HospitalId == hospitalId);
            if (hospital == null)
                throw new InvalidOperationException("Hospital not found.");
            bool hasAssignments = hospital.Stations.Any(s => s.RotationAssignments.Any());
            if(hasAssignments)  
                throw new InvalidOperationException("Cannot delete hospital with assigned rotations.");
            _dbContext.Stations.RemoveRange(hospital.Stations);
            _dbContext.Hospitals.Remove(hospital);
            await _dbContext.SaveChangesAsync();
            return hospital;
        }
        public async Task<List<Hospital>> ImportHospitalsFromExcelAsync(string filePath)
        {
            var createdHospitals = new List<Hospital>();
            var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var hospitalName = row.Cell(1).GetString().Trim();
                var address = row.Cell(2).GetString().Trim();

                if (string.IsNullOrEmpty(hospitalName))
                    continue;

                try
                {
                    var hospital = await CreateHospitalAsync(hospitalName, address);
                    if (hospital != null)
                        createdHospitals.Add(hospital);
                }
                catch
                {
                    continue;
                }
            }
            return createdHospitals;
        }
    }
}
