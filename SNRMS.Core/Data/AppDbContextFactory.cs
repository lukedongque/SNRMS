using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SNRMS.Core.Data
{
    internal class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql("Server=localhost;Database=SNRMS;User=root;Password=Dongque123", ServerVersion.AutoDetect("Server=localhost;Database=SNRMS;User=root;Password=Dongque123"));
            return new AppDbContext(optionsBuilder.Options);

        }
    }
}
