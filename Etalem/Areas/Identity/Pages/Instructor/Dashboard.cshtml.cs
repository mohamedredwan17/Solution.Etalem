using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etalem.Areas.Identity.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
