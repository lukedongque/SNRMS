using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.Core.Data
{
    public  class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<RotationAssignment> RotationAssignments { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
    }
}
