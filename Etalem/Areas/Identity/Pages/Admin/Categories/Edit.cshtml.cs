using Etalem.Models;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ICategoryService _categoryService;

    [BindProperty]
    public Category Category { get; set; }

    public EditModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();

        Category = category;
        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var updated = await _categoryService.UpdateCategoryAsync(Category);
        if (!updated) return NotFound();

        return RedirectToPage("Index");
    }
}
