using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
    public class AuthService
    {
        private readonly EduTrackContext _context;
        private User? _currentUser;

        public AuthService(EduTrackContext context)
        {
            _context = context;
        }

        public User? CurrentUser => _currentUser;

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsBlocked);

            if (user == null)
                return false;

            if (!VerifyPassword(password, user.PasswordHash))
                return false;

            _currentUser = user;
            return true;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password, UserRole role)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                return false;

            var user = new User
            {
                Username = username,
                Email = email,
                // Хранение пароля в открытом виде по требованию проекта
                PasswordHash = password,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _currentUser = user;
            return true;
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Сравнение простым образом без хэширования
            return password == hash;
        }

        public bool IsAdmin => _currentUser?.Role == UserRole.Administrator;
        public bool IsTeacher => _currentUser?.Role == UserRole.Teacher;
        public bool IsStudent => _currentUser?.Role == UserRole.Student;
    }
}

