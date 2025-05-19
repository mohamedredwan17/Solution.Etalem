using Etalem.Models;

namespace Etalem.Data.Repo.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<List<Category>> GetAllAsync();
    }
}
