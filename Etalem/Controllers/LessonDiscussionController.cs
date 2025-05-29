using Microsoft.AspNetCore.Mvc;
using Etalem.Services;
using Etalem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Etalem.Controllers
{
    [Authorize]
    public class LessonDiscussionController : Controller
    {
        private readonly LessonService _lessonService;
        private readonly UserManager<IdentityUser> _userManager;

        public LessonDiscussionController(LessonService lessonService, UserManager<IdentityUser> userManager)
        {
            _lessonService = lessonService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int LessonId, string Content, int? ParentDiscussionId = null)
        {
            if (string.IsNullOrEmpty(Content))
            {
                TempData["ErrorMessage"] = "Discussion content cannot be empty.";
                return Redirect(Url.Action("LessonDetails", "Enrollment", new { id = LessonId }));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return Redirect(Url.Action("LessonDetails", "Enrollment", new { id = LessonId }));
            }

            var discussion = new Discussion
            {
                LessonId = LessonId,
                UserId = user.Id,
                Content = Content,
                ParentDiscussionId = ParentDiscussionId,
                CreatedAt = DateTime.UtcNow
            };

            await _lessonService.AddDiscussionAsync(discussion);
            TempData["SuccessMessage"] = "Discussion added successfully!";
            return Redirect(Url.Action("LessonDetails", "Enrollment", new { id = LessonId }));
        }
    }
}