using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Services
{
    public class SectionService
    {
        private readonly AppDbContext _dbContext;
        public  SectionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
