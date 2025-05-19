using Etalem.Models;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public List<Category> Categories { get; set; }

    public IndexModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetAllCategoriesAsync();
    }

    
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return RedirectToPage();
    }
}
