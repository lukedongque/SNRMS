using SNRMS.Core.Data;
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
    }
}
