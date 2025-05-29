using Etalem.Data;
using Etalem.Data.Repo.Interfaces;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Etalem.Data.Repo
{
    public class DiscussionRepository : BaseRepository<Discussion>, IDiscussionRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscussionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Discussion> GetByIdAsync(int id)
        {
            return await _context.Discussions
                .Include(d => d.User)
                .Include(d => d.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public override async Task<IEnumerable<Discussion>> GetAllAsync()
        {
            return await _context.Discussions
                .Include(d => d.User)
                .Include(d => d.Replies)
                .ThenInclude(r => r.User)
                .ToListAsync();
        }
    }
}