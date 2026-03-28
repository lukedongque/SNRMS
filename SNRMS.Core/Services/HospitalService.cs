using SNRMS.Core.Data;
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
    }
}
