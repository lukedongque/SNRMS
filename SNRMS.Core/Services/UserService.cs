using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        public UserService(AppDbContext dbContext)
        {
            _dbContext  = dbContext;
        }
    }
}
