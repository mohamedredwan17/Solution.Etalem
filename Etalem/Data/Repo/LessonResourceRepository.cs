using Etalem.Data.Repo.Interfaces;
using Etalem.Models;

namespace Etalem.Data.Repo
{
    public class LessonResourceRepository : BaseRepository<LessonResource>, ILessonResourceRepository
    {
        public LessonResourceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
