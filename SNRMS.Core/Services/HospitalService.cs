using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SNRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<Hospital>> GetAllHospital()
        {
            return await _dbContext.Hospitals.Include(h => h.Stations).ThenInclude(h => h.RotationAssignments).ToListAsync();
        }

        public async Task<Hospital?> DeleteHospitalAsync(int hospitalId)
        {
            var hospital = await _dbContext.Hospitals.Include(h => h.Stations).ThenInclude(h => h.RotationAssignments).FirstOrDefaultAsync(h => h.HospitalId == hospitalId);
            if (hospital == null)
                throw new InvalidOperationException("Hospital not found.");
            //hospital.Stations.SelectMany(s => s.RotationAssignments).ToList().ForEach(ra => ra.StationId = null); to be fixed
            _dbContext.Stations.RemoveRange(hospital.Stations);
            _dbContext.Hospitals.Remove(hospital);
            await _dbContext.SaveChangesAsync();
            return hospital;
        }
    }
}
