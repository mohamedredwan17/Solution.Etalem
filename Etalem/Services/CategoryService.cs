using Etalem.Data.Repo.Interfaces;
using Etalem.Models;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Etalem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            await _categoryRepository.DeleteAsync(category);
            return true;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public Task<Category?> GetCategoryByIdAsync(int id)
        {
            return _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _categoryRepository.GetByNameAsync(name);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existing = await _categoryRepository.GetByIdAsync(category.Id);
            if (existing == null) return false;
            existing.Name = category.Name;
            await _categoryRepository.UpdateAsync(existing);
            return true;
        }
    }
}
