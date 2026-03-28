using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Services
{
    public class InstructorService
    {
        private readonly AppDbContext _dbContext;
        public InstructorService(AppDbContext dbContex) 
        {
            _dbContext = dbContex;
        }
    }
}
