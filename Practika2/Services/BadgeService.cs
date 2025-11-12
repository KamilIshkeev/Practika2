using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
    public class BadgeService
    {
        private readonly EduTrackContext _context;

        public BadgeService(EduTrackContext context)
        {
            _context = context;
        }

        public async Task AwardBadgeAsync(int userId, string badgeCode)
        {
            var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Code == badgeCode);
            if (badge == null)
            {
                // Badge doesn't exist, so we can't award it.
                return;
            }

            var userBadge = await _context.UserBadges
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badge.Id);

            if (userBadge == null)
            {
                _context.UserBadges.Add(new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id,
                    AwardedAt = System.DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}
