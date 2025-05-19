using Etalem.Models;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ICategoryService _categoryService;

    [BindProperty]
    public Category Category { get; set; }

    
    public CreateModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public void OnGet()
    {
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await _categoryService.CreateCategoryAsync(Category);
        return RedirectToPage("Index");
    }
}
