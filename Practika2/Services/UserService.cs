using Practika2.Data;
using Practika2.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Practika2.Services
{
    public class UserService
    {
        private readonly EduTrackContext _context;

        public UserService(EduTrackContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(User user, string newPassword)
        {
            // Storing the password in plain text as required by the project.
            user.PasswordHash = newPassword;
            await UpdateUserAsync(user);
        }
    }
}
