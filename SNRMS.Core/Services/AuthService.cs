using Microsoft.EntityFrameworkCore;
using SNRMS.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;
using SNRMS.Core.Models;

namespace SNRMS.Core.Services
{
    public class AuthService
    {
        private readonly AppDbContext _dbContext;

        public AuthService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        
        public string HashPassword(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);   
            return hashedPassword;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Username != username)
                return null;

            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {

                SessionManager.Login(user); 
                return user;
            }
            return null;
        }
    }
}
