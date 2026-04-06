using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Models;
using BCrypt.Net;
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

        public DbSet<StudentRotationHistory> StudentRotationHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(new User { 
                UserId = 1, 
                Username = "admin", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin", 
                StudentId = null, 
                InstructorId = null 
            });

            modelBuilder.Entity<Section>()
                .HasOne(s => s.Instructor)
                .WithMany()
                .HasForeignKey(s => s.InstructorId); ;

            modelBuilder.Entity<User>()
                .HasOne(u => u.Instructor)
                .WithOne(i => i.User)
                .HasForeignKey<User>(u => u.InstructorId);


        }
    }
}
