using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Services
{
    public class StudentService
    {
        private readonly AppDbContext _dbContext;
        public StudentService(AppDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
    }
}
