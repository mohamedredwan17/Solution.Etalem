using Etalem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Etalem.Controllers
{
    public class BaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnActionExecutionAsync(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context,
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            await base.OnActionExecutionAsync(context, next);
        }
    }
}